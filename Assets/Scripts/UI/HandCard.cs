using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.XR;

public class HandCard : MonoBehaviour
{
    [Header("Sprite")]
    public Image image;

    [Header("Front & Back")]
    public Image cardfront;
    public Image cardback;

    [Header("Properties")]
    public string cardName;
    public int cost;
    public int strength;
    public int health;
    public int level = 0;
    public Text description;
    public string creatureType;

    [Header("Card Drag & Hover")]
    public HandCardDragHover cardDragHover;

    [Header("Outline")]
    public Image cardOutline;
    public Color readyColor;
    [HideInInspector] public int handIndex;
    [HideInInspector] public PlayerType playerType;

    // Called from PlayerHand to instantiate the cards in the player's hand
    public void AddCard(CardInfo newCard, int index, PlayerType playerT)
    {
        handIndex = index;
        playerType = playerT;

        // Enable hover on player cards. We disable it for enemy cards.
        cardDragHover.canHover = true;
        cardOutline.gameObject.SetActive(true);//해당 함수 설정안함

        // Reveal card FRONT, hide card BACK
        cardfront.color = Color.white;
        cardback.color = Color.clear;

        // Set card image
        image.sprite = newCard.image;
        cost = newCard.cost;

        if (newCard.data is CreatureCard creatureCard)
        {
            //만약 ScriptableCard의 종류가 Creature카드라면 레벨 정보를 가져온다
            level = creatureCard.level;
            Debug.Log("CreatureCard level: " + level);
        }

        // Assign description, name and remaining stats
        // 카드설명란..난 적을게 없다
        //description.text = newCard.description; // Description
        //cost.text = newCard.cost.ToString(); // Cost
        //cardName.text = newCard.name;

        // Only set Health & Strength if CreatureCard
        //if (newCard.data is CreatureCard)
        //{
        //    health.text = ((CreatureCard)newCard.data).health.ToString();
        //    strength.text = ((CreatureCard)newCard.data).strength.ToString();
        //}
    }

    public void AddCardBack()
    {
        cardfront.color = Color.clear;
        cardback.color = Color.white;
    }

    // Clears the card. Called when we Play/remove a card.
    public void RemoveCard()
    {
        Destroy(gameObject);
    }

    public void UpdateFieldCardInfo(CardInfo card)
    {
        // Reveal card FRONT, hide card BACK
        cardfront.color = Color.white;
        cardback.color = Color.clear;

        // Set card image
        image.sprite = card.image;

        //// Assign description, name and remaining stats
        //description.text = card.description; // Description
        //cost.text = card.cost; // Cost
        //cardName.text = card.name;
        //
        //// Stats
        //health.text = ((CreatureCard)card.data).health.ToString();
        //strength.text = ((CreatureCard)card.data).strength.ToString();
    }

    private void Update()
    {
        if (playerType == PlayerType.PLAYER && cardDragHover != null)
        {
            // Only drag during our turn, if our player has enough mana.
            Player player = Player.localPlayer;
            int manaCost = cost;
            if (Player.gameManager.isOurTurn)
            {
                cardDragHover.canDrag = player.deck.CanPlayCard(manaCost);
                cardOutline.color = cardDragHover.canDrag ? readyColor : Color.clear;
            }
        }
    }
}