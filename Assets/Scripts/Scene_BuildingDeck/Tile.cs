using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    public CardAndAmount _item;
    public Button button;
    public Image card;


    public void ButtonClick()
    {
        if (CardBook.Inst.myCardCount >= 50)
        { return; }

        //if (CardBook.Inst.buildingDeck[0].amount >= 4)
        //{
        //    Debug.Log("�ߺ�4�� ������ ��");
        //    return;
        //}
        int index = x + (y * CardBook.Inst.Width) + (CardBook.Inst.bookPage - 1) * 20;
        CardBook.Inst.myCardCount++;
        Debug.Log("������ ī�� ���� : " + CardBook.Inst.myCardCount);
        ++CardBook.Inst.buildingDeck[index].amount;
        //this._item.amount++;
        //MyCardDeck.Inst.SetActiveButton(CardBook.Inst.myCardCount);
    }
}
