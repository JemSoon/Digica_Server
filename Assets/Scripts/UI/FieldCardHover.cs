using UnityEngine.EventSystems;
using UnityEngine;

public class FieldCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public FieldCard card;
    public float hoverDelay = 0.4f;
    private bool isHovering = false;

    [Header("Test")]
    public CanvasGroup canvasGroup;
    public bool canDrag = false;
    Transform parentReturnTo = null; // Return to raise canvas
    public GameObject EmptyCard; // Used for creating an empty placeholder card where our current card used to be.
    private GameObject temp;
    public bool isDragging = false;
    public void OnPointerClick(PointerEventData eventData)
    {
        // Make sure our Player isn't already targetting something
        if (!Player.localPlayer.isTargeting && Player.gameManager.isOurTurn && card.casterType == Target.FRIENDLIES && card.CanAttack() && card.card.data is CreatureCard)
        {
            card.SpawnTargetingArrow(card.card);
            HideCardInfo();
        }
        else if (!Player.localPlayer.isTargeting && Player.gameManager.isOurTurn && card.casterType == Target.FRIENDLIES && card.CantAttack() && card.securityAttack > 0 && card.card.data is CreatureCard)
        {
            //공격할 순 없는데 세큐리티 어택개수가 남아있다면
            card.SpawnTargetingArrow(card.card);
            HideCardInfo();
        }
        else if(!Player.localPlayer.isTargeting && Player.gameManager.isOurTurn && card.casterType == Target.MY_OPTION && !card.giveBuff && card.card.data is SpellCard spellCard && spellCard.hasSelectBuff)
        {
            //선택부여 버프를 카드 스폰후 자동 발동이아닌 수동발동 할때 용도
            //card.SpawnTargetingArrow(card.card, true);
            //HideCardInfo();
            //card.giveBuff = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging)
        {
            isHovering = true;

            Invoke("ShowCardInfo", hoverDelay);// Reveal card info after a slight delay, so it doesn't appear instantly when we play a card.
        } 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // Turn off hover
        card.cardHover.gameObject.SetActive(false);

        // Turn off Shine/Glow border
        Player.gameManager.CmdOnFieldCardHover(this.gameObject, false, false);

        Player.gameManager.isHoveringField = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    { 
        if(!canDrag) return;

        temp = Instantiate(EmptyCard);
        temp.transform.SetParent(this.transform.parent, false);

        temp.transform.SetSiblingIndex(transform.GetSiblingIndex());

        parentReturnTo = this.transform.parent;
        transform.SetParent(this.transform.parent.parent, false);

        canvasGroup.blocksRaycasts = false;

        isDragging = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        // If we can't drag, return.
        if (!canDrag) return;

        Vector3 screenPoint = eventData.position;
        screenPoint.z = 10.0f; //distance of the plane from the camera

        while(card.isUnderMostCard==false)
        {
            card.cardHover.gameObject.SetActive(false);
            card = card.underCard;
        }

        card.transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrag) return;

        transform.SetParent(parentReturnTo, false);
        transform.SetSiblingIndex(temp.transform.GetSiblingIndex());
        canvasGroup.blocksRaycasts = true;
        Destroy(temp);

        isDragging = false;
    }

    public void ShowCardInfo()
    {
        if (isHovering && !isDragging)
        {
            // Turn on hover if player isn't targeting
            if (!Player.localPlayer.isTargeting) card.cardHover.gameObject.SetActive(true);

            // Turn on Shine/Glow border so our opponents can see us hovering over cards during our turn.
            if (Player.gameManager.isOurTurn)
            {
                Player.gameManager.isHoveringField = true;
                Player.gameManager.CmdOnFieldCardHover(this.gameObject, true, Player.localPlayer.isTargeting);
            }
        }
    }

    public void HideCardInfo()
    {
        card.cardHover.gameObject.SetActive(false);
    }
}
