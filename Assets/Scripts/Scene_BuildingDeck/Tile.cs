using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    public CardAndAmount _item;
    public Button button;
    public Image card;


    public void ButtonClick()
    {
        if ((_item.card is CreatureCard digimonCard && digimonCard.level > 2) || !(_item.card is CreatureCard))
        {
            // (해당 카드가 크리쳐 카드고 레벨2(유년기)카드가 아니거나) (크리쳐카드가 아니라면) 일반 카드 목록에 들어감

            if (CardBook.Inst.myCardCount >= 50)
            { return; }

            int index = x + (y * CardBook.Inst.Width) + (CardBook.Inst.bookPage - 1) * 20;

            if (CardBook.Inst.buildingDeck[index].amount >= 4)
            {
                Debug.Log("중복4개 넘으려 함");
                return;
            }

            CardBook.Inst.myCardCount++;
            Debug.Log("디지몬 카드 개수 : " + CardBook.Inst.myCardCount);
            ++CardBook.Inst.buildingDeck[index].amount;
            CardBook.Inst.viewDeckList.Add(CardBook.Inst.buildingDeck[index].card);
            MyCardBook.Inst.SetActiveButton(CardBook.Inst.myCardCount);
        }

        else if (_item.card is CreatureCard tamaCard && tamaCard.level == 2) // 크리쳐카드가 레벨2(유년기 라면)
        {
            if (CardBook.Inst.myDigitamaCount >= 5)
            { return; }

            int index = x + (y * CardBook.Inst.Width) + (CardBook.Inst.bookPage - 1) * 20;

            if (CardBook.Inst.buildingDeck[index].amount >= 4)
            {
                Debug.Log("중복4개 넘으려 함");
                return;
            }

            CardBook.Inst.myDigitamaCount++;
            Debug.Log("디지타마 카드 개수 : " + CardBook.Inst.myDigitamaCount);
            ++CardBook.Inst.buildingDeck[index].amount;
            CardBook.Inst.viewTamaList.Add(CardBook.Inst.buildingDeck[index].card);
            MyTamaBook.Inst.SetActiveButton(CardBook.Inst.myDigitamaCount);
        }
    }
}
