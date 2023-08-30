using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TamaTile : MonoBehaviour
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

    public void TamaButtonClick()
    {
        MyTamaBook.Inst.deleteIndex = x;
        CardBook.Inst.myDigitamaCount--;

        CardBook.Inst.viewTamaList.Remove(this._item);
        CardBook.Inst.DecreaseCardAmount(_item.CardID);
        MyTamaBook.Inst.TamaReSetting();
    }
}
