using UnityEngine.EventSystems;
using UnityEngine;


public class PlayerField : MonoBehaviour, IDropHandler
{
    public Transform content;

    public void OnDrop(PointerEventData eventData)
    {
        if (Player.localPlayer.enemyInfo.data.isTargeting) //�̰� ���� ������ �÷��̾ ������..
        {
            Debug.Log(Player.localPlayer.enemyInfo.data.username);
            return; 
        } //test ��밡 Ÿ�������̸� ����
        if (Player.localPlayer.isTargeting) { return; }

        if (eventData.pointerDrag.transform.GetComponent<FieldCard>() != null && 
            eventData.pointerDrag.transform.GetComponent<FieldCard>().casterType == Target.MY_BABY) 
        {
            FieldCard uppestCard = eventData.pointerDrag.transform.GetComponent<FieldCard>(); //�̸� �ʹ� ��ϱ� ������ �ް�
            
            while(!uppestCard.isUpperMostCard)
            {
                uppestCard = uppestCard.upperCard; //�ֻ�� ī�� ��������
            }

            if(Player.gameManager.isDigitamaOpenOrMove) { return; } // ����Ÿ�� ������ ���̸� ����
            if (((CreatureCard)uppestCard.card.data).level<3) { return; } //�ʵ忡 �������� ī�尡 ����3�̸��̸� ����

            //�ʵ�ī��(������ ī��)�� ����ϸ�
            Debug.Log("���� ī�� �� ���");
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

        if(Player.localPlayer.isServer && MemoryChecker.Inst.memory - manaCost < -10) { return; } //�� �ڽ�Ʈ�� �����ϸ� return
        else if(!Player.localPlayer.isServer && MemoryChecker.Inst.memory + manaCost > 10) { return; }

        if (player.IsOurTurn() && player.deck.CanPlayCard(manaCost) && card.isEvoCard)
        {
            int index = card.handIndex;
            CardInfo cardInfo = player.deck.hand[index];

            Player.gameManager.isSpawning = true;
            Player.gameManager.isHovering = false;
            //Player.gameManager.CmdOnCardHover(0, index);

            player.deck.CmdPlayEvoCard(cardInfo, index, player, card.underCard); // Summon card onto the board
            player.CmdDrawDeck(1); // ��ȭ��Ű�� ���� ���� ��ο�(�ڽ�Ʈ ������� �־����) ���� �̴�� �Ѱ�

            if(player.smashPotato && ((CreatureCard)card.cardInfo.data).level == 6 /*&& (((CreatureCard)card.cardInfo.data).color1==CardColor.Green || ((CreatureCard)card.cardInfo.data).color2 == CardColor.Green)*/)
            {
                //���Ž� �������� ȿ�� üũ
                int reduceCost = (manaCost - 4);

                if (reduceCost <= 0) { reduceCost = 0; }

                player.combat.CmdChangeMana(-reduceCost);
                //���� ������ false�� �ǵ����ֱ�
                player.smashPotato = false;
            }

            else
            {
                //�� �ܿ� �Ϲ� �ڽ�Ʈ ����
                player.combat.CmdChangeMana(-manaCost); // Reduce player's mana
            }
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
        //�÷��̾� ���� ����

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
        
        

        //�� ī�庰 ��������
        int cardCount = content.childCount;
        for (int i = 0; i < cardCount; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();
            if(card == null) { continue; }// FieldCard ������Ʈ�� ������ ��ŵ

            //=���� ������ ���� �� ī��Ʈ�� 1�� ���̰� 0�̵Ǹ� ������ ���� DP����=//
            if(card.buffs.Count > 0)
            {
                for(int j = card.buffs.Count - 1; j >= 0; j--)//0������ �����ϸ� ���� ������ �ε������� ����
                {
                    card.buffs[j].buffTurn--;
                    Debug.Log(card.buffs[j].buffTurn);
                    if (card.buffs[j].buffTurn == 0)
                    {
                        card.CmdChangeSomeThing(card.buffs[j],false);//���� ���� ����
                        card.CmdRemoveBuff(j);//�� ���� ���� ��� ����
                    }
                }
            }

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
        MemoryChecker.Inst.memoryCheckerPos(); // ī�带 ���� ���� �޸� ��ȭ
    }
}
