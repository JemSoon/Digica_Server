
using UnityEngine;

public class PlayerRaiseField : MonoBehaviour
{
    public Transform content;
    public GameObject Spawnbutton;

    public void SpawnDigitama()
    {
        Player player = Player.localPlayer;

        if (player.IsOurTurn() && player.deck.CanPlayCard(0) && player.deck.babyCard.Count != 0)
        {
            CardInfo cardInfo = player.deck.babyCard[0];

            Player.gameManager.isSpawning = true;
            Player.gameManager.isHovering = false;

            player.deck.CmdPlayTamaCard(cardInfo, 0, player); // Summon card onto the board
            player.combat.CmdChangeMana(0); // Reduce player's mana

            Spawnbutton.SetActive(false); //나중에 출전시킬때 다시 스폰버튼 액티브시켜야함
        }
    }

    public void UpdateRaiseCards()
    {
        int cardCount = content.childCount;
        for (int i = 0; i < cardCount; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();
            card.CmdUpdateWaitTurn();
        }
    }
}
