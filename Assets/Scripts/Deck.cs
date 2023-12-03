using UnityEngine;
using Mirror;
using System;
using System.Collections;
using Unity.VisualScripting;
public class Deck : NetworkBehaviour
{

    [Header("Player")]
    public Player player;
    [HideInInspector] public int deckSize = 50;
    [HideInInspector] public int handSize = 7;//���߿� ���� ���

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
        //�Ϲ� �ʵ� ������ ����ī������ ũ����ī������ �����ؾ���

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
            //newCard.player.deck.playerField.Add(card);//�� �ʵ� ī�� ��Ͽ� �߰�

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
            //newCard.player.deck.playerField.Add(card);//�� �ʵ� ī�� ��Ͽ� �߰�

            // Update the Card Info that appears when hovering
            newCard.cardHover.UpdateFieldCardInfo(card);

            // Spawn it
            NetworkServer.Spawn(boardCard);

            // Remove card from hand
            hand.RemoveAt(index);

            if (isServer) RpcPlayCard(boardCard, index);

            spellCard.AppearSpellCard(owner);//����ī�� �ʵ� ������ �ٷ� ī��ȿ�� �����Ŵ ����!! ����!! ����!! RpcPlayCard���� �ε��� ������!! ����!! �Ϸ� ����!!
            
            if(!spellCard.isTamer)
            { 
                newCard.player.deck.graveyard.Add(newCard.card);//�ߵ� ���� ��������
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
            //���� ī�尡 ����Ʈ ���¿����� ����ī��� ����� �ֻ�� ī�带 ��������
            underCard.CmdRotation(underCard, Quaternion.Euler(0, 0, 0));
            newCard.CmdRotation(newCard, Quaternion.Euler(0, 0, -90));
        }

