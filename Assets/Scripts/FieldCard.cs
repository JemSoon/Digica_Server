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

    public bool giveBuff = false; // 1회용 버프 썼는가?(스펠카드용)

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
    public bool isUpperMostCard => upperCard == null; // 최상단 카드인가?
    public bool isUnderMostCard => underCard == null; // 최하단 카드인가?

    [Header("Security")]
    public bool isSecurity = false;
    [Header("SpellEffect")]
    readonly public SyncList<Buffs> buffs = new SyncList<Buffs>(); // 효과 받은 수치를 저장해 두기
    [Header("Buffs")]
    [SyncVar] public int securityAttack = 0;
    [Header("Test")]
    public FieldCardHover cardDragHover;

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

            //cardName.text = card.name; //터진다~

            // Update card hover info
            cardHover.UpdateFieldCardInfo(card);
        }

        //healthText.text = health.ToString();
        //strengthText.text = strength.ToString();

        if (CanAttack()) shine.color = readyColor;
        else if (CantAttack()) shine.color = Color.clear;

        ChaseUpperCard();

        //최상단 카드가 아니면 콜리전끄기test
        //if(isUpperMostCard==false) { GetComponent<BoxCollider2D>().enabled = false; }
        //else { GetComponent<BoxCollider2D>().enabled = true; }

        if (player==Player.localPlayer && casterType==Target.MY_BABY && cardDragHover!=null /*&& waitTurn <= 0*/)
        {
            if (Player.gameManager.isOurTurn)
            {
                cardDragHover.canDrag = true; //player.deck.CanPlayCard(manaCost); //원래는 마나 총량 넘으면 못내게 했는데 ECost라는 다른 루트땜에 일단 낼 수 있게함
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateWaitTurn()
    {
        //Debug.LogError("Here");
        if (waitTurn > 0) { waitTurn--; }
    }

    public void ChaseUpperCard()
    {
        if(isUpperMostCard==false && upperCard!=null && !cardDragHover.isDragging)//맨 위 카드가 아니고 위 카드 정보가 null이 아니라면
        {
            // 상대편에서 내 카드 볼때 메모리체커에 가려짐 반대로 해야할듯
            //GetComponent<RectTransform>().anchoredPosition = upperCard.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, -47); 
            upperCard.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + new Vector2(0, 47);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroySpellCard()
    {
        if (card.data is SpellCard spellCard)
        {
            //player.deck.playerField.Remove(card);//안씀
            //player.deck.graveyard.Add(card);//카드를 꺼내자마자 무덤에 저장되야함
            spellCard.EndTurnEffect(player);
            Destroy(this.gameObject);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeSomeThing(Buffs buff, bool isStart)
    {
        if(isStart)
        {
            //isStart는 버프를 추가할 때인지 뺄 때인지 구분용
            strength += buff.buffDP;
            securityAttack += buff.securityAttack;
        }
        else
        {
            strength -= buff.buffDP;
            securityAttack = 0;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAddBuff(Buffs buff)
    {
        buffs.Add(buff);
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveBuff(int index)
    {
        buffs.RemoveAt(index);
        Debug.Log("버프 제거 완료");
    }
}