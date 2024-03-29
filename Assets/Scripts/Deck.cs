using UnityEngine;
using Mirror;
using System.Collections;
using Unity.VisualScripting;
public class Deck : NetworkBehaviour
{

    [Header("Player")]
    public Player player;
    [HideInInspector] public int deckSize = 50;
    [HideInInspector] public int handSize = 7;//나중에 수정 요망

    [Header("Decks")]
    readonly public SyncListCard deckList = new SyncListCard(); // DeckList used during the match. Contains all cards in the deck. This is where we'll be drawing card froms.
    readonly public SyncListCard graveyard = new SyncListCard(); // Cards in player graveyard.
    readonly public SyncListCard hand = new SyncListCard(); // Cards in player's hand during the match.
    readonly public SyncListCard babyCard = new SyncListCard();

    [Header("Battlefield")]
    readonly public SyncListCard playerField = new SyncListCard(); // Field where we summon creatures.

    [Header("SecurityCard")]
    readonly public SyncListCard securityCard = new SyncListCard();

    [Header("Starting Deck")]
    public CardAndAmount[] startingDeck;

    [HideInInspector] public bool spawnInitialCards = true;

    //public void OnDeckListChange(SyncListCard.Operation op, int index, CardInfo oldCard, CardInfo newCard)
    //{
    //    UpdateDeck(index, 1, newCard);
    //}

    //public void OnHandChange(SyncListCard.Operation op, int index, CardInfo oldCard, CardInfo newCard)
    //{
    //    UpdateDeck(index, 2, newCard);
    //}

    //public void OnGraveyardChange(SyncListCard.Operation op, int index, CardInfo oldCard, CardInfo newCard)
    //{
    //    UpdateDeck(index, 3, newCard);
    //}

    public void UpdateDeck(int index, int type, CardInfo newCard)
    {
        // Deck List
        if (type == 1) deckList[index] = newCard;

        // Hand
        if (type == 2) hand[index] = newCard;

        // Gaveyard
        if (type == 3) graveyard[index] = newCard;

    }


    ///////////////
    public bool CanPlayCard(int manaCost)
    {
        if (player.isServer)
        {
            if (MemoryChecker.Inst.memory - manaCost >= -10 && player.health > 0)
            { return true; }
            else
            { return false; }
        }
        else
        {
            if (MemoryChecker.Inst.memory + manaCost <= 10 && player.health > 0)
            { return true; }
            else
            { return false; }
        }
    }

    public void DrawCard(int amount)
    {
        PlayerHand playerHand = Player.gameManager.playerHand;
        for (int i = 0; i < amount; ++i)
        {
            int index = i;
            playerHand.AddCard(index);
        }
        spawnInitialCards = false;
    }

    [Command]
    public void CmdPlayCard(CardInfo card, int index, Player owner)
    {
        //일반 필드 출전은 스펠카드인지 크리쳐카드인지 구분해야함

        CreatureCard creature = null; 
        SpellCard spellCard = null;

        if(card.data is CreatureCard)
        { 
            creature = (CreatureCard)card.data;

            GameObject boardCard = Instantiate(creature.cardPrefab.gameObject);
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.card = new CardInfo(card.data); // Save Card Info so we can re-access it later if we need to.
                                                    //newCard.cardName.text = card.name;
            newCard.health = creature.health;
            newCard.strength = creature.strength;
            newCard.image.sprite = card.image;
            newCard.image.color = Color.white;
            newCard.player = owner;
            //newCard.player.deck.playerField.Add(card);//내 필드 카드 목록에 추가

            // If creature has charge, reduce waitTurn to 0 so they can attack right away.
            if (creature.hasCharge) newCard.waitTurn = 0;

            // Update the Card Info that appears when hovering
            newCard.cardHover.UpdateFieldCardInfo(card);

            // Spawn it
            NetworkServer.Spawn(boardCard);

            // Remove card from hand
            hand.RemoveAt(index);

            if (isServer) RpcPlayCard(boardCard, index);
        }
        
        else if(card.data is SpellCard)
        { 
            spellCard = (SpellCard)card.data;

            GameObject boardCard = Instantiate(spellCard.cardPrefab.gameObject);
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.card = new CardInfo(card.data); // Save Card Info so we can re-access it later if we need to.
                                                    //newCard.cardName.text = card.name;
            newCard.image.sprite = card.image;
            newCard.image.color = Color.white;
            newCard.player = owner;
            //newCard.player.deck.playerField.Add(card);//내 필드 카드 목록에 추가

            // Update the Card Info that appears when hovering
            newCard.cardHover.UpdateFieldCardInfo(card);

            // Spawn it
            NetworkServer.Spawn(boardCard);

            // Remove card from hand
            hand.RemoveAt(index);

            if (isServer) RpcPlayCard(boardCard, index);

            //spellCard.AppearSpellCard(owner);//스펠카드 필드 스폰시 바로 카드효과 실행시킴 서순!! 서순!! 서순!! RpcPlayCard에서 인덱스 정렬함!! 서순!! 하루 날림!!

            if (!spellCard.isTamer)
            {
                newCard.player.deck.graveyard.Add(newCard.card);//발동 직후 무덤으로
            }
        }
    }

