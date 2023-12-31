using System;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;

//Useful for UI. Whether the player is, well, a player or an enemy.
public enum PlayerType { PLAYER, ENEMY };

[RequireComponent(typeof(Deck))]
[Serializable]
public class Player : Entity
{
    [Header("Player Info")]
    [SyncVar(hook = nameof(UpdatePlayerName))] public string username; // SyncVar hook to call a command whenever a username changes (like when players load in initially).

    [Header("Portrait")]
    public Sprite portrait; // For the player's icon at the top left of the screen & in the PartyHUD.

    [Header("Deck")]
    public Deck deck;
    public Sprite cardback;
    [SyncVar, HideInInspector] public int tauntCount = 0; // Amount of taunt creatures on your side of the board.

    [Header("Stats")]
    [SyncVar] public int maxMana = 10;
    [SyncVar] public int currentMax = 0;
    [SyncVar] public int _mana = 0;
    [SyncVar] public int mana;
    //{
    //    get { return Mathf.Min(_mana, maxMana); }
    //    set { _mana = Mathf.Clamp(value, 0, maxMana); } // (현재 값,최소 보정값, 최대 보정값)
    //}

    // Quicker access for UI scripts
    [HideInInspector] public static Player localPlayer;
    [HideInInspector] public bool hasEnemy = false; // If we have set an enemy.
    [HideInInspector] public PlayerInfo enemyInfo; // We can't pass a Player class through the Network, but we can pass structs. 
    //We store all our enemy's info in a PlayerInfo struct so we can pass it through the network when needed.
    [HideInInspector] public static GameManager gameManager;
    [SyncVar] public bool firstPlayer = false; // Is it player 1, player 2, etc.

    [Header("Buffs")]
    [SyncVar] public bool smashPotato;

    public List<FieldCard> UICardsList; //필드의 카드를 버튼으로 직접 지목할때
    readonly public SyncList<CardInfo> UICardInfoList = new SyncList<CardInfo>(); //무덤이나 덱 등의 카드를 지목할때
    public override void OnStartLocalPlayer()
    {
        localPlayer = this;
        firstPlayer = isServer;//서버겸 방장이면 firstPlayer

        //Get and update the player's username and stats
        CmdLoadPlayer(PlayerPrefs.GetString("Name"));
        LoadBuildingDeck();//짠 덱을 가져옴
    }

    public void LoadBuildingDeck()
    {
        // '덱빌딩' 씬에서 저장된 Json 데이터를 로드
        string jsonData = GameManager.localPlayerDeck;

        // Json을 리스트로 역직렬화
        CardAndAmountListWrapper wrapper = JsonUtility.FromJson<CardAndAmountListWrapper>(jsonData);

        // 리스트를 배열로 변환하여 Deck에 저장
        deck.startingDeck = wrapper.deckList.ToArray();

        CmdLoadDeck(deck.startingDeck);

    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        //deck.deckList.Callback += deck.OnDeckListChange;
        //deck.hand.Callback += deck.OnHandChange;
        //deck.graveyard.Callback += deck.OnGraveyardChange;
    }

    [Command]
    public void CmdLoadPlayer(string user)
    {
        //Update the player's username, which calls a SyncVar hook.
        //Learn more here: https://mirror-networking.com/docs/Guides/Sync/SyncVarHook.html
        username = user;
    }

    // Update the player's username, as well as the box above the player's head where their name is displayed.
    void UpdatePlayerName(string oldUser, string newUser)
    {
        //Update username
        username = newUser;

        //Update game object's name in editor (only useful for debugging).
        gameObject.name = newUser;
    }

