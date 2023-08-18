using UnityEngine;
using UnityEngine.EventSystems;

public class HandCardDragHover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Card Properties")]
    public HandCard card;
    public CanvasGroup canvasGroup;

    [Header("Card Hover")]
    public bool canHover = false; // Hover and Drag are false by default and set to true when the card is instantiated.

    [Header("Card Drag")]
    public bool canDrag = false;
    Transform parentReturnTo = null; // Return to hand canvas
    public GameObject EmptyCard; // Used for creating an empty placeholder card where our current card used to be.
    private GameObject temp;

    [Header("Test")]
    private bool isDragging = false; // 카드를 드래그 중입니까?

    public void OnPointerEnter(PointerEventData eventData)
    {
        // If we can't hover, return.
        if (!canHover) return;

        // Move card locally
        card.transform.localScale = new Vector2(1.5f, 1.5f);
        card.transform.localPosition = new Vector2(card.transform.localPosition.x, 190);
        int index = card.transform.GetSiblingIndex();

        // Move corresponding card on opponent's screen
        Player.gameManager.isHovering = true;
        //Player.gameManager.CmdOnCardHover(-25, index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!canHover) return;

        // Return to normal
        card.transform.localScale = new Vector2(1f, 1f);
        card.transform.localPosition = new Vector2(card.transform.localPosition.x, 0);
        int index = card.handIndex;

        // Move corresponding card back to normal on opponent's screen
        //Player.gameManager.CmdOnCardHover(0, index);
        Player.gameManager.isHovering = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // If we can't drag, return.
        if (!canDrag) return;

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
        transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
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

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.GetComponent<FieldCard>() == null) return;

        if (card.cardInfo.data is CreatureCard handCard && // hand카드가 CreatureCard이고
            collision.GetComponent<FieldCard>().card.data is CreatureCard fieldCard && // field카드가 CreatureCard이고
            isDragging==true) // 드래그 중이라면
        {
            //만약 ScriptableCard의 종류가 Creature카드라면 레벨 정보를 가져온다
            if (handCard.level == fieldCard.level+1)
            {
                Debug.Log("진화 가능");
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("나가짐");
    }
}
