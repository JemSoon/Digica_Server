
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CardBook : MonoBehaviour
{
    public static CardBook Inst { get; private set; }

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

        var tile0 = rows[0].tiles[0];
        var tile1 = rows[0].tiles[1];

        //ScriptableCard scriptableCard0 = ScriptableCard.Cache.TryGetValue(buildingDeck[0].cardID, out ScriptableCard card0) ? card0 : null;
        //ScriptableCard scriptableCard1 = ScriptableCard.Cache.TryGetValue(buildingDeck[1].cardID, out ScriptableCard card1) ? card1 : null;
        tile0.card.sprite = buildingDeck[0].card.image;
        tile1.card.sprite = buildingDeck[1].card.image;

        //for (var y = 0; y < Height; y++)
        //{
        //    for (var x = 0; x < Width; x++)
        //    {
        //        var tile = rows[y].tiles[x];

        //        tile.x = x;
        //        tile.y = y;

        //        tile._item.data = buildingDeck[x + (y * Width) + (bookPage - 1) * 20];//각 인덱스에 맞게 배치

        //        tile.card.sprite = tile._item.image;
        //    }
        //}
    }

    //public void myDeckShift()
    //{
    //    for (int i = MyCardDeck.Inst.deleteIndex; i < myDeck.Count - 1; i++)
    //    {
    //        if (myDeck[i + 1] == null)
    //        { break; }

    //        myDeck[i] = myDeck[i + 1];
    //    }
    //    myDeck.RemoveAt(myDeck.Count - 1);
    //}

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

    //public void ClickNext()
    //{
    //    if (bookPage >= 11)
    //    { return; }

    //    bookPage++;
    //    textMesh.text = "페이지 " + bookPage;
    //    TileImageSetting();
    //}
    //public void ClickFront()
    //{
    //    if (bookPage <= 1)
    //    { return; }

    //    bookPage--;
    //    textMesh.text = "페이지 " + bookPage;
    //    TileImageSetting();
    //}

    public void GotoBattle()
    {
        //DeckData deckData = ScriptableObject.CreateInstance<DeckData>();
        //deckData.deck = buildingDeck;
        //string path = "Assets/Resources/NewDeckData.asset";
        //AssetDatabase.CreateAsset(deckData, path);
        //AssetDatabase.SaveAssets();
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