    [Command]
    public void CmdLoadDeck(CardAndAmount[] startingDeck)
    {
        for (int i = 0; i < startingDeck.Length; ++i)
        {
            CardAndAmount card = startingDeck[i];
            for (int v = 0; v < card.amount; ++v)
            {
                if ((card.card is CreatureCard creatureCard && creatureCard.level > 2) || !(card.card is CreatureCard))
                { deck.deckList.Add(card.amount > 0 ? new CardInfo(card.card, 1) : new CardInfo()); }

                else if (card.card is CreatureCard tamaCard && tamaCard.level == 2)
                { deck.babyCard.Add(card.amount > 0 ? new CardInfo(card.card, 1) : new CardInfo()); }
            }
        }

        //섞기 (본 덱)
        for (int j = 0; j < deck.deckList.Count; ++j)
        {
            int rand = UnityEngine.Random.Range(j, deck.deckList.Count);
            CardInfo temp = deck.deckList[j];
            deck.deckList[j] = deck.deckList[rand];
            deck.deckList[rand] = temp;
        }
        //섞기 (디지타마 덱)
        for (int a=0; a<deck.babyCard.Count; ++a)
        {
            int rand = UnityEngine.Random.Range(a, deck.babyCard.Count);
            CardInfo temp = deck.babyCard[a];
            deck.babyCard[a] = deck.babyCard[rand];
            deck.babyCard[rand] = temp;
        }

        //섞은카드 5장 세큐리티로 배분
        for (int i = 0; i < deck.deckList.Count; ++i)
        {
            CardInfo card = deck.deckList[0];//기억하십시오 0번째를 빼면 1번이 다시 0번으로 당겨진다
            if (deck.securityCard.Count < 5)
            {
                deck.securityCard.Add(new CardInfo(card.data, 1));//손으로 카드 보내고
                deck.deckList.Remove(card);//덱 리스트에선 제거
            }
        }

        //손에 섞은카드 차례로 배분
        for (int i =0; i<deck.deckList.Count; ++i) 
        {
            CardInfo card = deck.deckList[0];//기억하십시오 0번째를 빼면 1번이 다시 0번으로 당겨진다
            if (deck.hand.Count < 5)
            { 
                deck.hand.Add(new CardInfo(card.data, 1));//손으로 카드 보내고
                deck.deckList.Remove(card);//덱 리스트에선 제거
            }
        }
    }
    

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        health = gameManager.maxHealth;
        maxMana = gameManager.maxMana;
        deck.deckSize = gameManager.deckSize;
        deck.handSize = gameManager.handSize;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        //Get EnemyInfo as soon as another player connects. Only start updating once our Player has been loaded in properly(username will be set if loaded in).
        if (!hasEnemy && username != "")
        {
            UpdateEnemyInfo();
        }