        if(underCard.buffs.Count > 0)
        {
            //�Ʒ��� �� ī�尡 ������ �ִٸ�
            for(int i = underCard.buffs.Count-1; i>=0; i--)
            {
                //���� ������ ���� ����
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

        // Remove card from hand
        hand.RemoveAt(index);

        if (isServer) RpcPlayEvoTamaCard(boardCard, index, underCard);
    }

    [Command(requiresAuthority = false)] //���� ������� ����� (��� Ŭ���̾�Ʈ�� ��ť��Ƽ ī�带 ������ ���̱� ����)
    public void CmdPlaySecurityCard(CardInfo card, Player owner, Entity attacker)
    {
        // �� ī��� ũ���� ī������ �ɼ�,���̸� ī������ �� �� ����

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

            // ������� ��ť��Ƽ ī�带 ������������ ����
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

            // ������� ��ť��Ƽ ī�带 ������������ ����
            owner.deck.securityCard.RemoveAt(0);

            if (isServer) RpcPlaySecurityCard(boardCard, owner, attacker);

            spellCard.AppearSecuritySpellCard(owner);
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
    }

    [Command]
    public void CmdRaiseToBattle(FieldCard fieldCard, Player owner)
    {
        FieldCard mostUpperCard = fieldCard.FindMostUpperCard();

        while (fieldCard.isUnderMostCard == false)
        {
            //���ϴ� ī�� �켱 ��������
            fieldCard = fieldCard.underCard;
        }

        while (fieldCard.isUpperMostCard == false)
        {
            if (isServer) RpcMoveRaiseToBattle(fieldCard, true);
            fieldCard = fieldCard.upperCard;
        }
        //(�ѹ� ��)������ �ֻ�� ī�嵵..while���� �ֻ���� ������
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
                creatureCard.AppearCast(boardCard.GetComponent<FieldCard>());
            }
            else if(boardCard.GetComponent<FieldCard>().card.data is SpellCard spellCard)
            {
                boardCard.GetComponent<FieldCard>().casterType = Target.MY_OPTION;

                if (spellCard.hasSelectBuff)
                { 
                    boardCard.GetComponent<FieldCard>().SpawnTargetingArrow(boardCard.GetComponent<FieldCard>().card, true);
                    Player.gameManager.caster = boardCard.GetComponent<FieldCard>();
                }

                if(spellCard.isTamer)
                {
                    spellCard.FindTamerTarget(Player.gameManager.playerField.content);
                    spellCard.FindTamerTarget(Player.gameManager.playerRaiseField.content);
                }
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

            FieldCard tempUnderCard = underCard;//���ī���� ���ī�带 �ޱ�� ���� ���ī��
            while (tempUnderCard.isUnderMostCard == false)
            {
                if (((CreatureCard)tempUnderCard.card.data).evolutionType.Exists(evo => evo == EvolutionType.MYTURN))
                {
                    //��ȭī�� �ø��� �޸𸮰� ��뿡�� �ȳѾ�ٸ� ����
                    ((CreatureCard)tempUnderCard.card.data).MyTurnCast(tempUnderCard, newCard);
                }
                tempUnderCard = tempUnderCard.underCard;
            }
            //������ isUnderMostCard�� true�� ī��� �ѹ� ��
            if (((CreatureCard)tempUnderCard.card.data).evolutionType.Exists(evo => evo == EvolutionType.MYTURN))
            {
                //��ȭī�� �ø��� �޸𸮰� ��뿡�� �ȳѾ�ٸ� ����
                ((CreatureCard)tempUnderCard.card.data).MyTurnCast(tempUnderCard, newCard);
            }

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
            //Player.gameManager.playerHand.RemoveCard(index); // �տ��� �����°� �ƴ϶� �տ��� ������ �ʿ䰡 ����
            Player.gameManager.isSpawning = false;
        }
        else if (player.hasEnemy)
        {
            boardCard.GetComponent<FieldCard>().casterType = Target.OTHER_BABY;
            boardCard.transform.SetParent(Player.gameManager.enemyRaiseField.content, false); // �� RaiseField���� �ȸ���
            Player.gameManager.enemyRaiseField.Spawnbutton.SetActive(false);//����Ÿ�� �޸� ������Ʈ �������Բ�
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

            CheckTamerInField(newCard); //���� ī�� �ø� �� ���̸� ȿ�� üũ(���� ������ Ȯ�ο�, ���߿� �ٸ� ȿ���鵵 �ִٸ� �ű⿡ �°� ������Ȳ PlayCard�� �߰��ؾ���)
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
        // ũ���� ī�� ��

        if (player.isLocalPlayer)
        {
            // Set our FieldCard as a FRIENDLY creature for our local player, and ENEMY for our opponent.
            boardCard.GetComponent<FieldCard>().casterType = Target.FRIENDLIES;
            boardCard.transform.SetParent(Player.gameManager.playerField.content, false);
            Player.gameManager.isSpawning = false;

            if(boardCard.GetComponent<FieldCard>().card.data is SpellCard spellCard &&
                spellCard.hasSelectSecurityBuff)
            {
                Player player2 = boardCard.GetComponent<FieldCard>().player;
                boardCard.GetComponent<FieldCard>().SpawnTargetingArrow(boardCard.GetComponent<FieldCard>().card, player2, true);
                Player.gameManager.caster = boardCard.GetComponent<FieldCard>();
            }
        }
        else if (player.hasEnemy)
        {
            boardCard.GetComponent<FieldCard>().casterType = Target.ENEMIES;
            boardCard.transform.SetParent(Player.gameManager.enemyField.content, false);
        }

        StartCoroutine(DelayBattle(attacker, boardCard, 1.5f)); //��ŸƮ �ڷ�ƾ �ǳ� ��Ծ� �ǳ�!! �׷��� �� �ȵ���? �̷��� �־�!!
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
                    //���ϴ� ī�� �켱 ��������
                    fieldCard = fieldCard.underCard;
                }

                while (fieldCard.isUpperMostCard == false)
                {
                    //���ϴ� ���� �������� ���� �� ��ȭ�� ȿ���� ������ �ֻ�� ī�忡 �־��ش�
                    if (((CreatureCard)fieldCard.card.data).evolutionType.Exists(evo => evo == EvolutionType.MYTURN))
                    {
                        //�� ���� ���� �������϶� ���� ��� �Ʒ����� �ָ��� �ȱ�
                        ((CreatureCard)fieldCard.card.data).MyTurnCast(fieldCard, mostUpperCard);
                    }
                    fieldCard = fieldCard.upperCard;
                }

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
        //��ť��Ƽ ī�� ���� �� ��� �ڿ� �ο�� �ϱ��
        yield return new WaitForSeconds(time);
        //while���� �� ������ ����� �ȱ׷� isTargeting�ν� ����
        //�ڼ��� �ٲ��� ���ÿ���

        while (boardCard != null && boardCard.GetComponent<FieldCard>().isTargeting)
        {
            // boardCard�� �ı����� �ʾҰ� isTargeting�� true�� ��� ��� ���
            yield return null;
        }

        if (boardCard.IsDestroyed() == false)//������ �𸣰����� �ι����ͼ� ���� �ı������ Ȯ���صּ� �����
        {
            FieldCard target = boardCard.GetComponent<FieldCard>();
            if (target.player.isLocalPlayer)
            {
                attacker.combat.CmdBattle(attacker, target);
                //((FieldCard)attacker).CmdRotation(((FieldCard)attacker), Quaternion.Euler(0, 0, -90));
            }
        }
    }

    public void CheckTamerInField(FieldCard spawnCard)
    {
        //�ʵ带 ��ȸ�ϸ� ���̸�ī�尡 �����ִ��� üũ�Ѵ�
        int cardCount = Player.gameManager.playerField.content.childCount;
        Transform content = Player.gameManager.playerField.content;

        for (int i = 0; i < cardCount; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();

            if (card.card.data is SpellCard spellCard && spellCard.isTamer)
            {   
                //���� �ʵ忡 �̹� ���̸� ī�尡 �ִٸ� ���� spawn��Ų ������ ī�忡 �ش� ���̸� ȿ�� �������� üũ��Ų��
                spellCard.FindTamerTarget(spawnCard);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAddGraveyard(Player player, CardInfo card)
    {
        player.deck.graveyard.Add(card);
    }
}
