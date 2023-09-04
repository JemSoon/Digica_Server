using UnityEngine;
using Mirror;
using System;

public class Deck : NetworkBehaviour
{

    [Header("Player")]
    public Player player;
    [HideInInspector] public int deckSize = 50;
    [HideInInspector] public int handSize = 7;//나중에 수정 요망

    [Header("Decks")]
    public SyncListCard deckList = new SyncListCard(); // DeckList used during the match. Contains all cards in the deck. This is where we'll be drawing card froms.
    public SyncListCard graveyard = new SyncListCard(); // Cards in player graveyard.
    public SyncListCard hand = new SyncListCard(); // Cards in player's hand during the match.
    public SyncListCard babyCard = new SyncListCard();

    [Header("Battlefield")]
    public SyncListCard playerField = new SyncListCard(); // Field where we summon creatures.

    [Header("SecurityCard")]
    public SyncListCard securityCard = new SyncListCard();

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
        if(player.isServer)
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
        hand.RemoveAt(index);

        if (isServer) RpcPlayCard(boardCard, index);
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

        // If creature has charge, reduce waitTurn to 0 so they can attack right away.
        if (creature.hasCharge) newCard.waitTurn = 0;

        // Update the Card Info that appears when hovering
        newCard.cardHover.UpdateFieldCardInfo(card);

        // Spawn it
        NetworkServer.Spawn(boardCard);

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

    [Command(requiresAuthority = false)] //권한 없애줘야 실행됨 (상대 클라이언트의 세큐리티 카드를 꺼내는 것이기 때문)
    public void CmdPlaySecurityCard(CardInfo card, Player owner)
    {
        // 이 카드는 크리쳐 카드일지 옵션,테이머 카드일지 알 수 없다

        if(card.data is CreatureCard)
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

            if (creature.hasCharge) newCard.waitTurn = 0;
            
            // Update the Card Info that appears when hovering
            newCard.cardHover.UpdateFieldCardInfo(card);

            // Spawn it
            NetworkServer.Spawn(boardCard);

            // 대상자의 세큐리티 카드를 스폰시켰으니 제거
            owner.deck.securityCard.RemoveAt(0);

            if (isServer) RpcPlaySecurityCard(boardCard, owner);
        }
        else if(card.data is SpellCard)
        {
            SpellCard spellCard = (SpellCard)card.data;
            GameObject boardCard = Instantiate(spellCard.cardPrefab.gameObject);
            FieldCard newCard = boardCard.GetComponent<FieldCard>();
            newCard.card = new CardInfo(card.data); // Save Card Info so we can re-access it later if we need to.
                                                    //newCard.cardName.text = card.name;
            newCard.image.sprite = card.image;
            newCard.image.color = Color.white;
            newCard.player = owner;

            // Update the Card Info that appears when hovering
            newCard.cardHover.UpdateFieldCardInfo(card);

            // Spawn it
            NetworkServer.Spawn(boardCard);

            // 대상자의 세큐리티 카드를 스폰시켰으니 제거
            owner.deck.securityCard.RemoveAt(0);

            if (isServer) RpcPlaySecurityCard(boardCard, owner);
        }
    }

    [Command]
    public void CmdStartNewTurn()
    {
        
    }

    [ClientRpc]
    public void RpcPlayCard(GameObject boardCard, int index)
    {
        if (Player.gameManager.isSpawning)
        {
            // Set our FieldCard as a FRIENDLY creature for our local player, and ENEMY for our opponent.
            boardCard.GetComponent<FieldCard>().casterType = Target.FRIENDLIES;
            boardCard.transform.SetParent(Player.gameManager.playerField.content, false);
            Player.gameManager.playerHand.RemoveCard(index); // Update player's hand
            Player.gameManager.isSpawning = false;
        }
        else if (player.hasEnemy)
        {
            boardCard.GetComponent<FieldCard>().casterType = Target.ENEMIES;
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
    public void RpcPlaySecurityCard(GameObject boardCard, Player player)
    {
        if (player.isLocalPlayer)
        {
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
    }
}