        if (hasEnemy && isLocalPlayer && gameManager.isGameStart == false)
        {  
            gameManager.StartGame();
        }
    }

    public void UpdateEnemyInfo()
    {
        //Find all Players and add them to the list.
        Player[] onlinePlayers = FindObjectsOfType<Player>();

        //Loop through all online Players(should just be one other Player)
        for (int i =0; i<onlinePlayers.Length; ++i)
        {
            //Make sure the players are loaded properly(we load the usernames first)
            if (onlinePlayers[i].username != "")
            {
                //There should only be one other Player online, so if it's not us then it's the enemy.
                if (onlinePlayers[i] != Player.localPlayer)
                {
                    //Get & Set PlayerInfo from our Enemy's gameObject
                    PlayerInfo currentPlayer = new PlayerInfo(onlinePlayers[i].gameObject);
                    enemyInfo = currentPlayer;
                    hasEnemy = true;
                    enemyInfo.data.casterType = Target.OPPONENT;
                    //Debug.LogError("Player " + username + " Enemy " + enemy.username + " / " + enemyInfo.username); // Used for Debugging
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdDrawDeck(int Count)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (deck.deckList.Count == 0) { return; } // 카드 없으면 리턴

            deck.hand.Add(deck.deckList[0]);
            deck.deckList.RemoveAt(0);
        }

        RpcDrawDeckForTurn(Count);
    }

    [Command(requiresAuthority = false)]
    public void CmdDrawDeckServerOnly(int Count)
    {
        //서버에서 SyncListCard정보만 가져오기용 함수(Rpc사용 안함)
        //드로우 하고 Sepcific까지 추가하려니 서버속도차 때문에 한카드만 두장 추가되게 되서 만듦
        //이걸 이용한 후 CmdDrawSpecificCard(CardInfo card, Player owner, int Amount)에 Amount를 통해 추가한 모든카드 한번에 드로우
        for (int i = 0; i < Count; ++i)
        {
            if (deck.deckList.Count == 0) { return; } // 카드 없으면 리턴

            deck.hand.Add(deck.deckList[0]);
            deck.deckList.RemoveAt(0);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdDrawDeckNotMyTurn(int Count, Player owner)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (deck.deckList.Count == 0) { return; } // 카드 없으면 리턴

            deck.hand.Add(deck.deckList[0]);
            deck.deckList.RemoveAt(0);
        }

        RpcDrawDeckForPlayer(Count, owner);
    }

    [Command(requiresAuthority = false)]
    public void CmdDrawSpecificCard(CardInfo card, Player owner)
    {
        deck.hand.Add(card);

        RpcDrawDeckForPlayer(1 , owner);//상대의 시큐리티 오픈해서 상대에게 넣는것이기에 내것이 아니라 false
    }

    [Command(requiresAuthority = false)]
    public void CmdDrawSpecificCard(CardInfo card, Player owner, int Amount)
    {
        // Amount == 이 특정카드와 그 전에 드로우해서 추가한 총 카드 장 수
        deck.hand.Add(card);

        RpcDrawDeckForPlayer(Amount, owner);//서버 통신 속도문제로 SyncList만 서버에서 가져와놓고 여기서 최종 추가된 모든 SyncList카드 손에 추가하기
    }

    [Command(requiresAuthority = false)]
    public void CmdAddBuff(Buffs buff)
    {
        buffs.Add(buff);
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeSomeThing(Buffs buff, bool isStart)
    {
        if (isStart) 
        {
            smashPotato = buff.smashPotato;
        }

        else
        {
            smashPotato = !buff.smashPotato;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveBuff(int index)
    {
        buffs.RemoveAt(index);
        Debug.Log("버프 제거 완료");
    }

    [ClientRpc]
    private void RpcDrawDeckForTurn(int Count)
    {
        //for (int i = 0; i < deck.hand.Count; ++i) { Debug.Log(deck.hand[i].data.cardName); }
        if (gameManager.isOurTurn)
        {
            PlayerHand playerHand = Player.gameManager.playerHand;
            for (int i = 0; i < Count; i++)
            {
                playerHand.AddCard(deck.hand.Count - Count + i);
                //Debug.Log(deck.hand[deck.hand.Count - Count + i].data.cardName +" "+ (deck.hand.Count - Count + i).ToString());
            }

        }
    }

    [ClientRpc]
    private void RpcDrawDeckForPlayer(int Count, Player owner)//isMine == 시전중인 player에게 주냐? 아니면 상대 player에게 주냐?
    {
        //for (int i = 0; i < deck.hand.Count; ++i) { Debug.Log(deck.hand[i].data.cardName); }
        if (Player.localPlayer== owner)
        {
            PlayerHand playerHand = Player.gameManager.playerHand;
            for (int i = 0; i < Count; i++)
            {
                playerHand.AddCard(deck.hand.Count - Count + i);
                Debug.Log(deck.hand[deck.hand.Count - Count + i].data.cardName + " " + (deck.hand.Count - Count + i).ToString());
            }

        }
    }

    [ClientRpc]
    public void RPCGetMyFieldCard(Player owner)
    {
        if (owner == Player.localPlayer)
        {
            int childCount = Player.gameManager.playerField.content.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                FieldCard card = Player.gameManager.playerField.content.GetChild(i).GetComponent<FieldCard>();

                if (card.casterType == Target.FRIENDLIES)
                {
                    Debug.Log("필드카드 동료 목록 " + card.card.name);
                    //나중에 해당 카드를 선택할수 있는 함수를 추가
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSetActiveBlockPanel(Player owner, bool isMyList)
    {
        RpcSetActiveBlockPanel(owner, isMyList);
    }

    [ClientRpc]
    public void RpcSetActiveBlockPanel(Player owner, bool isMyList)
    {
        if (owner == Player.localPlayer)
        {
            Player.gameManager.blockPanel.SetActive(true);
            //UICardsList = new List<FieldCard>();
            UICardsList.Clear();
            if(isMyList)
            {
                //나의 블록 카드 리스트를 보일거면
                int cardCount = Player.gameManager.playerField.content.childCount;
                for (int i = 0; i < cardCount; ++i)
                {
                    FieldCard card = Player.gameManager.playerField.content.GetChild(i).GetComponent<FieldCard>();

                    if (card.isUpperMostCard && card.card.data is CreatureCard creature && creature.hasBlocker)
                    {
                        UICardsList.Add(card);

                        for (int j = UICardsList.Count; j < Player.gameManager.blockButtonImage.Count; ++j)
                        {
                            //사용하지 않는 버튼 비활성화
                            Player.gameManager.blockButtonImage[j].gameObject.SetActive(false);
                        }

                        for (int j = 0; j < UICardsList.Count; ++j)
                        {
                            //사용하는 버튼 활성화
                            Player.gameManager.blockButtonImage[j].sprite = creature.image;
                            Player.gameManager.blockButtonImage[j].gameObject.SetActive(true);
                        }
                        Player.gameManager.attackerImage.sprite = ((FieldCard)Player.gameManager.caster).card.data.image;
                        Player.gameManager.targetImage.sprite = ((FieldCard)Player.gameManager.target).card.data.image;
                    }
                }
            }
            
            else
            {
                //상대 블록 카드 리스트를 나에게 보일거면
                int cardCount = Player.gameManager.enemyField.content.childCount;
                for(int i =0; i< cardCount; ++i)
                {
                    FieldCard card = Player.gameManager.enemyField.content.GetChild(i).GetComponent<FieldCard>();

                    if (card.isUpperMostCard && card.card.data is CreatureCard creatureCard && creatureCard.hasBlocker)
                    {
                        UICardsList.Add(card);

                        for (int j = UICardsList.Count; j < Player.gameManager.blockButtonImage.Count; ++j)
                        {
                            //사용하지 않는 버튼 비활성화
                            Player.gameManager.blockButtonImage[j].gameObject.SetActive(false);
                        }

                        for (int j = 0; j < UICardsList.Count; ++j)
                        {
                            //사용하는 버튼 활성화
                            Player.gameManager.blockButtonImage[j].sprite = creatureCard.image;
                            Player.gameManager.blockButtonImage[j].gameObject.SetActive(true);
                        }
                        Player.gameManager.attackerImage.sprite = ((FieldCard)Player.gameManager.caster).card.data.image;
                        Player.gameManager.targetImage.sprite = ((FieldCard)Player.gameManager.target).card.data.image;
                    }
                }
                
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSetActiveDestroyPanel(Player owner)
    {
        RpcSetActiveDestroyPanel(owner);
    }

    [ClientRpc]
    public void RpcSetActiveDestroyPanel(Player owner)
    {
        if (owner == Player.localPlayer)
        {
            Player.gameManager.destroyPanel.SetActive(true);
            //UICardsList = new List<FieldCard>();

            //상대 블록 카드 리스트를 나에게 보일거면
            //int cardCount = Player.gameManager.enemyField.content.childCount;
            //for (int i = 0; i < cardCount; ++i)
            //{
            //    FieldCard card = Player.gameManager.enemyField.content.GetChild(i).GetComponent<FieldCard>();
            //
            //    if (card.isUpperMostCard && card.card.data is CreatureCard creatureCard)
                {
                    //hasSomething은 블록,관통,재밍 등등의 특성 대상자
                    //UICardsList.Add(card);

                    for (int j = UICardsList.Count; j < Player.gameManager.destroyButtonImage.Count; ++j)
                    {
                        //사용하지 않는 버튼 비활성화
                        Player.gameManager.destroyButtonImage[j].gameObject.SetActive(false);
                    }

                    for (int j = 0; j < UICardsList.Count; ++j)
                    {
                        //사용하는 버튼 활성화
                        Player.gameManager.destroyButtonImage[j].sprite = UICardsList[j].card.image;
                        Player.gameManager.destroyButtonImage[j].gameObject.SetActive(true);
                    }
                  
                }
            //}
        }

    }

    [Command(requiresAuthority = false)]
    public void CmdSetActiveRevivePanel(Player owner, bool active)
    {
        RpcSetActiveRevivePanel(owner,active);
    }

    [ClientRpc]
    public void RpcSetActiveRevivePanel(Player owner, bool active)
    {
        for(int i =0; i<8; ++i) 
        {
            Player.gameManager.reviveUIImage[i].gameObject.SetActive(false);
        }

        if (owner == Player.localPlayer)
        {
            Player.gameManager.revivePanel.SetActive(active);
            
            for (int j = UICardInfoList.Count; j < Player.gameManager.reviveButtonImage.Count; ++j)
            {
                //사용하지 않는 버튼 비활성화
                Player.gameManager.reviveButtonImage[j].gameObject.SetActive(false);
            }

            for (int j = 0; j < UICardInfoList.Count; ++j)
            {
                //사용하는 버튼 활성화
                Player.gameManager.reviveButtonImage[j].sprite = UICardInfoList[j].image;
                Player.gameManager.reviveButtonImage[j].gameObject.SetActive(true);
            }

        }
        else
        {
            //상대가 보는 패널
            Player.gameManager.revivePanel.SetActive(active);
            for(int i =0; i<8; ++i)
            {
                //구경꾼은 버튼을 전부 끈다
                Player.gameManager.reviveButtonImage[i].gameObject.SetActive(false);
            }

            for (int j = 0; j < UICardInfoList.Count; ++j)
            {
                Player.gameManager.reviveUIImage[j].sprite = UICardInfoList[j].image;
                Player.gameManager.reviveUIImage[j].gameObject.SetActive(true);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSetActivePickUpPanel(Player owner, bool active)
    {
        RpcSetActivePickUpPanel(owner,active);
    }

    [ClientRpc]
    public void RpcSetActivePickUpPanel(Player owner, bool active)
    {
        for (int i = 0; i < 8; ++i)
        {
            Player.gameManager.pickUpUIImage[i].gameObject.SetActive(false);
        }

        if (owner == Player.localPlayer)
        {
            Player.gameManager.pickUpPanel.SetActive(active);

            for (int j = UICardInfoList.Count; j < Player.gameManager.pickUpButtonImage.Count; ++j)
            {
                //사용하지 않는 버튼 비활성화
                Player.gameManager.pickUpButtonImage[j].gameObject.SetActive(false);
            }

            for (int j = 0; j < UICardInfoList.Count; ++j)
            {
                //사용하는 버튼 활성화
                Player.gameManager.pickUpButtonImage[j].sprite = UICardInfoList[j].image;
                Player.gameManager.pickUpButtonImage[j].gameObject.SetActive(true);
            }

        }
        else
        {
            //상대가 보는 패널
            Player.gameManager.pickUpPanel.SetActive(active);
            for (int i = 0; i < 8; ++i)
            {
                //구경꾼은 버튼을 전부 끈다
                Player.gameManager.pickUpButtonImage[i].gameObject.SetActive(false);
            }

            for (int j = 0; j < UICardInfoList.Count; ++j)
            {
                Player.gameManager.pickUpUIImage[j].sprite = UICardInfoList[j].image;
                Player.gameManager.pickUpUIImage[j].gameObject.SetActive(true);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveGraveyard(CardInfo card)
    {
        deck.graveyard.Remove(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdAddUICardInfo(CardInfo card)
    {
        UICardInfoList.Add(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdAddUICardInfoAndRemoveDeckList()
    {
        //싱크를 위해 한번에 순서대로 처리하는 함수
        if(deck.deckList.Count>0)
        {
            UICardInfoList.Add(deck.deckList[0]);
            CmdRemoveDeckList(0);
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdClearUICardInfo()
    {
        UICardInfoList.Clear();
    }
    [Command(requiresAuthority =false)]
    public void CmdRemoveDeckList(int index)
    {
        deck.deckList.RemoveAt(index);
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveDeckList(CardInfo card)
    {
        deck.deckList.Remove(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveUICardInfo(CardInfo card)
    {
        UICardInfoList.Remove(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdAddDeckList(CardInfo card)
    {
        deck.deckList.Add(card);
    }
    public bool IsOurTurn() => gameManager.isOurTurn;
}