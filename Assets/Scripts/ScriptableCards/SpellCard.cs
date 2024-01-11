// Put all our cards in the Resources folder. We use Resources.LoadAll down
// below to load our cards into a cache so we can easily reference them later
using System.Collections.Generic;
using UnityEngine;

public enum SpellType { DP, EVO, MEMORY, DESTROY, SECURITY_CHECK, BACKtoDECK, ACTIVE, DRAW, SECURITY, }

// Struct for cards in your deck. Card + amount (ex : Sinister Strike x3). Used for Deck Building. Probably won't use it, just add amount to Card struct instead.
[CreateAssetMenu(menuName = "Card/Spell Card", order = 111)]
public partial class SpellCard : ScriptableCard
{
    [Header("Propeties")]
    public bool targeted = false; // Targeted or random
    public int healthChange = 0; // If it affects a creature's stats (+X for positive changes like healing, -X for negative changes like damage)
    public int strengthChange = 0; // 
    public int memoryChange = 0;
    public int cardDraw = 0; // Same as health. +X for positive (drawing cards), -X for negative (discarding)
    public bool untilEndOfTurn = false; // If the changes only purposes until end of turn.
    public bool isTamer;
    public SpellType type;
    public bool hasSelectBuff;
    public bool hasSelectSecurityBuff;
    public bool if_Security_Go_Hand;

    [Header("Buff")]
    public Buffs buff;
    public Buffs SecurityBuff;

    [Header("Board Prefab")]
    public FieldCard cardPrefab;

    [Header("Targets")]
    public List<Target> acceptableTargets = new List<Target>();
    public List<Target> acceptableSecurityTargets = new List<Target>();

