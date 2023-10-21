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
    public int Ecost;
    public int strength;
    public int health;
    public int level = 0;
    public Text description;
    public string creatureType;
    public bool isEvoCard;

    [Header("Card Drag & Hover")]
    public HandCardDragHover cardDragHover;

    [Header("Outline")]
    public Image cardOutline;
    public Color readyColor;
    /*[HideInInspector]*/ public int handIndex;
    [HideInInspector] public PlayerType playerType;

    [Header("Evolnfo")]
    [HideInInspector] public CardInfo cardInfo;
    public FieldCard underCard;

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
        SetECost(newCard);

        if (newCard.data is CreatureCard creatureCard)
        {
            //만약 ScriptableCard의 종류가 Creature카드라면 레벨 정보를 가져온다
            level = creatureCard.level;
            //Debug.Log("CreatureCard level: " + level);
        }
        cardInfo = newCard;
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
                //모든 카드는 드래그 할 수 있되, 필드 출전시 총 마나의 양이 -10보다 작거나 10보다 크면 반환시켜둠
                //나중에 옵션카드나 테이머카드는 필드에 자신의 컬러 카드가 있는지 확인하고 낼 수 있게끔..
                cardDragHover.canDrag = true; //player.deck.CanPlayCard(manaCost); //원래는 마나 총량 넘으면 못내게 했는데 ECost라는 다른 루트땜에 일단 낼 수 있게함
                cardOutline.color = cardDragHover.canDrag ? readyColor : Color.clear;
            }
        }
    }

    public int SetECost(CardInfo Info)
    {
        if (Info.data != null)
        {
            if (Info.data is CreatureCard creatureCard)
            {
                return Ecost = creatureCard.Ecost;
            }
            else
            {
                return Ecost = 0;
            }
        }
        else
        { return Ecost = 0; }
    }
}