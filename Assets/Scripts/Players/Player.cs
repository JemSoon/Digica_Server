using System;
using UnityEngine;
using Mirror;

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
    //    set { _mana = Mathf.Clamp(value, 0, maxMana); } // (���� ��,�ּ� ������, �ִ� ������)
    //}

    // Quicker access for UI scripts
    [HideInInspector] public static Player localPlayer;
    [HideInInspector] public bool hasEnemy = false; // If we have set an enemy.
    [HideInInspector] public PlayerInfo enemyInfo; // We can't pass a Player class through the Network, but we can pass structs. 
    //We store all our enemy's info in a PlayerInfo struct so we can pass it through the network when needed.
    [HideInInspector] public static GameManager gameManager;
    [SyncVar] public bool firstPlayer = false; // Is it player 1, player 2, etc.

    public override void OnStartLocalPlayer()
    {
        localPlayer = this;
        firstPlayer = isServer;//������ �����̸� firstPlayer

        //Get and update the player's username and stats
        CmdLoadPlayer(PlayerPrefs.GetString("Name"));
        LoadBuildingDeck();//§ ���� ������
        //CmdLoadDeck();
    }

    public void LoadBuildingDeck()
    {
        // '������' ������ ����� Json �����͸� �ε�
        string jsonData = GameManager.localPlayerDeck;

        // Json�� ����Ʈ�� ������ȭ
        CardAndAmountListWrapper wrapper = JsonUtility.FromJson<CardAndAmountListWrapper>(jsonData);

        // ����Ʈ�� �迭�� ��ȯ�Ͽ� Deck�� ����
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
        Debug.Log("�� �ε� ��");

        for (int i = 0; i < startingDeck.Length; ++i)
        {
            CardAndAmount card = startingDeck[i];
            for (int v = 0; v < card.amount; ++v)
            {
                deck.deckList.Add(card.amount > 0 ? new CardInfo(card.card, 1) : new CardInfo());
                
                //if (deck.hand.Count < 7) 
                //{ 
                //    deck.hand.Add(new CardInfo(card.card, 1)); 
                //}

            }
        }

        //����
        for (int j = 0; j < deck.deckList.Count; ++j)
        {
            int rand = UnityEngine.Random.Range(j, deck.deckList.Count);
            CardInfo temp = deck.deckList[j];
            deck.deckList[j] = deck.deckList[rand];
            deck.deckList[rand] = temp;
        }

        
        //�տ� ����ī�� ���ʷ� ���
        for(int i =0; i<deck.deckList.Count; ++i) 
        {
            CardInfo card = deck.deckList[0];//����Ͻʽÿ� 0��°�� ���� 1���� �ٽ� 0������ �������
            if (deck.hand.Count < 7)
            { 
                deck.hand.Add(new CardInfo(card.data, 1));//������ ī�� ������
                deck.deckList.Remove(card);//�� ����Ʈ���� ����
            }
        }
        
        if (deck.hand.Count == 7)
        {
            //deck.hand.Shuffle();
            
            for (int i = 0; i < deck.deckList.Count; ++i)
            {
                Debug.Log("��ü �� "+i.ToString() + deck.deckList[i].name);
                
            }
            for(int i =0; i<deck.hand.Count; ++i)
            {
                Debug.Log("�� ī�� "+i.ToString() + deck.hand[i].name);
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

            //CmdLoadEnemyDeck();            
            gameManager.StartGame();
        }
    }

    public void UpdateEnemyInfo()
    {
        //Find all Players and add them to the list.
        Player[] onlinePlayers = FindObjectsOfType<Player>();

        //Loop through all online Players(should just be one other Player)
        foreach (Player players in onlinePlayers)
        {
            //Make sure the players are loaded properly(we load the usernames first)
            if (players.username != "")
            {
                //There should only be one other Player online, so if it's not us then it's the enemy.
                if (players != this)
                {
                    //Get & Set PlayerInfo from our Enemy's gameObject
                    PlayerInfo currentPlayer = new PlayerInfo(players.gameObject);
                    enemyInfo = currentPlayer;
                    hasEnemy = true;
                    enemyInfo.data.casterType = Target.OPPONENT;
                    //Debug.LogError("Player " + username + " Enemy " + enemy.username + " / " + enemyInfo.username); // Used for Debugging
                }
            }
        }
    }
    
    public void PlayerDraw(int Count)
    {
        PlayerHand playerHand = Player.gameManager.playerHand;
        
        CmdDrawDeck(Count);
        for(int i =0; i<Count; i++)
        {
            playerHand.AddCard(deck.hand.Count - 1);
        }
    }

    [Command]
    public void CmdDrawDeck(int Count)
    {
        for (int i = 0; i < Count; ++i)
        {
            deck.hand.Add(deck.deckList[0]);
            deck.deckList.RemoveAt(0);
        }
    }

    public bool IsOurTurn() => gameManager.isOurTurn;
}