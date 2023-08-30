
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

    public CardAndAmount[] buildingDeck;//���� ī�帮��Ʈ�� �ش� ī���� �� ����
    public List<ScriptableCard> viewDeckList;//���� �� �ð�ȭ�� ����Ʈ
    public List<ScriptableCard> viewTamaList;
    private void Awake()
    {
        Inst = this;

        myCardCount = 0;

        //textMesh.text = "������ " + bookPage;

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
                    //buildingDeck�� �ε����� ������ ī�� �޸��̹�����
                    tile._item.card = null;
                    tile.card.sprite = cardback;
                }

                else
                {
                    tile._item = buildingDeck[x + (y * Width) + (bookPage - 1) * 20];//�� �ε����� �°� ��ġ

                    tile.card.sprite = tile._item.card.image;
                }
            }
        }
    }

    public void myDeckShift()
    {
        //������ �ٲ�鼭 �� �Լ� �ʿ䰡 ����
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
        text.text = "������ " + bookPage;
        TileImageSetting();
    }
    public void ClickFront()
    {
        if (bookPage <= 1)
        { return; }

        bookPage--;
        text.text = "������ " + bookPage;
        TileImageSetting();
    }

    public void GotoBattle()
    {
        SaveDeckData(buildingDeck);

        SceneManager.LoadScene("Battle");
    }


    // CardAndAmount ����ü�� ����Ʈ�� ��ȯ�Ͽ� �����ϴ� �Լ�
    public void SaveDeckData(CardAndAmount[] deckData)
    {
        // ����Ʈ�� ��ȯ
        List<CardAndAmount> deckList = new List<CardAndAmount>(deckData);

        {
            //�� �ڵ���� PlayerPrefs�� �̿��� �� ����,�ε带 �Ͽ�����
            //�����Բ��� PlayerPrefs�� ���̺�/�ε� ����̶� ������ ���� �Ѵ� ���ǵ� ���̿��� ����Ǵ� ������ ����Ҷ�� �ٲٴ°� ��õ��

            // ����Ʈ�� Json���� ����ȭ�Ͽ� ����
         
            //string jsonData = JsonUtility.ToJson(new CardAndAmountListWrapper { deckList = deckList });
         
            //PlayerPrefs.SetString("DeckData", jsonData);
         
            //PlayerPrefs.Save();
        }
        GameManager.localPlayerDeck = JsonUtility.ToJson(new CardAndAmountListWrapper { deckList = deckList });
    }

    public void DecreaseCardAmount(string cardID)
    {
        // buildingDeck �迭���� CardID�� ��ġ�ϴ� ī���� �ε����� ã���ϴ�.
        int index = System.Array.FindIndex(buildingDeck, cardData => cardData.card.CardID == cardID);

        // ���� �迭 ������ ī�带 ã���� ���
        if (index != -1)
        {
            // ã�� �ε����� �ش��ϴ� ī���� amount�� ���ҽ�ŵ�ϴ�.
            buildingDeck[index].amount--;

            // ���� amount�� ������ �Ǵ� ��쿡 ���� ó���� ���⼭ ������ �� �ֽ��ϴ�.
        }
        else
        {
            Debug.LogWarning("������ ī�带 ã�� ���߽��ϴ�.");
        }
    }

}
