
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

    public CardAndAmount[] buildingDeck;//나의 카드리스트와 해당 카드의 총 갯수
    public List<ScriptableCard> viewDeckList;//나의 덱 시각화용 리스트
    public List<ScriptableCard> viewTamaList;
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
        //서버로 바뀌면서 이 함수 필요가 음슴
        for (int i = MyCardBook.Inst.deleteIndex; i < buildingDeck.Length - 1; i++)
        {
            if (buildingDeck[i + 1].card == null)
            { break; }

            buildingDeck[i] = buildingDeck[i + 1];
        }
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

        {
            //이 코드들은 PlayerPrefs를 이용해 덱 저장,로드를 하였으나
            //선생님께서 PlayerPrefs는 세이브/로드 기능이라 게임을 끄고 켜는 세션들 사이에서 저장되는 데이터 저장소라고 바꾸는걸 추천함

            // 리스트를 Json으로 직렬화하여 저장
         
            //string jsonData = JsonUtility.ToJson(new CardAndAmountListWrapper { deckList = deckList });
         
            //PlayerPrefs.SetString("DeckData", jsonData);
         
            //PlayerPrefs.Save();
        }
        GameManager.localPlayerDeck = JsonUtility.ToJson(new CardAndAmountListWrapper { deckList = deckList });
    }

    public void DecreaseCardAmount(string cardID)
    {
        // buildingDeck 배열에서 CardID와 일치하는 카드의 인덱스를 찾습니다.
        int index = System.Array.FindIndex(buildingDeck, cardData => cardData.card.CardID == cardID);

        // 만약 배열 내에서 카드를 찾았을 경우
        if (index != -1)
        {
            // 찾은 인덱스에 해당하는 카드의 amount를 감소시킵니다.
            buildingDeck[index].amount--;

            // 만약 amount가 음수가 되는 경우에 대한 처리를 여기서 수행할 수 있습니다.
        }
        else
        {
            Debug.LogWarning("덱에서 카드를 찾지 못했습니다.");
        }
    }

}
