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
            // (�ش� ī�尡 ũ���� ī��� ����2(�����)ī�尡 �ƴϰų�) (ũ����ī�尡 �ƴ϶��) �Ϲ� ī�� ��Ͽ� ��

            if (CardBook.Inst.myCardCount >= 50)
            { return; }

            int index = x + (y * CardBook.Inst.Width) + (CardBook.Inst.bookPage - 1) * 20;

            if (CardBook.Inst.buildingDeck[index].amount >= 4)
            {
                Debug.Log("�ߺ�4�� ������ ��");
                return;
            }

            CardBook.Inst.myCardCount++;
            Debug.Log("������ ī�� ���� : " + CardBook.Inst.myCardCount);
            ++CardBook.Inst.buildingDeck[index].amount;
            CardBook.Inst.viewDeckList.Add(CardBook.Inst.buildingDeck[index].card);
            MyCardBook.Inst.SetActiveButton(CardBook.Inst.myCardCount);
        }

        else if (_item.card is CreatureCard tamaCard && tamaCard.level == 2) // ũ����ī�尡 ����2(����� ���)
        {
            if (CardBook.Inst.myDigitamaCount >= 5)
            { return; }

            int index = x + (y * CardBook.Inst.Width) + (CardBook.Inst.bookPage - 1) * 20;

            if (CardBook.Inst.buildingDeck[index].amount >= 4)
            {
                Debug.Log("�ߺ�4�� ������ ��");
                return;
            }

            CardBook.Inst.myDigitamaCount++;
            Debug.Log("����Ÿ�� ī�� ���� : " + CardBook.Inst.myDigitamaCount);
            ++CardBook.Inst.buildingDeck[index].amount;
            CardBook.Inst.viewTamaList.Add(CardBook.Inst.buildingDeck[index].card);
            MyTamaBook.Inst.SetActiveButton(CardBook.Inst.myDigitamaCount);
        }
    }
}
