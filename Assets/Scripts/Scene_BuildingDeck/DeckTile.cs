using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckTile : MonoBehaviour
{
    public int x;
    public int y;

    public ScriptableCard _item;
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

        CardBook.Inst.viewDeckList.Remove(this._item);
        CardBook.Inst.DecreaseCardAmount(_item.CardID);
        MyCardBook.Inst.DeckReSetting();
    }
}
