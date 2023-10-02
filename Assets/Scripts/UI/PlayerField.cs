using UnityEngine.EventSystems;
using UnityEngine;


public class PlayerField : MonoBehaviour, IDropHandler
{
    public Transform content;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.transform.GetComponent<FieldCard>() != null && 
            eventData.pointerDrag.transform.GetComponent<FieldCard>().casterType == Target.MY_BABY) 
        {
            FieldCard uppestCard = eventData.pointerDrag.transform.GetComponent<FieldCard>(); //이름 너무 기니까 변수로 받고
            
            while(!uppestCard.isUpperMostCard)
            {
                uppestCard = uppestCard.upperCard; //최상단 카드 가져오고
            }

            if(Player.gameManager.isDigitamaOpenOrMove) { return; } // 디지타마 오픈한 턴이면 리턴
            if (((CreatureCard)uppestCard.card.data).level<3) { return; } //필드에 보내려는 카드가 레벨3미만이면 못냄

            //필드카드(육성존 카드)를 드롭하면
            Debug.Log("육성 카드 온 드롭");
            Player Raiseplayer = Player.localPlayer;
            if (Raiseplayer.IsOurTurn())
            {
                Player.gameManager.isSpawning = true;
                Player.gameManager.isHovering = false;
                Player.gameManager.isDigitamaOpenOrMove = true;
                Raiseplayer.deck.CmdRaiseToBattle(eventData.pointerDrag.transform.GetComponent<FieldCard>(), Raiseplayer);
            }

            return; 
        }

        HandCard card = eventData.pointerDrag.transform.GetComponent<HandCard>();
        Player player = Player.localPlayer;
        int manaCost;
        if (card.isEvoCard)
        { manaCost = card.Ecost; }
        else
        { manaCost = card.cost; }

        if(Player.localPlayer.isServer && MemoryChecker.Inst.memory - manaCost < -10) { return; } //총 코스트량 오버하면 return
        else if(!Player.localPlayer.isServer && MemoryChecker.Inst.memory + manaCost > 10) { return; }

        if (player.IsOurTurn() && player.deck.CanPlayCard(manaCost) && card.isEvoCard)
        {
            int index = card.handIndex;
            CardInfo cardInfo = player.deck.hand[index];

            Player.gameManager.isSpawning = true;
            Player.gameManager.isHovering = false;
            //Player.gameManager.CmdOnCardHover(0, index);

            player.deck.CmdPlayEvoCard(cardInfo, index, player, card.underCard); // Summon card onto the board
            player.CmdDrawDeck(1); // 진화시키고 나면 한장 드로우(코스트 까기전에 있어야함) 순서 이대로 둘것
            player.combat.CmdChangeMana(-manaCost); // Reduce player's mana
        }

        else if (player.IsOurTurn() && player.deck.CanPlayCard(manaCost) && !card.isEvoCard)
        {
            int index = card.handIndex;
            CardInfo cardInfo = player.deck.hand[index];
            // Debug.LogError(index + " / " + cardInfo.name);
            
            Player.gameManager.isSpawning = true;
            Player.gameManager.isHovering = false;
            //Player.gameManager.CmdOnCardHover(0, index);
            player.deck.CmdPlayCard(cardInfo, index, player); // Summon card onto the board
            player.combat.CmdChangeMana(-manaCost); // Reduce player's mana

        }
    }

    public void UpdateFieldCards()
    {
        int cardCount = content.childCount;
        for (int i = 0; i < cardCount; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();
            
            card.CmdUpdateWaitTurn();
        }
    }

    public void EndTurnFieldCards()
    {
        int cardCount = content.childCount;
        for (int i = 0; i < cardCount; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();
            if(card == null) { continue; }// FieldCard 컴포넌트가 없으면 스킵

            //=턴이 끝날때 버프 턴 카운트를 1씩 줄이고 0이되면 버프로 받은 DP제거=//
            if(card.buffs.Count > 0)
            {
                for(int j = card.buffs.Count - 1; j >= 0; j--)//0번부터 시작하면 꼬임 마지막 인덱스부터 ㄱㄱ
                {
                    card.buffs[j].buffTurn--;
                    Debug.Log(card.buffs[j].buffTurn);
                    if (card.buffs[j].buffTurn == 0)
                    {
                        card.CmdChangeSomeThing(card.buffs[j],false);//딜뻥 버프 제거
                        card.CmdRemoveBuff(j);//그 다음 버프 목록 제거
                    }
                }
            }
            //===============================================================//

            card.CmdDestroySpellCard();
        }
    }

    public int GetFieldCardCount()
    {
        int Count = 0;

        for (int i = 0; i < content.childCount; ++i)
        {
            Transform child = content.GetChild(i);
            FieldCard card = child.GetComponent<FieldCard>();

            if (card != null && card.isUnderMostCard)
            { Count++; }
        }
        return Count;
    }

    void LateUpdate()
    {
        if(Player.localPlayer!= null)
        MemoryChecker.Inst.memoryCheckerPos(); // 카드를 냄에 따른 메모리 변화
    }
}
