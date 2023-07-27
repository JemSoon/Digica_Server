using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    public CardInfo _item;
    public Button button;
    public Image card;
    public CardInfo Item
    {
        get => _item;

        set
        {
            if (_item.Equals(value)) { return; }
            _item = value;

            card.sprite = _item.image;
        }
    }

    public void ButtonClick()
    {
        if (CardBook.Inst.myCardCount >= 50)
        { return; }

        //if (CardBook.Inst.buildingDeck[0].amount >= 4)
        //{
        //    Debug.Log("중복4개 넘으려 함");
        //    return;
        //}
        //++CardBook.Inst.buildingDeck[0].amount;
        ++CardBook.Inst.buildingDeck[x].amount;
        CardBook.Inst.myCardCount++;
        Debug.Log(CardBook.Inst.myCardCount);
    }
}
