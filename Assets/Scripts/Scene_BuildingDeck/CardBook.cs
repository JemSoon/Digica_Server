
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CardBook : MonoBehaviour
{
    public static CardBook Inst { get; private set; }

    [SerializeField] Text text;
    [SerializeField] Button next;
    [SerializeField] Button front;
    [SerializeField] Sprite cardback;

    public Row[] rows;
    public Tile[,] Tiles { get; private set; }
    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    public int myCardCount;
    public int myDigitamaCount;
    public int bookPage = 1;

    public CardAndAmount[] buildingDeck;

    private void Awake()
    {
        Inst = this;

        myCardCount = 0;

        //textMesh.text = "페이지 " + bookPage;

        TileImageSetting();
    }

    void TileImageSetting()
    {
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                int index = x + (y * Width) + (bookPage - 1) * 20;

                if (index >= buildingDeck.Length)
                {
                    //buildingDeck의 인덱스를 넘으면 카드 뒷면이미지로
                    tile._item.card = null;
                    tile.card.sprite = cardback;
                }

                else
                {
                    tile._item = buildingDeck[x + (y * Width) + (bookPage - 1) * 20];//각 인덱스에 맞게 배치

                    tile.card.sprite = tile._item.card.image;
                }
            }
        }
    }

    public void myDeckShift()
    {
        for (int i = MyCardBook.Inst.deleteIndex; i < buildingDeck.Length - 1; i++)
        {
            if (buildingDeck[i + 1].card == null)
            { break; }

            buildingDeck[i] = buildingDeck[i + 1];
        }
        //buildingDeck.RemoveAt(myDeck.Count - 1);
        CardAndAmountListWrapper wrapper = new CardAndAmountListWrapper();
        int index = buildingDeck.Length - 1;
        wrapper.deckList.RemoveAt(index);
        //빌딩 덱이 리스트가 아니라 배열이다보니 복잡해지는 서순..
        buildingDeck = wrapper.deckList.ToArray();
    }

    //public void myTamaShift()
    //{
    //    for (int i = MyTamaDeck.Inst.deleteIndex; i < myDigitamaDeck.Count - 1; i++)
    //    {
    //        if (myDigitamaDeck[i + 1] == null)
    //        { break; }

    //        myDigitamaDeck[i] = myDigitamaDeck[i + 1];
    //    }
    //    myDigitamaDeck.RemoveAt(myDigitamaDeck.Count - 1);
    //}

    public void ClickNext()
    {
        if (bookPage >= 11)
        { return; }

        bookPage++;
        text.text = "페이지 " + bookPage;
        TileImageSetting();
    }
    public void ClickFront()
    {
        if (bookPage <= 1)
        { return; }

        bookPage--;
        text.text = "페이지 " + bookPage;
        TileImageSetting();
    }

    public void GotoBattle()
    {
        SaveDeckData(buildingDeck);

        SceneManager.LoadScene("Battle");
    }


    // CardAndAmount 구조체를 리스트로 변환하여 저장하는 함수
    public void SaveDeckData(CardAndAmount[] deckData)
    {
        // 리스트로 변환
        List<CardAndAmount> deckList = new List<CardAndAmount>(deckData);

        // 리스트를 Json으로 직렬화하여 저장
        string jsonData = JsonUtility.ToJson(new CardAndAmountListWrapper { deckList = deckList });
        PlayerPrefs.SetString("DeckData", jsonData);
        PlayerPrefs.Save();
    
    }

}
