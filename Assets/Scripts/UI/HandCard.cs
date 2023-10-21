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
        cardOutline.gameObject.SetActive(true);//�ش� �Լ� ��������

        // Reveal card FRONT, hide card BACK
        cardfront.color = Color.white;
        cardback.color = Color.clear;

        // Set card image
        image.sprite = newCard.image;
        cost = newCard.cost;
        SetECost(newCard);

        if (newCard.data is CreatureCard creatureCard)
        {
            //���� ScriptableCard�� ������ Creatureī���� ���� ������ �����´�
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
                //��� ī��� �巡�� �� �� �ֵ�, �ʵ� ������ �� ������ ���� -10���� �۰ų� 10���� ũ�� ��ȯ���ѵ�
                //���߿� �ɼ�ī�峪 ���̸�ī��� �ʵ忡 �ڽ��� �÷� ī�尡 �ִ��� Ȯ���ϰ� �� �� �ְԲ�..
                cardDragHover.canDrag = true; //player.deck.CanPlayCard(manaCost); //������ ���� �ѷ� ������ ������ �ߴµ� ECost��� �ٸ� ��Ʈ���� �ϴ� �� �� �ְ���
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