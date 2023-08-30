using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MyTamaBook : MonoBehaviour
{
    public static MyTamaBook Inst { get; private set; }

    public Row[] rows;
    public TamaTile[,] Tiles { get; private set; }
    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    public int deleteIndex;

    private void Awake()
    {
        Inst = this;
        TamaTileSetting();
    }

    void TamaTileSetting()
    {
        Tiles = new TamaTile[rows.Max(row => row.tamaTiles.Length), rows.Length];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                TamaTile tile = rows[y].tamaTiles[x];

                tile.x = x;
                tile.y = y;

                Tiles[x, y] = tile;//초기화 선언 빼면 널익셉션..!
            }
        }
    }

    public void SetActiveButton(int value)
    {
        Tiles[value - 1, 0].button.gameObject.SetActive(true);
        Tiles[value - 1, 0]._item = CardBook.Inst.viewTamaList[value - 1];
        Tiles[value - 1, 0].card.sprite = CardBook.Inst.viewTamaList[value - 1].image;
    }

    public void TamaReSetting()
    {
        for (int i = 0; i < CardBook.Inst.myDigitamaCount; i++)
        {
            Tiles[i, 0]._item = CardBook.Inst.viewTamaList[i];
            Tiles[i, 0].card.sprite = CardBook.Inst.viewTamaList[i].image;
        }
        Tiles[CardBook.Inst.myDigitamaCount, 0].button.gameObject.SetActive(false);
    }
}