    public void AppearSpellCard(Player owner)
    {
        if(type == SpellType.MEMORY) 
        {
            switch (cardName)
            {
                case "�׷���Ƽ ������":
                    if (owner.firstPlayer)
                    { MemoryChecker.Inst.memory += 2; }
                    else
                    {  MemoryChecker.Inst.memory -= 2;}
                    break;
                case "������":
                    if (owner == Player.localPlayer)
                    {
                        FindTamerTarget(Player.gameManager.playerField.content);
                    }
                    break;
            }
        }

        if(type == SpellType.DRAW)
        {
            switch (cardName)
            {
                case "��Ŭ���� ������":
                    owner.CmdDrawDeck(2);
                    break;
                case "���� ����":
                    owner.CmdDrawDeck(1);
                    break;
                case "Ȧ�� �����Ĵ�":
                    owner.CmdDrawDeck(owner.deck.securityCard.Count / 2);//��ť��Ƽ2�帶�� ��ο�
                    break;
            }
        }

       if(type == SpellType.MEMORY)
        {
            switch (cardName)
            {
                case "���Ž� ��������":
                    owner.CmdAddBuff(buff);
                    owner.CmdChangeSomeThing(buff,true);
                    break;
            }
        }

        if (type == SpellType.DESTROY)
        {
            switch (cardName)
            {
                case "������� ����":
                    GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
                    //owner.UICardsList = new List<FieldCard>();
                    owner.UICardsList.Clear();
                    bool blockExist = false;
                    for (int i = 0; i < enemyField.transform.childCount; ++i)
                    {
                        FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();

                        if (enemyCard == null) { return; }

                        //if(target�� �÷��̾� �ʵ忡 �ֻ�� ī���� hasBlock�� ������ ��� Ÿ��)
                        if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && ((CreatureCard)enemyCard.card.data).hasBlocker)
                        {
                            owner.UICardsList.Add(enemyCard);
                            
                            //��� ī�忡 CreatureCard�� �ƴ� ���̸ӳ� ����ī�� ������ �����Ƿ� ���ǿ� CreatureCard�ʼ�
                            Debug.Log("�ı� ��� ������ ī�� ����!");
                            blockExist= true;
                        }
                    }
                    
                    if(blockExist)
                    {
                        owner.CmdSyncTargeting(owner, true);
                        owner.CmdSetActiveDestroyPanel(owner);
                    }

                    break;

                case "���̳� �����ÿ�":
                    GameObject enemyField2 = Player.gameManager.enemyField.content.gameObject;
                    GameObject playerField2 = Player.gameManager.playerField.content.gameObject;

                    owner.UICardsList.Clear();
                    bool isTamerExist = false;
                    bool blockExist2 = false;

                    for (int i = 0; i < playerField2.transform.childCount; ++i)
                    {
                        FieldCard myCard = playerField2.transform.GetChild(i).GetComponent<FieldCard>();

                        if (myCard == null) { return; }

                        //�ϴ� ���� �ʵ忡 ����ī�� ���̸Ӱ� �ִ��� üũ
                        if (myCard.isUpperMostCard && myCard.card.data is SpellCard spellCard && spellCard.isTamer && (spellCard.color1 == CardColor.Red || spellCard.color2 == CardColor.Red))
                        {
                            //��� ī�忡 CreatureCard�� �ƴ� ���̸ӳ� ����ī�� ������ �����Ƿ� ���ǿ� CreatureCard�ʼ�
                            Debug.Log("���̸� ����! ����� 8000DP�� ��!");
                            isTamerExist = true;
                        }
                    }

                    if (isTamerExist)
                    {
                        for (int i = 0; i < enemyField2.transform.childCount; ++i)
                        {
                            FieldCard enemyCard = enemyField2.transform.GetChild(i).GetComponent<FieldCard>();

                            if (enemyCard == null) { return; }

                            if(enemyCard.isUpperMostCard &&enemyCard.card.data is CreatureCard && enemyCard.strength <=8000)
                            {
                                owner.UICardsList.Add(enemyCard);
                                blockExist2 = true;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < enemyField2.transform.childCount; ++i)
                        {
                            FieldCard enemyCard = enemyField2.transform.GetChild(i).GetComponent<FieldCard>();

                            if (enemyCard == null) { return; }

                            if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && enemyCard.strength <= 5000)
                            {
                                owner.UICardsList.Add(enemyCard);
                                blockExist2 = true;
                            }
                        }
                    }

                    if (blockExist2)
                    {
                        owner.CmdSyncTargeting(owner, true);
                        owner.CmdSetActiveDestroyPanel(owner);
                    }

                    break;
            }
        }
    }

    public void AppearSecuritySpellCard(Player owner, FieldCard card)
    {
        if (type == SpellType.MEMORY)
        {
            switch (cardName)
            {
                case "�׷���Ƽ ������":
                    //��ť ȿ�� ����
                    break;

                case "������":
                    card.CmdMakeSecurity(false);
                    //StartTamerCast(card.player);
                    if(owner == Player.localPlayer)
                    {
                        FindTamerTarget(Player.gameManager.playerField.content);
                    }
                    break;

                case "�ѼҶ�":
                    card.CmdMakeSecurity(false);
                    break;
            }
        }

        if (type == SpellType.DRAW)
        {
            switch (cardName)
            {
                case "��Ŭ���� ������":
                    //��ť ȿ�� ����
                    break;
                case "���� ����":
                    owner.CmdDrawDeckNotMyTurn(2, owner);
                    break;
                case "Ȧ�� �����Ĵ�":
                    owner.CmdDrawDeckNotMyTurn(owner.deck.securityCard.Count / 2, owner);//��ť��Ƽ2�帶�� ��ο�
                    break;
            }
        }

        if (type == SpellType.DP)
        {
            switch (cardName)
            {
                case "�극�̺� ����̵�":
                    //�տ� �� ī�� �߰�
                    CardInfo cardInfo = new CardInfo();
                    cardInfo.cardID = CardID;
                    //cardInfo.data = ScriptableCard.Cache[cardInfo.cardID];
                    cardInfo.amount = 1;
                    owner.CmdDrawSpecificCard(cardInfo, owner);
                    break;

                case "�� ���̾�":
                    //���� ī�� ��ο�
                    //�ڵ�ο� �� �� �ٸ� �ļ� ��ο� ó���������� ServerOnly����
                    owner.CmdDrawDeckServerOnly(1);

                    //�տ� �� ī�� �߰�
                    CardInfo cardInfo1 = new CardInfo();
                    cardInfo1.cardID = CardID;
                    //cardInfo.data = ScriptableCard.Cache[cardInfo.cardID];
                    cardInfo1.amount = 1;
                    owner.CmdDrawSpecificCard(cardInfo1, owner, 2);
                    break;
                case "ȣ�� ������":
                    CardInfo cardInfo2 = new CardInfo();
                    cardInfo2.cardID = CardID;
                    owner.CmdDrawSpecificCard(cardInfo2, owner);
                    break;
            }
        }

        if (type == SpellType.DESTROY)
        {
            switch (cardName)
            {
                case "������� ����":
                    GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
                    //owner.UICardsList = new List<FieldCard>();
                    owner.UICardsList.Clear();
                    bool blockExist = false;
                    for (int i = 0; i < enemyField.transform.childCount; ++i)
                    {
                        FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();

                        if (enemyCard == null) { return; }

                        //if(target�� �÷��̾� �ʵ忡 �ֻ�� ī���� hasBlock�� ������ ��� Ÿ��)
                        if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && ((CreatureCard)enemyCard.card.data).hasBlocker)
                        {
                            owner.UICardsList.Add(enemyCard);

                            //��� ī�忡 CreatureCard�� �ƴ� ���̸ӳ� ����ī�� ������ �����Ƿ� ���ǿ� CreatureCard�ʼ�
                            Debug.Log("�ı� ��� ������ ī�� ����!");
                            blockExist = true;
                        }
                    }

                    if (blockExist)
                    {
                        owner.CmdSyncTargeting(owner, true);
                        owner.CmdSetActiveDestroyPanel(owner);
                    }

                    break;

                case "���̳� �����ÿ�":
                    GameObject enemyField2 = Player.gameManager.enemyField.content.gameObject;
                    GameObject playerField2 = Player.gameManager.playerField.content.gameObject;

                    owner.UICardsList.Clear();
                    bool isTamerExist = false;
                    bool blockExist2 = false;

                    for (int i = 0; i < playerField2.transform.childCount; ++i)
                    {
                        FieldCard myCard = playerField2.transform.GetChild(i).GetComponent<FieldCard>();

                        if (myCard == null) { return; }

                        //�ϴ� ���� �ʵ忡 ����ī�� ���̸Ӱ� �ִ��� üũ
                        if (myCard.isUpperMostCard && myCard.card.data is SpellCard spellCard && spellCard.isTamer && (spellCard.color1 == CardColor.Red || spellCard.color2 == CardColor.Red))
                        {
                            //��� ī�忡 CreatureCard�� �ƴ� ���̸ӳ� ����ī�� ������ �����Ƿ� ���ǿ� CreatureCard�ʼ�
                            Debug.Log("���̸� ����! ����� 8000DP�� ��!");
                            isTamerExist = true;
                        }
                    }

                    if (isTamerExist)
                    {
                        for (int i = 0; i < enemyField2.transform.childCount; ++i)
                        {
                            FieldCard enemyCard = enemyField2.transform.GetChild(i).GetComponent<FieldCard>();

                            if (enemyCard == null) { return; }

                            if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && enemyCard.strength <= 8000)
                            {
                                owner.UICardsList.Add(enemyCard);
                                blockExist2 = true;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < enemyField2.transform.childCount; ++i)
                        {
                            FieldCard enemyCard = enemyField2.transform.GetChild(i).GetComponent<FieldCard>();

                            if (enemyCard == null) { return; }

                            if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && enemyCard.strength <= 5000)
                            {
                                owner.UICardsList.Add(enemyCard);
                                blockExist2 = true;
                            }
                        }
                    }

                    if (blockExist2)
                    {
                        owner.CmdSyncTargeting(owner, true);
                        owner.CmdSetActiveDestroyPanel(owner);
                    }

                    break;
            }
        }
    }

    public void AttackPlayerSpellCardCast(Entity attacker, Player owner)
    {
        switch (cardName)
        {
            case "�ѼҶ�":
                Debug.Log("�ѼҶ� ���� ���");
                owner.CmdSetActiveBuffPanel(attacker, owner,true);
                break;
        }
    }

    public void EndTurnEffect(Player owner)
    {
        if (type == SpellType.MEMORY)
        {
            switch (cardName)
            {
                case "�׷���Ƽ ������":
                    if (owner.firstPlayer)
                    { 
                        MemoryChecker.Inst.memory -= 2; 
                        
                        if(MemoryChecker.Inst.memory < -10)
                        {   
                            //�޸� -10�Ѿ -11���ϰ� �Ǹ� -10���� ����
                            MemoryChecker.Inst.memory = -10; 
                        }
                    }
                    else
                    { 
                        MemoryChecker.Inst.memory += 2; 

                        if (MemoryChecker.Inst.memory > 10)
                        {
                            //�޸� 10�Ѿ 11�̻��� �Ǹ� 10���� ����
                            MemoryChecker.Inst.memory = 10;
                        }
                    }
                    break;
            }
        }
    }

    public override void StartCast(Entity caster, Entity target)
    {
        base.StartCast(caster, target);

        while (target.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            target = target.GetComponent<FieldCard>().upperCard; //�ֻ��� ī�� ��������
        }

        if (caster.GetComponent<FieldCard>().isSecurity == false)
        {
            //�Ϲ� �ɼ�ī�� ȿ�����
            target.GetComponent<FieldCard>().CmdChangeSomeThing(buff, true);
            target.GetComponent<FieldCard>().CmdAddBuff(buff);
        }
        else
        {
            //��ť �ɼ�ī�� ȿ�����
            target.GetComponent<FieldCard>().CmdChangeSomeThing(SecurityBuff, true);
            target.GetComponent<FieldCard>().CmdAddBuff(SecurityBuff);
        }
        --caster.GetComponent<FieldCard>().buffTargetCount;

        if (caster.GetComponent<FieldCard>().buffTargetCount == 0)
        { caster.DestroyTargetingArrow(); }
    }
    public override void EndCast(Entity caster, Entity target)
    {
    }
    
    public void StartTamerCast(Player player)
    {
        if(cardName=="������")
        {
            if(player.firstPlayer && MemoryChecker.Inst.memory <=2)
            {
                MemoryChecker.Inst.CmdChangeMemory(3);
            }
            else if(!player.firstPlayer && MemoryChecker.Inst.memory>=-2)
            {
                MemoryChecker.Inst.CmdChangeMemory(-3);
            }
        }
    }
    public void FindTamerTarget(Transform content)
    {
        int count = content.childCount;

        for(int i=0; i< count; ++i)
        {
            FieldCard card = content.GetChild(i).GetComponent<FieldCard>();

            switch(cardName)
            {
                case "������":
                    if (card == null) { return; }
                    if (card.evoCount >= 4)
                    {
                        if (card.player.IsOurTurn())
                        {
                            card.CmdAddBuff(buff);
                            card.CmdChangeSomeThing(buff, true);
                        }
                        else
                        {
                            //card.tempBuff.securityAttack = 0;
                            card.CmdChangeSomeThing(buff, false);
                            Debug.Log(card.card.data.cardName);
                            
                            for(int a=card.buffs.Count-1; a>=0; --a)
                            {
                                if(card.buffs[a].cardname==buff.cardname)
                                {
                                    //������ �������������� �����ؼ� buff==card.buff�� �ɼ� ����(���� --�Ǹ鼭 �ٸ��� �Ǳ� ����)
                                    //�� �̸����� ������������ ã�Ƽ� �ε����� ���� 
                                    card.CmdRemoveBuff(a);
                                }
                            }
                            //card.CmdRemoveBuff(buff);
                        }
                    }
                    break;
            }
            
        }
    }

    public void FindTamerTarget(FieldCard card)
    {
        //��� �ʵ�ī�尡 �ƴ� Ư�� ī�忡 ���̸� ȿ�� �ο��ɰ��� üũ�ϴ� �Լ�

        switch (cardName)
        {
            case "������":
                if (card == null) { return; }
                if (card.evoCount >= 4)
                {
                    if (card.player.IsOurTurn())
                    {
                        card.CmdAddBuff(buff);
                        card.CmdChangeSomeThing(buff, true);
                    }
                    else
                    {
                        card.CmdChangeSomeThing(buff, false);

                        for (int a = card.buffs.Count - 1; a >= 0; --a)
                        {
                            if (card.buffs[a].cardname == buff.cardname)
                            {
                                //������ �������������� �����ؼ� buff==card.buff�� �ɼ� ����(���� --�Ǹ鼭 �ٸ��� �Ǳ� ����)
                                //�� �̸����� ������������ ã�Ƽ� �ε����� ���� 
                                card.CmdRemoveBuff(a);
                            }
                        }
                        //card.CmdRemoveBuff(buff);
                    }
                }
                break;
        }
    }
}