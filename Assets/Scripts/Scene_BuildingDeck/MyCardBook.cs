using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MyCardBook : MonoBehaviour
{
    public static MyCardBook Inst { get; private set; }

    public Row[] rows;
    public DeckTile[,] Tiles { get; private set; }
    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    public int deleteIndex;

    private void Awake()
    {
        Inst = this;
        DeckTileSetting();
    }

    void DeckTileSetting()
    {
        Tiles = new DeckTile[rows.Max(row => row.deckTiles.Length), rows.Length];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                DeckTile tile = rows[y].deckTiles[x];

                tile.x = x;
                tile.y = y;

                Tiles[x, y] = tile;//초기화 선언 빼면 널익셉션..!
            }
        }
    }

    public void SetActiveButton(int value)
    {
        Tiles[(value - 1) % 10, (value - 1) / 10].button.gameObject.SetActive(true);
        Tiles[(value - 1) % 10, (value - 1) / 10]._item = CardBook.Inst.viewDeckList[value - 1];
        Tiles[(value - 1) % 10, (value - 1) / 10].card.sprite = CardBook.Inst.viewDeckList[value - 1].image;
    }

    public void DeckReSetting()
    {
        for (var y = 0; y <= CardBook.Inst.myCardCount / 10; y++)
        {

            int xLimit = (y < CardBook.Inst.myCardCount / 10) ? 10 : CardBook.Inst.myCardCount % 10;

            for (var x = 0; x < xLimit; x++)
            {
                int index = x + y * 10;
                DeckTile tile = rows[y].deckTiles[x];

                tile.x = x;
                tile.y = y;

                if (index < CardBook.Inst.myCardCount)
                {
                    Tiles[x, y]._item = CardBook.Inst.viewDeckList[index];
                    Tiles[x, y].card.sprite = CardBook.Inst.viewDeckList[index].image;
                }
                else
                {
                    Tiles[x, y]._item = null;
                    Tiles[x, y]._item.image = null;
                }
            }
        }
        Tiles[CardBook.Inst.myCardCount % 10, CardBook.Inst.myCardCount / 10].button.gameObject.SetActive(false);
    }
}
