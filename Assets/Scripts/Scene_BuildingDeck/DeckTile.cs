using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckTile : MonoBehaviour
{
    public int x;
    public int y;

    public CardAndAmount _item;
    public Button button;
    public Image card;

    private void Start()
    {
        button.gameObject.SetActive(false);
    }

    public void DeckButtonClick()
    {
        MyCardBook.Inst.deleteIndex = x % 10 + y * 10;
        CardBook.Inst.myCardCount--;
        this._item.amount--;
        CardBook.Inst.myDeckShift();
        MyCardBook.Inst.DeckReSetting();
    }
}