    [Command]
    public void CmdPlayEvoCard(CardInfo card, int index, Player owner, FieldCard underCard)
    {
        CreatureCard creature = (CreatureCard)card.data;
        GameObject boardCard = Instantiate(creature.cardPrefab.gameObject);
        FieldCard newCard = boardCard.GetComponent<FieldCard>();
        newCard.card = new CardInfo(card.data); // Save Card Info so we can re-access it later if we need to.
        //newCard.cardName.text = card.name;
        newCard.health = creature.health;
        newCard.strength = creature.strength;
        newCard.image.sprite = card.image;
        newCard.image.color = Color.white;
        newCard.player = owner;

        newCard.underCard = underCard;
        underCard.upperCard = newCard;

        newCard.waitTurn = underCard.waitTurn;
        newCard.evoCount = underCard.evoCount + 1;

        // If creature has charge, reduce waitTurn to 0 so they can attack right away.
        if (creature.hasCharge) newCard.waitTurn = 0;

        // Update the Card Info that appears when hovering
        newCard.cardHover.UpdateFieldCardInfo(card);

        // Spawn it
        NetworkServer.Spawn(boardCard);

        if (underCard.GetComponent<RectTransform>().rotation == Quaternion.Euler(0, 0, -90))
        {
            //만약 카드가 레스트 상태였으면 이전카드는 세우고 최상단 카드를 돌려놓기
            underCard.CmdRotation(underCard, Quaternion.Euler(0, 0, 0));
            newCard.CmdRotation(newCard, Quaternion.Euler(0, 0, -90));
        }

        if(underCard.buffs.Count > 0)
        {
            //아래가 될 카드가 버프가 있다면
            for(int i = underCard.buffs.Count-1; i>=0; i--)
            {
                //전부 버프를 빼고 제거
                underCard.CmdChangeSomeThing(underCard.buffs[i], false);
                underCard.CmdRemoveBuff(i);
            } 
        }

        // Remove card from hand
        hand.RemoveAt(index);

        if (isServer) RpcPlayEvoCard(boardCard, index, underCard);
    }

    [Command]
    public void CmdPlayTamaCard(CardInfo card, Player owner)
    {
        CreatureCard creature = (CreatureCard)card.data;
        GameObject boardCard = Instantiate(creature.cardPrefab.gameObject);
        FieldCard newCard = boardCard.GetComponent<FieldCard>();
        newCard.card = new CardInfo(card.data); // Save Card Info so we can re-access it later if we need to.
        //newCard.cardName.text = card.name;
        newCard.health = creature.health;
        newCard.strength = creature.strength;
        newCard.image.sprite = card.image;
        newCard.image.color = Color.white;
        newCard.player = owner;

        // If creature has charge, reduce waitTurn to 0 so they can attack right away.
        if (creature.hasCharge) newCard.waitTurn = 0;

        // Update the Card Info that appears when hovering
        newCard.cardHover.UpdateFieldCardInfo(card);

        // Spawn it
        NetworkServer.Spawn(boardCard);

        // Remove card from hand
        babyCard.RemoveAt(0);

        if (isServer) RpcPlayTamaCard(boardCard);
    }

