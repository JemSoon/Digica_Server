using UnityEngine.EventSystems;
using UnityEngine;

public class PlayerField : MonoBehaviour, IDropHandler
{
    public Transform content;
    public Transform raiseContent;

    public void OnDrop(PointerEventData eventData)
    {
        if (Player.localPlayer.enemyInfo.data.isTargeting) //이게 현재 서버인 플레이어만 가져옴..
        {
            Debug.Log(Player.localPlayer.enemyInfo.data.username);
            return; 
        } //test 상대가 타게팅중이면 리턴
        if (Player.localPlayer.isTargeting) { return; }

        #region 육성카드 필드로
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
        #endregion

        #region 핸드카드 필드로
        HandCard card = eventData.pointerDrag.transform.GetComponent<HandCard>();
        Player player = Player.localPlayer;
        int manaCost;

        if (card == null) { return; }//필드카드도 육성에서 여기다 놓을수도 있기에 핸드카드만 받게끔

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

            if(player.smashPotato && ((CreatureCard)card.cardInfo.data).level == 6 /*&& (((CreatureCard)card.cardInfo.data).color1==CardColor.Green || ((CreatureCard)card.cardInfo.data).color2 == CardColor.Green)*/)
            {
                //스매시 포테이토 효과 체크
                int reduceCost = (manaCost - 4);

                if (reduceCost <= 0) { reduceCost = 0; }

                player.combat.CmdChangeMana(-reduceCost);
                //버프 썻으니 false로 되돌려주기
                player.smashPotato = false;
            }

            else
            {
                //그 외엔 일반 코스트 차감
                player.combat.CmdChangeMana(-manaCost); // Reduce player's mana
            }
        }

        else if (player.IsOurTurn() && player.deck.CanPlayCard(manaCost) && !card.isEvoCard)
        {
            int index = card.handIndex;
            CardInfo cardInfo = player.deck.hand[index];
            // Debug.LogError(index + " / " + cardInfo.name);

            if (cardInfo.data is SpellCard spellCard && spellCard.isTamer == false)
            {
                if(content.childCount == 0) { return; } //아무것도 없는 필드에서 옵션카드 낼 수 없다

                for (int i = 0; i < content.childCount; i++)
                {
                    FieldCard myCard = content.GetChild(i).gameObject.GetComponent<FieldCard>();

                    if (myCard == null || (myCard.card.data.color1 != spellCard.color1 && myCard.card.data.color1 != spellCard.color2 && myCard.card.data.color2 != spellCard.color1 && myCard.card.data.color2 != spellCard.color2))
                    {
                        Debug.Log("카드 생성 반환");
                        //테이머가 아닌 옵션카드가 필드에 나가려는데 필드에 카드가없거나 다른 같은색 카드가 없다면 사용 불가
                        return;
                    }
                }
            }
            Debug.Log("카드 생성");
            Player.gameManager.isSpawning = true;
            Player.gameManager.isHovering = false;
            //Player.gameManager.CmdOnCardHover(0, index);
            player.deck.CmdPlayCard(cardInfo, index, player); // Summon card onto the board
            player.combat.CmdChangeMana(-manaCost); // Reduce player's mana

        }
        #endregion
    }

    public void UpdateFieldCards()
    {
        UpdateTamerEffect();

        int cardCount = content.childCount;
        for (int i = 0; i < cardCount; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();
            
            if(card.card.data is SpellCard spellCard && spellCard.isTamer)
            {
                //테이머 턴 개시시 효과 발동
                spellCard.StartTamerCast(card.player);
            }

            //else if (card.card.data is CreatureCard creatureCard && creatureCard.evolutionType.Exists(evo => evo == EvolutionType.MYTURN))
            //{
            //    creatureCard.MyTurnCast(card, card.upperCard);
            //}

            //if (card.card.data is CreatureCard creatureCard && card.isUpperMostCard)
            //{
            //    //테이머 턴 개시시 효과 발동
            //    creatureCard.DigimonCast(card);
            //}
            //card.CmdDigimonCast();

            card.CmdUpdateWaitTurn();
        }
    }

    public void UpdateTamerEffect()
    {
        int cardCount = content.childCount;
        for (int i = 0; i < cardCount; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();

            if (card.card.data is SpellCard spellCard && spellCard.isTamer)
            {
                //테이머 턴개시,끝날시 발동될 효과
                spellCard.FindTamerTarget(content);
                spellCard.FindTamerTarget(raiseContent);
            }
        }
    }

    public void UpdateTurnEvoEffect()
    {
        int cardCount = content.childCount;
        for (int i = 0; i < cardCount; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();
            if(card==null) { return; }

            if (card.card.data is CreatureCard creatureCard && creatureCard.evolutionType.Exists(evo => evo == EvolutionType.MYTURN) && card.upperCard!=null)
            {
                creatureCard.MyTurnCast(card, card.upperCard);
            }
        }
    }

    public void UpdateDigimonEffect()
    {
        int cardCount = content.childCount;
        for (int i = 0; i < cardCount; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();
            if (card == null) { return; }

            if (card.card.data is CreatureCard creatureCard && card.isUpperMostCard)
            {
                creatureCard.DigimonCast(card);
            }
        }
    }

    public void EndBuffTurnSpellCards()
    {
        //플레이어 버프 제거
        Player players = Player.localPlayer;

        if (players.buffs.Count > 0)
        {
            for (int z = players.buffs.Count - 1; z >= 0; z--)
            {
                players.buffs[z].buffTurn--;
                Debug.Log(players.username +" "+ players.buffs[z].buffTurn);
                if (players.buffs[z].buffTurn == 0)
                {
                    players.CmdChangeSomeThing(players.buffs[z], false);
                    players.CmdRemoveBuff(z);
                }
            }
        }
        

        //각 카드별 버프제거
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
                    Debug.Log(card.buffs[j].buffTurn + card.buffs[j].cardname);
                    if (card.buffs[j].buffTurn == 0)
                    {
                        card.CmdChangeSomeThing(card.buffs[j],false);//딜뻥 버프 제거
                        card.CmdRemoveBuff(j);//그 다음 버프 목록 제거
                    }
                }
            }

            card.CmdDestroySpellCard();
        }

        UpdateTurnEvoEffect();
        UpdateDigimonEffect();
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
