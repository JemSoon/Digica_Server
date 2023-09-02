
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerRaiseField : MonoBehaviour, IDropHandler
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

    public void OnDrop(PointerEventData eventData)
    {
        HandCard card = eventData.pointerDrag.transform.GetComponent<HandCard>();
        Player player = Player.localPlayer;
        int manaCost = card.Ecost;

        if (Player.localPlayer.isServer && MemoryChecker.Inst.memory - manaCost < -10) { return; } //총 코스트량 오버하면 return
        else if (!Player.localPlayer.isServer && MemoryChecker.Inst.memory + manaCost > 10) { return; }

        if (player.IsOurTurn() && player.deck.CanPlayCard(manaCost) && card.isEvoCard)
        {
            int index = card.handIndex;
            CardInfo cardInfo = player.deck.hand[index];

            Player.gameManager.isSpawning = true;
            Player.gameManager.isHovering = false;
            //Player.gameManager.CmdOnCardHover(0, index);
            player.deck.CmdPlayEvoTamaCard(cardInfo, index, player, card.underCard); // Summon card onto the board
            player.combat.CmdChangeMana(-manaCost); // Reduce player's mana

            player.PlayerDraw(1); // 진화시키고 나면 한장 드로우
        }
    }
}