    [Command]
    public void CmdPlayEvoTamaCard(CardInfo card, int index, Player owner, FieldCard underCard)
    {
        CreatureCard creature = (CreatureCard)card.data;
        GameObject boardCard = Instantiate(creature.cardPrefab.gameObject);
        FieldCard newCard = boardCard.GetComponent<FieldCard>();
        newCard.card = new CardInfo(card.data); // Save Card Info so we can re-access it later if we need to.
        //newCard.cardName.text = card.name;
        newCard.health = creature.health;
        newCard.strength = creature.strength;
        newCard.image.sprite = card.image;
        newCard.image.color = Color.white;
        newCard.player = owner;

        newCard.underCard = underCard;
        underCard.upperCard = newCard;

        newCard.waitTurn = underCard.waitTurn;
        newCard.evoCount = underCard.evoCount + 1;

        // If creature has charge, reduce waitTurn to 0 so they can attack right away.
        if (creature.hasCharge) newCard.waitTurn = 0;

        // Update the Card Info that appears when hovering
        newCard.cardHover.UpdateFieldCardInfo(card);

        // Spawn it
        NetworkServer.Spawn(boardCard);

        if(underCard.buffs.Count>0)
        {
            for(int i =underCard.buffs.Count-1; i>=0; i--)
            {
                underCard.CmdChangeSomeThing(underCard.buffs[i], false);
                underCard.CmdRemoveBuff(i);
            }
        }

        // Remove card from hand
        hand.RemoveAt(index);

        if (isServer) RpcPlayEvoTamaCard(boardCard, index, underCard);
    }

    [Command(requiresAuthority = false)] //권한 없애줘야 실행됨 (상대 클라이언트의 세큐리티 카드를 꺼내는 것이기 때문)
    public void CmdPlaySecurityCard(CardInfo card, Player owner, Entity attacker)
    {
        // 이 카드는 크리쳐 카드일지 옵션,테이머 카드일지 알 수 없다
        Debug.Log("지금 세큐리티 파기말고 오픈 시작됩니다");
        if (card.data is CreatureCard)
        {
            CreatureCard creature = (CreatureCard)card.data;
            GameObject boardCard = Instantiate(creature.cardPrefab.gameObject);
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.card = new CardInfo(card.data); // Save Card Info so we can re-access it later if we need to.
                                                    //newCard.cardName.text = card.name;
            newCard.isSecurity = true;
            newCard.health = creature.health;
            newCard.strength = creature.strength;
            newCard.image.sprite = card.image;
            newCard.image.color = Color.white;
            newCard.player = owner;

            if (creature.hasCharge) newCard.waitTurn = 0;

            // Update the Card Info that appears when hovering
            newCard.cardHover.UpdateFieldCardInfo(card);

            // Spawn it
            NetworkServer.Spawn(boardCard);
            //Debug.Log("세큐리티 서버 스폰");
            // 대상자의 세큐리티 카드를 스폰시켰으니 제거
            Debug.Log("세큐리티 오픈의 0번째 카드 : "+owner.deck.securityCard[0].data.cardName);
            owner.deck.securityCard.RemoveAt(0);

            if (isServer) RpcPlaySecurityCard(boardCard, owner, attacker);
        }
        else if (card.data is SpellCard)
        {
            SpellCard spellCard = (SpellCard)card.data;
            GameObject boardCard = Instantiate(spellCard.cardPrefab.gameObject);
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.card = new CardInfo(card.data); // Save Card Info so we can re-access it later if we need to.
                                                    //newCard.cardName.text = card.name;
            newCard.isSecurity = true;
            newCard.image.sprite = card.image;
            newCard.image.color = Color.white;
            newCard.player = owner;

            // Update the Card Info that appears when hovering
            newCard.cardHover.UpdateFieldCardInfo(card);

            // Spawn it
            NetworkServer.Spawn(boardCard);

            // 대상자의 세큐리티 카드를 스폰시켰으니 제거
            owner.deck.securityCard.RemoveAt(0);

            if (isServer) RpcPlaySecurityCard(boardCard, owner, attacker);

            //spellCard.AppearSecuritySpellCard(owner);
        }
    }

