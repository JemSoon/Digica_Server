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
        //++CardBook.Inst.buildingDeck[0].amount;
        ++CardBook.Inst.buildingDeck[x].amount;
        CardBook.Inst.myCardCount++;
        Debug.Log(CardBook.Inst.myCardCount);
    }
}
