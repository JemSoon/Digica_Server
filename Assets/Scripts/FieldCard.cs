using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class FieldCard : Entity
{
    [SyncVar/*, HideInInspector*/] public CardInfo card; // Get card info

    [Header("Card Properties")]
    public Image image; // card image on field
    public Text cardName; // Text of the card name
    public Text healthText; // Text of the health
    public Text strengthText; // Text of the strength

    [Header("Shine")]
    public Image shine;
    public Color hoverColor;
    public Color readyColor; // Shine color when ready to attack
    public Color targetColor; // Shine color when ready to attack

    [Header("Card Hover")]
    public HandCard cardHover;

    [Header("Owner")]
    [SyncVar]
    public Player player;

    [Header("Evo Route")]
    public FieldCard upperCard;
    public FieldCard underCard;
    public bool isUpperMostCard => upperCard == null; // �ֻ�� ī���ΰ�?
    public bool isUnderMostCard => underCard == null; // ���ϴ� ī���ΰ�?

    [Header("Security")]
    public bool isSecurity = false;

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        // If we have a card but no sprite, make sure the sprite is up to date since we can't SyncVar the sprite.
        // Useful to avoid bugs when a player was offline when the card spawned, or if they reconnected.
        if (image.sprite == null && (card.name != null || cardName.text == ""))
        {
            // Update Stats
            image.color = Color.white;
            image.sprite = card.image;

            //cardName.text = card.name; //������~

            // Update card hover info
            cardHover.UpdateFieldCardInfo(card);
        }

        //healthText.text = health.ToString();
        //strengthText.text = strength.ToString();

        if (CanAttack()) shine.color = readyColor;
        else if (CantAttack()) shine.color = Color.clear;

        ChaseUpperCard();

        //�ֻ�� ī�尡 �ƴϸ� �ݸ�������test
        //if(isUpperMostCard==false) { GetComponent<BoxCollider2D>().enabled = false; }
        //else { GetComponent<BoxCollider2D>().enabled = true; }
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateWaitTurn()
    {
        //Debug.LogError("Here");
        if (waitTurn > 0) waitTurn--;
    }

    public void ChaseUpperCard()
    {
        if(isUpperMostCard==false && upperCard!=null)//�� �� ī�尡 �ƴϰ� �� ī�� ������ null�� �ƴ϶��
        {
            // ������� �� ī�� ���� �޸�üĿ�� ������ �ݴ�� �ؾ��ҵ�
            //GetComponent<RectTransform>().anchoredPosition = upperCard.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, -47); 
            upperCard.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + new Vector2(0, 47);
        }
    }
}