    [Command(requiresAuthority = false)] 
    public void CmdBreakSecurityCard(CardInfo card, Player owner)
    {
        // 이 카드는 크리쳐 카드일지 옵션,테이머 카드일지 알 수 없다
        // 전투없이 바로 파기될때 쓰는 함수 스펠카드 효과도 발동 안함!
        if (card.data is CreatureCard)
        {
            CreatureCard creature = (CreatureCard)card.data;
            GameObject boardCard = Instantiate(creature.cardPrefab.gameObject);
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.card = new CardInfo(card.data); // Save Card Info so we can re-access it later if we need to.
                                                    //newCard.cardName.text = card.name;
            newCard.isSecurity = true;
            newCard.health = creature.health;
            newCard.strength = creature.strength;
            newCard.image.sprite = card.image;
            newCard.image.color = Color.white;
            newCard.player = owner;

            if (creature.hasCharge) newCard.waitTurn = 0;

            // Update the Card Info that appears when hovering
            newCard.cardHover.UpdateFieldCardInfo(card);

            // Spawn it
            NetworkServer.Spawn(boardCard);
            //Debug.Log("세큐리티 서버 스폰");

            Debug.Log("지우기 전 시큐 0번 : "+owner.deck.securityCard[0].data.cardName);
            // 대상자의 세큐리티 카드를 스폰시켰으니 제거
            owner.deck.securityCard.RemoveAt(0);
            Debug.Log("지운 후 시큐 0번 : " + owner.deck.securityCard[0].data.cardName);
            if (isServer) RpcBreakSecurityCard(boardCard, owner);
        }
        else if (card.data is SpellCard)
        {
            SpellCard spellCard = (SpellCard)card.data;
            GameObject boardCard = Instantiate(spellCard.cardPrefab.gameObject);
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.card = new CardInfo(card.data); // Save Card Info so we can re-access it later if we need to.
                                                    //newCard.cardName.text = card.name;
            newCard.isSecurity = true;
            newCard.image.sprite = card.image;
            newCard.image.color = Color.white;
            newCard.player = owner;

            // Update the Card Info that appears when hovering
            newCard.cardHover.UpdateFieldCardInfo(card);

            // Spawn it
            NetworkServer.Spawn(boardCard);

            // 대상자의 세큐리티 카드를 스폰시켰으니 제거
            owner.deck.securityCard.RemoveAt(0);

            //if (isServer) RpcPlaySecurityCard(boardCard, owner, attacker);
            if (isServer) RpcBreakSecurityCard(boardCard, owner);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdStartNewTurn()
    {
        if (Player.gameManager.turnCount != 1)
        { player.CmdDrawDeck(1); }
    }

    [Command(requiresAuthority = false)]
    public void CmdEndTurn()
    {
        GameObject playerField = Player.gameManager.playerField.content.gameObject;

        for (int i = 0; i < playerField.transform.childCount; ++i)
        {
            FieldCard myCard = playerField.transform.GetChild(i).GetComponent<FieldCard>();

            if (myCard == null) { return; }

            //내 필드의 모든 카드 block초기화 
            myCard.blocked = false;

            //싱크바 아닌 변수라 서버에서 변동해도 클라에서 변동안됨 하려면 Rpc로전달
            //myCard.isMyTurnEvoCastingActive = false;
            //myCard.isMyTurnDigimonCastingActive = false;
        }
    }

    [Command]
    public void CmdRaiseToBattle(FieldCard fieldCard, Player owner)
    {
        FieldCard mostUpperCard = fieldCard.FindMostUpperCard();

        while (fieldCard.isUnderMostCard == false)
        {
            //최하단 카드 우선 가져오기
            fieldCard = fieldCard.underCard;
        }

        while (fieldCard.isUpperMostCard == false)
        {
            if (isServer) RpcMoveRaiseToBattle(fieldCard, true);
            fieldCard = fieldCard.upperCard;
        }
        //(한번 더)마지막 최상단 카드도..while문은 최상단은 안해줌
        if (isServer) RpcMoveRaiseToBattle(fieldCard, false);
    }

    [ClientRpc]
    public void RpcPlayCard(GameObject boardCard, int index)
    {
        if (Player.gameManager.isSpawning)
        {
            // Set our FieldCard as a FRIENDLY creature for our local player, and ENEMY for our opponent.
            if (boardCard.GetComponent<FieldCard>().card.data is CreatureCard creatureCard)
            { 
                boardCard.GetComponent<FieldCard>().casterType = Target.FRIENDLIES;
                creatureCard.AppearDigimonCast(boardCard.GetComponent<FieldCard>());
            }
            else if(boardCard.GetComponent<FieldCard>().card.data is SpellCard spellCard)
            {
                boardCard.GetComponent<FieldCard>().casterType = Target.MY_OPTION;

                if (spellCard.hasSelectBuff)
                {
                    boardCard.GetComponent<FieldCard>().buffTargetCount = spellCard.buff.howMany;
                    boardCard.GetComponent<FieldCard>().SpawnTargetingArrow(boardCard.GetComponent<FieldCard>().card, true);
                    Player.gameManager.caster = boardCard.GetComponent<FieldCard>();
                }

                if(spellCard.isTamer)
                {
                    spellCard.FindTamerTarget(Player.gameManager.playerField.content);
                    spellCard.FindTamerTarget(Player.gameManager.playerRaiseField.content);
                }

                spellCard.AppearSpellCard(boardCard.GetComponent<FieldCard>().player);//test
            }

            boardCard.transform.SetParent(Player.gameManager.playerField.content, false);
            Player.gameManager.playerHand.RemoveCard(index); // Update player's hand
            Player.gameManager.isSpawning = false;
        }
        else if (player.hasEnemy)
        {
            if (boardCard.GetComponent<FieldCard>().card.data is CreatureCard)
            { boardCard.GetComponent<FieldCard>().casterType = Target.ENEMIES; }
            else if (boardCard.GetComponent<FieldCard>().card.data is SpellCard)
            { boardCard.GetComponent<FieldCard>().casterType = Target.OTHER_OPTION; }

            boardCard.transform.SetParent(Player.gameManager.enemyField.content, false);
            Player.gameManager.enemyHand.RemoveCard(index);
        }
    }

    [ClientRpc]
    public void RpcPlayEvoCard(GameObject boardCard, int index, FieldCard underCard)
    {
        if (Player.gameManager.isSpawning)
        {
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.underCard = underCard;
            underCard.upperCard = newCard;

            //여기서 메모리코스트 차감될때 상대 턴으로 넘어가는지 확인이 필요할거 같다......
            FieldCard tempUnderCard = underCard;//언더카드의 언더카드를 받기용 더미 언더카드
            while (tempUnderCard.isUnderMostCard == false)
            {
                tempUnderCard.isMyTurnEvoCastingActive = false;

                if (((CreatureCard)tempUnderCard.card.data).evolutionType.Exists(evo => evo == EvolutionType.MYTURN))
                {
                    if(tempUnderCard.player.isServer==false)
                    {
                        //서버가 아닌 참가자 클라일때 턴이 넘어가는데도 버프가 부여되서 추가코드..
                        if(MemoryChecker.Inst.memory /*+ ((CreatureCard)newCard.card.data).Ecost*/<=0)
                        {
                            //Debug.Log(MemoryChecker.Inst.memory + " + " + ((CreatureCard)newCard.card.data).Ecost);
                            //현 메모리에 새 진화카드 올린 코스트가 상대턴으로 안넘길때만 버프
                            ((CreatureCard)tempUnderCard.card.data).MyTurnCast(tempUnderCard, newCard);
                        }
                    }
                    else
                    {
                        ((CreatureCard)tempUnderCard.card.data).MyTurnCast(tempUnderCard, newCard);
                    }
                }
                tempUnderCard = tempUnderCard.underCard;
            }
            //마지막 isUnderMostCard가 true인 카드로 한번 더
            if (((CreatureCard)tempUnderCard.card.data).evolutionType.Exists(evo => evo == EvolutionType.MYTURN))
            {
                tempUnderCard.isMyTurnEvoCastingActive = false;

                if (tempUnderCard.player.isServer == false)
                {
                    //서버가 아닌 참가자 클라일때 턴이 넘어가는데도 버프가 부여되서 추가코드..
                    if (MemoryChecker.Inst.memory /*+ ((CreatureCard)newCard.card.data).Ecost*/<= 0)
                    {
                        //Debug.Log(MemoryChecker.Inst.memory + " + " + ((CreatureCard)newCard.card.data).Ecost);
                        //현 메모리에 새 진화카드 올린 코스트가 상대턴으로 안넘길때만 버프
                        ((CreatureCard)tempUnderCard.card.data).MyTurnCast(tempUnderCard, newCard);
                    }
                }
                else
                {
                    ((CreatureCard)tempUnderCard.card.data).MyTurnCast(tempUnderCard, newCard);
                }
            }

            //진화한 최상단 카드의 디지몬 효과 발동
            ((CreatureCard)newCard.card.data).EvoDigimonCast(newCard);
            //최상단 카드의 오리지널 디지몬 효과 발동
            ((CreatureCard)newCard.card.data).DigimonCast(newCard);

            // Set our FieldCard as a FRIENDLY creature for our local player, and ENEMY for our opponent.
            boardCard.GetComponent<FieldCard>().casterType = Target.FRIENDLIES;
            boardCard.transform.SetParent(Player.gameManager.playerField.content, false);
            Player.gameManager.playerHand.RemoveCard(index); // Update player's hand
            Player.gameManager.isSpawning = false;
        }
        else if (player.hasEnemy)
        {
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.underCard = underCard;
            underCard.upperCard = newCard;
            boardCard.GetComponent<FieldCard>().casterType = Target.ENEMIES;
            boardCard.transform.SetParent(Player.gameManager.enemyField.content, false);
            Player.gameManager.enemyHand.RemoveCard(index);
        }
    }

    [ClientRpc]
    public void RpcPlayTamaCard(GameObject boardCard)
    {
        if (Player.gameManager.isSpawning)
        {
            // Set our FieldCard as a FRIENDLY creature for our local player, and ENEMY for our opponent.
            boardCard.GetComponent<FieldCard>().casterType = Target.MY_BABY;
            boardCard.transform.SetParent(Player.gameManager.playerRaiseField.content, false);
            //Player.gameManager.playerHand.RemoveCard(index); // 손에서 꺼내는게 아니라 손에서 제거할 필요가 없음
            Player.gameManager.isSpawning = false;
        }
        else if (player.hasEnemy)
        {
            boardCard.GetComponent<FieldCard>().casterType = Target.OTHER_BABY;
            boardCard.transform.SetParent(Player.gameManager.enemyRaiseField.content, false); // 적 RaiseField아직 안만듦
            Player.gameManager.enemyRaiseField.Spawnbutton.SetActive(false);//디지타마 뒷면 오브젝트 없어지게끔
            //Player.gameManager.enemyHand.RemoveCard(index);
        }
    }

    [ClientRpc]
    public void RpcPlayEvoTamaCard(GameObject boardCard, int index, FieldCard underCard)
    {
        if (Player.gameManager.isSpawning)
        {
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.underCard = underCard;
            underCard.upperCard = newCard;
            // Set our FieldCard as a FRIENDLY creature for our local player, and ENEMY for our opponent.
            boardCard.GetComponent<FieldCard>().casterType = Target.MY_BABY;
            boardCard.transform.SetParent(Player.gameManager.playerRaiseField.content, false);
            Player.gameManager.playerHand.RemoveCard(index); // Update player's hand
            Player.gameManager.isSpawning = false;

            CheckTamerInField(newCard); //새로 카드 올릴 시 테이머 효과 체크(현재 신태일 확인용, 나중에 다른 효과들도 있다면 거기에 맞게 스폰상황 PlayCard에 추가해야함)
        }
        else if (player.hasEnemy)
        {
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.underCard = underCard;
            underCard.upperCard = newCard;
            boardCard.GetComponent<FieldCard>().casterType = Target.OTHER_BABY;
            boardCard.transform.SetParent(Player.gameManager.enemyRaiseField.content, false);
            Player.gameManager.enemyHand.RemoveCard(index);
        }
    }


    [ClientRpc]
    public void RpcPlaySecurityCard(GameObject boardCard, Player player, Entity attacker)
    {
        if (player.isLocalPlayer)
        {
            // Set our FieldCard as a FRIENDLY creature for our local player, and ENEMY for our opponent.
            boardCard.GetComponent<FieldCard>().casterType = Target.FRIENDLIES;
            boardCard.transform.SetParent(Player.gameManager.playerField.content, false);
            Player.gameManager.isSpawning = false;

            CreatureCard attackerCreature = (CreatureCard)attacker.GetComponent<FieldCard>().card.data;
            //어태커의 특성이 세큐리티 무효화가 아니라면(예:워그레이몬은 세큐리티 무효화함)
            if(boardCard.GetComponent<FieldCard>().card.data is SpellCard spellCard && attackerCreature.makeSecurityEffectNull==false)
            {
                spellCard.AppearSecuritySpellCard(player, boardCard.GetComponent<FieldCard>());

                if (spellCard.hasSelectSecurityBuff)
                {
                    Player player2 = boardCard.GetComponent<FieldCard>().player;
                    boardCard.GetComponent<FieldCard>().SpawnTargetingArrow(boardCard.GetComponent<FieldCard>().card, player2, true);
                    Player.gameManager.caster = boardCard.GetComponent<FieldCard>();
                }
            }
        }
        else if (player.hasEnemy)
        {
            boardCard.GetComponent<FieldCard>().casterType = Target.ENEMIES;
            boardCard.transform.SetParent(Player.gameManager.enemyField.content, false);
        }

        //Debug.Log("지금 딜레이 배틀 시작");
        StartCoroutine(DelayBattle(attacker, boardCard, 1.5f)); //스타트 코루틴 맨날 까먹어 맨날!! 그러고 왜 안되지? 이러고 있어!!        
    }

    [ClientRpc]
    public void RpcBreakSecurityCard(GameObject boardCard, Player player)
    {
        Debug.Log("파기될 세큐리티 카드이름 : "+boardCard.GetComponent<FieldCard>().card.data.cardName);
        if (player.isLocalPlayer)
        {
            Debug.Log("지운 후Rpc 시큐 0번 : " + player.deck.securityCard[0].data.cardName);
            // Set our FieldCard as a FRIENDLY creature for our local player, and ENEMY for our opponent.
            boardCard.GetComponent<FieldCard>().casterType = Target.FRIENDLIES;
            boardCard.transform.SetParent(Player.gameManager.playerField.content, false);
            Player.gameManager.isSpawning = false;
        }
        else if (player.hasEnemy)
        {
            boardCard.GetComponent<FieldCard>().casterType = Target.ENEMIES;
            boardCard.transform.SetParent(Player.gameManager.enemyField.content, false);
        }

        if(boardCard.gameObject.IsDestroyed()==false)
        {
            //Debug.Log("지금 딜레이 배틀 시작");
            Debug.Log("실제 삭제된 카드 : "+boardCard.GetComponent<FieldCard>().card.data.cardName);
            StartCoroutine(DelayGoTrash(boardCard, 1.5f));
            //GoTrash(boardCard);
        }
    }

    [ClientRpc]
    public void RpcMoveRaiseToBattle(FieldCard fieldCard, bool isSpawning)
    {
        if (Player.gameManager.isSpawning)
        {
            fieldCard.casterType = Target.FRIENDLIES;
            fieldCard.transform.SetParent(Player.gameManager.playerField.content, false);
            Player.gameManager.isSpawning = isSpawning;

            if (isSpawning == false)
            {
                FieldCard mostUpperCard = fieldCard.FindMostUpperCard();

                while (fieldCard.isUnderMostCard == false)
                {
                    //최하단 카드 우선 가져오기
                    fieldCard = fieldCard.underCard;
                    fieldCard.isMyTurnEvoCastingActive = false;
                    fieldCard.isMyTurnDigimonCastingActive = false;
                }

                while (fieldCard.isUpperMostCard == false)
                {
                    //최하단 부터 차례차례 나의 턴 진화원 효과가 있으면 최상단 카드에 넣어준다
                    if (((CreatureCard)fieldCard.card.data).evolutionType.Exists(evo => evo == EvolutionType.MYTURN))
                    {
                        //다 돌고 최종 마지막일때 버프 목록 아래부터 주르륵 훑기
                        ((CreatureCard)fieldCard.card.data).MyTurnCast(fieldCard, mostUpperCard);
                    }
                    fieldCard = fieldCard.upperCard;
                }
                //최상단 카드의 오리지널 디지몬 효과 발동
                ((CreatureCard)mostUpperCard.card.data).DigimonCast(mostUpperCard);
                //육성존에서 진화하는데도 발동되야함?? 혹시몰라 일단 주석
                //((CreatureCard)mostUpperCard.card.data).EvoDigimonCast(mostUpperCard);

                Player.gameManager.playerRaiseField.Spawnbutton.SetActive(true);
            }
        }

        else if (player.hasEnemy)
        {
            fieldCard.casterType = Target.ENEMIES;

            fieldCard.transform.SetParent(Player.gameManager.enemyField.content, false);

            Player.gameManager.enemyRaiseField.Spawnbutton.SetActive(true);
        }
    }

    private IEnumerator DelayBattle(Entity attacker, GameObject boardCard, float time)
    {
        //세큐리티 카드 출현 후 잠시 뒤에 싸우게 하기용
        yield return new WaitForSeconds(time);
        //while문을 이 다음에 써야함 안그럼 isTargeting인식 못함
        //★순서 바꾸지 마시오★

        while (boardCard != null && boardCard.GetComponent<FieldCard>().isTargeting)
        {
            // boardCard가 파괴되지 않았고 isTargeting이 true인 경우 계속 대기
            yield return null;
        }

        //한소라 카드가 attacker필드에 있으면 ActiveBuffPanel

        if (boardCard.IsDestroyed() == false)
        {
            FieldCard target = boardCard.GetComponent<FieldCard>();
            if (target.player.isLocalPlayer)
            {
                //Debug.Log("타깃을 Player에서 새로 스폰된 시큐 카드로 변경됩니다");
                attacker.combat.CmdBattle(attacker, target);
            }
        }
    }

    private IEnumerator DelayGoTrash(GameObject boardCard, float time)
    {
        yield return new WaitForSeconds(time);

        FieldCard destroyCard = boardCard.GetComponent<FieldCard>();

        destroyCard.player.deck.CmdAddGraveyard(destroyCard.player, destroyCard.card);
        Destroy(boardCard.gameObject);
    }
    public void GoTrash(GameObject boardCard)
    {
        FieldCard destroyCard = boardCard.GetComponent<FieldCard>();

        destroyCard.player.deck.CmdAddGraveyard(destroyCard.player, destroyCard.card);
        Destroy(boardCard.gameObject);
    }

    public void CheckTamerInField(FieldCard spawnCard)
    {
        //필드를 선회하며 테이머카드가 나와있는지 체크한다
        int cardCount = Player.gameManager.playerField.content.childCount;
        Transform content = Player.gameManager.playerField.content;

        for (int i = 0; i < cardCount; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();

            if (card.card.data is SpellCard spellCard && spellCard.isTamer)
            {   
                //만약 필드에 이미 테이머 카드가 있다면 새로 spawn시킨 디지몬 카드에 해당 테이머 효과 적용할지 체크시킨다
                spellCard.FindTamerTarget(spawnCard);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAddGraveyard(Player player, CardInfo card)
    {
        player.deck.graveyard.Add(card);
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveSecurity(Player player, int index)
    {
        player.deck.securityCard.RemoveAt(index);
    }
}
