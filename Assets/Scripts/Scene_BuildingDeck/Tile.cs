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

        int index = x + (y * CardBook.Inst.Width) + (CardBook.Inst.bookPage - 1) * 20;

        if (CardBook.Inst.buildingDeck[index].amount >= 4)
        {
            Debug.Log("중복4개 넘으려 함");
            return;
        }
     
        CardBook.Inst.myCardCount++;
        Debug.Log("디지몬 카드 개수 : " + CardBook.Inst.myCardCount);
        ++CardBook.Inst.buildingDeck[index].amount;
        //this._item.amount++;
        //MyCardDeck.Inst.SetActiveButton(CardBook.Inst.myCardCount);
    }
}
