// Put all our items in the Resources folder. We use Resources.LoadAll down
// below to load our items into a cache so we can easily reference them.
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum CreatureType : byte { ATTACK, EVO,  }
public enum EvolutionType : byte { ATTACK, EVO, MYTURN, }


// Struct for cards in your deck. Card + amount (ex : Sinister Strike x3). Used for Deck Building. Probably won't use it, just add amount to Card struct instead.
[CreateAssetMenu(menuName = "Card/Creature Card", order = 111)]
public partial class CreatureCard : ScriptableCard
{
    [Header("Stats")]
    public int strength;
    public int health;
    public int Ecost;
    public int level;

    [Header("Targets")]
    public List<Target> acceptableTargets = new List<Target>();
    public List<Target> buffTargets = new List<Target>();

    [Header("Type")]
    public List<CreatureType> creatureType;
    public List<EvolutionType> evolutionType;

    [Header("Specialities")]
    public bool hasCharge = false;
    public bool hasTaunt = false;
    public bool hasBlocker = false;
    public bool hasSpear = false;
    public bool hasJamming = false;
    public bool makeSecurityEffectNull = false;//���׷��̸��� ��ť��Ƽ ��ȿȭ�� Ȯ���ϴ�

    [Header("Death Abilities")]
    public List<CardAbility> deathcrys = new List<CardAbility>();
    [HideInInspector] public bool hasDeathCry = false; // If our card has a DEATHCRY ability

    [Header("Board Prefab")]
    public FieldCard cardPrefab;

    [Header("Buff")]
    public Buffs buff;//������ ����
    public Buffs evolutionBuff;//��ȭ�� ����

    public virtual void Attack(Entity attacker, Entity target)
    {
        //����Ŀ�� Ÿ���� �ַο� ���� ����(���߿� �����ϴµ� ����Ŀ�� ������ �ٽ� �ַο� ���涧 Ÿ������ �ȵ�) 
        attacker.DestroyTargetingArrow();
        //���� ���Ŀ�� �ִ��� Ȯ��
        GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
        bool foundBlocker = false; // �߰�: ��� Ÿ�� ������ ī�带 ã�Ҵ��� ����
        for (int i = 0; i < enemyField.transform.childCount; ++i)
        {
            FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();


            //if(target�� �÷��̾� �ʵ忡 �ֻ�� ī���� hasBlock�� �ְ� ��ť�ƴϰ� Ÿ��!=���ī�� ��� ��� Ÿ��)
            if (enemyCard.isUpperMostCard && !enemyCard.isSecurity && enemyCard.card.data is CreatureCard && ((CreatureCard)enemyCard.card.data).hasBlocker && !enemyCard.attacked && target!=enemyCard)
            {
                foundBlocker = true;
                Player.gameManager.CmdSyncTarget(target);
                //���Ÿ��
                Debug.Log("��� Ÿ�� ������ ī�� ����!");
                enemyCard.CmdSyncTargeting(enemyCard.player, true);

                enemyCard.player.CmdSetActiveBlockPanel(enemyCard.player, attacker, target);

                break;
            }
        }

        if (target is Player user)
        {
            if(!foundBlocker)
            {
                //attacker.GetComponent<FieldCard>().CmdIsAttacking(true);
                //���ݴ���� �÷��̾��� ��ť��Ƽ ī��[0]���� �� �װͰ� ����
                Debug.Log("��ť��Ƽ ī�� ����");
                if (user.deck.securityCard.Count > 0)
                {
                    //Debug.Log("������ ȿ�� ������ ��� ��ťī�� ���� : "+user.deck.securityCard.Count);
                    AttackDigimonCast(attacker.GetComponent<FieldCard>(),null);//1��7�� �׽�Ʈ
                    //Debug.Log("������ ȿ�� �� ��(AttckDigimonCast���� ��) ��� ��ťī�� ���� : " + user.deck.securityCard.Count);
                    attacker.combat.CmdBattle(attacker, target);//�̰� �߹޾� ����..?
                }
                else
                {
                    //���� ���� attacker�� �¸�
                    Player.gameManager.CmdEndGame(attacker);
                    Debug.Log("���� ���� " + attacker.GetComponentInParent<FieldCard>().player.username + "�� �¸�!");
                }
            }
        }

        else
        {
            AttackDigimonCast(attacker.GetComponent<FieldCard>(), null);
            if (!foundBlocker || ((FieldCard)target).isSecurity)
            {
                //���Ŀ�� ��� �ʵ忡 ���ٰų� ��ť�������� ���� Ÿ��ī��� �̹� ��Ͽ��θ� Ȯ���Ѱ��̱⿡ �Ϲ� ���� ����
                attacker.combat.CmdBattle(attacker, target);
            }
        }

        //attacker.DestroyTargetingArrow();//���� ��ġ������..

        if (attacker.CanAttack())
        {
            attacker.combat.CmdIncreaseWaitTurn();
            attacker.GetComponentInParent<FieldCard>().CmdChangeAttacked(true);
        }
        else if (attacker.CantAttack() && ((FieldCard)attacker).securityAttack > 0)
        {
            attacker.combat.CmdReduceSecurityAttack();
        }
    }

    public bool isSameColor(CreatureCard card1, CreatureCard card2)
    {
        if (card1.color1 == card2.color1 ||
           card1.color2 == card2.color2 ||
           card1.color2 == card2.color1 ||
           card1.color2 == card2.color1)
        {
            return true;
        }
        else
        { return false; }
    }

    private void OnValidate()
    {
        if (deathcrys.Count > 0) hasDeathCry = true;

        // By default, all creatures can only attack enemy creatures and our opponent. We set it here so every card get it's automatically.
        if (acceptableTargets.Count == 0)
        {
            acceptableTargets.Add(Target.ENEMIES);
            acceptableTargets.Add(Target.OPPONENT);
        }
    }

    public override void StartCast(Entity caster, Entity target)
    {
        base.StartCast(caster, target);

        while (target.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            target = target.GetComponent<FieldCard>().upperCard; //�ֻ��� ī�� ��������
        }

        target.GetComponent<FieldCard>().CmdChangeSomeThing(buff, true);
        target.GetComponent<FieldCard>().CmdAddBuff(buff);

        caster.DestroyTargetingArrow();
    }

    public void AttackCast(FieldCard caster, FieldCard buffTarget)//��ȭ�� ĳ��Ʈ
    {
        //���� Ÿ�� �ֻ�� ����
        buffTarget = caster.upperCard;
        while (buffTarget != buffTarget.isUpperMostCard)
        {
            buffTarget = buffTarget.upperCard;
        }

        switch (cardName)
        {
            case "��ϸ�":

                if(caster.isMyTurnEvoCastingActive==false)
                {
                    buffTarget.CmdChangeSomeThing(evolutionBuff, true);
                    buffTarget.CmdAddBuff(evolutionBuff);
                    caster.isMyTurnEvoCastingActive = true;
                }
                //buffTarget.combat.CmdAfterBattle(caster, battleTarget);
                break;

            case "�׶���":
                GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
                //caster.player.UICardsList = new List<FieldCard>();
                caster.player.UICardsList.Clear();
                Player player = caster.player;

                bool DestroytargetOn = false;//��� �ִ��� ������ Ȯ�ο� ��������

                for (int i = 0; i < enemyField.transform.childCount; ++i)
                {
                    FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();

                    if (enemyCard == null) { return; }

                    //if(target�� �÷��̾� �ʵ忡 �ֻ�� ī���� ���� DP�� 2000���ϸ� �Ҹ�)
                    if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && enemyCard.strength <= 2000)
                    {
                        //��� ī�忡 CreatureCard�� �ƴ� ���̸ӳ� ����ī�� ������ �����Ƿ� ���ǿ� CreatureCard�ʼ�
                        caster.player.UICardsList.Add(enemyCard);
                        Debug.Log("�ı� ��� ������ ī�� ����!");
                        DestroytargetOn = true;
                    }
                }
                if (DestroytargetOn == false)
                {
                    Debug.Log("�ı� ��� ������ ī�� ����");
                    break;
                }
                player.CmdSyncTargeting(player, true);
                player.CmdSetActiveDestroyPanel(player);
                break;
        }
    }

    public void AttackDigimonCast(FieldCard caster, FieldCard target)//������ ĳ��Ʈ
    {
        if (caster.isUpperMostCard == false)
        { return; }

        switch (cardName)
        {
            case "��Ż�׷��̸�":
                if (caster.player.isServer)
                {
                    if (caster.player.IsOurTurn())
                    {
                        MemoryChecker.Inst.buffMemory += 3;
                        MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + 3);
                    }
                }
                else
                {
                    if (caster.player.IsOurTurn())
                    {
                        MemoryChecker.Inst.buffMemory -= 3;
                        MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory - 3);
                    }
                }
                break;

            case "��Ż�׷��̸�(û)":
                if (caster.player.isServer)
                {
                    if (caster.player.IsOurTurn() && caster.isMyTurnDigimonCastingActive == false)
                    {
                        MemoryChecker.Inst.CmdChangeMemorySameSync(MemoryChecker.Inst.memory - 5);
                        caster.isMyTurnDigimonCastingActive = true;
                    }
                }
                else
                {
                    if (caster.player.IsOurTurn() && caster.isMyTurnDigimonCastingActive == false)
                    {
                        MemoryChecker.Inst.CmdChangeMemorySameSync(MemoryChecker.Inst.memory + 5);
                        caster.isMyTurnDigimonCastingActive = true;
                    }
                }
                break;

            case "��ũ��":
                int Count = caster.player.enemyInfo.graveCount / 10;

                for (int i = 0; i < Count; ++i)
                {
                    if (caster.player.enemyInfo.graveCount >= 0)
                    { 
                        caster.player.enemyInfo.data.deck.CmdBreakSecurityCard(caster.player.enemyInfo.data.deck.securityCard[0], caster.player.enemyInfo.data); 
                    }
                }
                break;
        }
    }

    public void MyTurnCast(FieldCard caster, FieldCard target)
    {
        switch (cardName)
        {
            case "�׷��̸�":

                if (caster.upperCard == null) { return; }

                target = caster.upperCard;
                while (target != target.isUpperMostCard)
                {
                    target = target.upperCard;
                }

                if (target.casterType == Target.MY_BABY) { return; }

                if (caster.player.IsOurTurn())
                {
                    if(!target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname) && caster.isMyTurnEvoCastingActive==false)
                    {
                        //������Ͽ� ������ �߰�
                        target.CmdChangeSomeThing(evolutionBuff, true);
                        target.CmdAddBuff(evolutionBuff);
                        caster.isMyTurnEvoCastingActive = true;
                    }
                }
                else
                {
                    caster.isMyTurnEvoCastingActive = false;
                    //if (target.buffs.Count > 0)
                    if (target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname))
                    {
                        {
                            //�� ���� �ƴѵ��� �׷��̸� ���� ã�� ����
                            target.CmdChangeSomeThing(evolutionBuff, false);
                            target.CmdRemoveBuff(evolutionBuff.cardname);
                            
                        }
                    }
                }
                break;

            case "��Ż�׷��̸�(û)":
                if (caster.upperCard == null) { return; }

                target = caster.upperCard;
                while (target != target.isUpperMostCard)
                {
                    target = target.upperCard;
                }

                if (target.casterType == Target.MY_BABY) { return; }

                if (caster.player.IsOurTurn())
                {
                    if (!target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname) && caster.isMyTurnEvoCastingActive == false)
                    {
                        //������Ͽ� ������ �߰�
                        target.CmdChangeSomeThing(evolutionBuff, true);
                        target.CmdAddBuff(evolutionBuff);
                        caster.isMyTurnEvoCastingActive = true;
                    }
                }
                else
                {
                    caster.isMyTurnEvoCastingActive = false;
                    if (target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname))
                    {
                        {
                            Debug.Log("upī�� ������ ���Ͻ��״µ� �� ������");
                            target.CmdChangeSomeThing(evolutionBuff, false);
                            target.CmdRemoveBuff(evolutionBuff.cardname);
                            
                        }
                    }
                }
                break;

            case "���̺���":
                if (caster.upperCard == null) { return; }

                target = caster.upperCard;
                while (target != target.isUpperMostCard)
                {
                    target = target.upperCard;
                }

                if (target.casterType == Target.MY_BABY) { return; }

                if (caster.player.IsOurTurn())
                {
                    if (((CreatureCard)target.card.data).hasSpear && (!target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname)))
                    {
                        //FieldCard�� CreatureCard�� isSpear������ ���� bool�� �޾ƾ��� 
                        target.CmdChangeSomeThing(evolutionBuff, true);
                        target.CmdAddBuff(evolutionBuff);
                        caster.isMyTurnEvoCastingActive = true;
                    }
                }
                else
                {
                    caster.isMyTurnEvoCastingActive = false;
                    if (((CreatureCard)target.card.data).hasSpear && target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname))
                    {
                        {
                            //�� ���� �ƴѵ��� �׷��̸� ���� ã�� ����
                            target.CmdChangeSomeThing(evolutionBuff, false);
                            target.CmdRemoveBuff(evolutionBuff.cardname); 
                        }
                    }
                }
                break;

            case "���":
                if (caster.upperCard == null) { return; }

                target = caster.upperCard;
                while (target != target.isUpperMostCard)
                {
                    target = target.upperCard;
                }

                if (target.casterType == Target.MY_BABY) { return; }

                if (caster.player.IsOurTurn())
                {
                    if (caster.player.enemyInfo.data.deck.graveyard.Count >= 5)
                    {
                        //��� Ʈ���ð� 5�� �̻��϶� �ߵ�
                        if(!target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname))
                        {
                            target.CmdChangeSomeThing(evolutionBuff, true);
                            target.CmdAddBuff(evolutionBuff);
                            caster.isMyTurnEvoCastingActive = true;
                        }
                    }
                    else
                    {
                        caster.isMyTurnEvoCastingActive = false;
                        //��� Ʈ���ð� 5�� �̸��� �Ǿ��ٸ� ��� ���� ����
                        if (target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname))
                        {
                            target.CmdChangeSomeThing(evolutionBuff, false);
                            target.CmdRemoveBuff(evolutionBuff.cardname);
                        }
                    }
                }
                else
                {
                    caster.isMyTurnEvoCastingActive = false;
                    if (target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname))
                    {
                        target.CmdChangeSomeThing(evolutionBuff, false);
                        target.CmdRemoveBuff(evolutionBuff.cardname);
                    }
                }
                break;

            case "�ް��α׶���":
                if (caster.upperCard == null) { return; }

                target = caster.upperCard;
                while (target != target.isUpperMostCard)
                {
                    target = target.upperCard;
                }

                if (target.casterType == Target.MY_BABY) { return; }

                if (caster.player.IsOurTurn())
                {
                    if (caster.player.enemyInfo.data.deck.graveyard.Count >= 5)
                    {
                        //��� Ʈ���ð� 5�� �̻��϶� �ߵ�
                        if (!target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname))
                        {
                            target.CmdChangeSomeThing(evolutionBuff, true);
                            target.CmdAddBuff(evolutionBuff);
                            caster.isMyTurnEvoCastingActive = true;
                        }
                    }
                    else
                    {
                        caster.isMyTurnEvoCastingActive = false;
                        //��� Ʈ���ð� 5�� �̸��� �Ǿ��ٸ� ��� ���� ����
                        if (target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname))
                        {
                            target.CmdChangeSomeThing(evolutionBuff, false);
                            target.CmdRemoveBuff(evolutionBuff.cardname);
                        }
                    }
                }
                else
                {
                    caster.isMyTurnEvoCastingActive = false;
                    if (target.buffs.Any(buff => buff.cardname == evolutionBuff.cardname))
                    {
                        target.CmdChangeSomeThing(evolutionBuff, false);
                        target.CmdRemoveBuff(evolutionBuff.cardname);
                    }
                }
                break;
        }
    }

    public void MyTurnDigimonCast(FieldCard caster, FieldCard target)
    {
        switch (cardName)
        {
            case "ȭ�����ڸ�":
                if(!caster.isUpperMostCard) { return; }//�ֻ�� ī�尡 �ƴϸ� ����

                if (caster.player.IsOurTurn())
                {
                    if (caster.player.firstPlayer)
                    {
                        if (MemoryChecker.Inst.memory >= 3 && !target.buffs.Any(buffs => buffs.cardname == buff.cardname) && caster.isMyTurnDigimonCastingActive == false)
                        {
                            //���� ȣ��Ʈ�̰� �޸𸮰� 3�̻��̶��
                            target.CmdChangeSomeThing(buff, true);
                            target.CmdAddBuff(buff);
                            caster.isMyTurnDigimonCastingActive = true;
                        }
                        else if(MemoryChecker.Inst.memory < 3 && target.buffs.Any(buffs => buffs.cardname == buff.cardname) && caster.isMyTurnDigimonCastingActive)
                        {
                            target.CmdChangeSomeThing(buff, false);
                            target.CmdRemoveBuff(buff.cardname);
                            caster.isMyTurnDigimonCastingActive = false;
                        }
                    }
                    else
                    {
                        //������ ȣ��Ʈ�̰� �޸𸮰� -3���� �۴ٸ� �����ο�
                        if (MemoryChecker.Inst.memory <= -3 && !target.buffs.Any(buffs => buffs.cardname == buff.cardname) && caster.isMyTurnDigimonCastingActive == false)
                        {
                            target.CmdChangeSomeThing(buff, true);
                            target.CmdAddBuff(buff);
                            caster.isMyTurnDigimonCastingActive = true;
                        }
                        else if(MemoryChecker.Inst.memory > -3 && target.buffs.Any(buffs => buffs.cardname == buff.cardname) && caster.isMyTurnDigimonCastingActive)
                        {
                            target.CmdChangeSomeThing(buff, false);
                            target.CmdRemoveBuff(buff.cardname);
                            caster.isMyTurnDigimonCastingActive = false;
                        }
                    }
                }
                else
                {
                    if (target.buffs.Any(buffs => buffs.cardname == buff.cardname))
                    {
                        target.CmdChangeSomeThing(buff, false);
                        target.CmdRemoveBuff(buff.cardname);
                        caster.isMyTurnDigimonCastingActive = false;
                    }
                }
                break;
        }
    }

    public void AppearDigimonCast(FieldCard caster)
    {
        switch (cardName)
        {
            case "���ñ׷��̸�":
                GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
                //caster.player.UICardsList = new List<FieldCard>();
                caster.player.UICardsList.Clear();
                for (int i = 0; i < enemyField.transform.childCount; ++i)
                {
                    FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();

                    if (enemyCard == null) { return; }

                    //if(target�� �÷��̾� �ʵ忡 �ֻ�� ī���� hasBlock�� ������ ��� Ÿ��)
                    if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && ((CreatureCard)enemyCard.card.data).hasBlocker)
                    {
                        caster.player.UICardsList.Add(enemyCard);
                        //��� ī�忡 CreatureCard�� �ƴ� ���̸ӳ� ����ī�� ������ �����Ƿ� ���ǿ� CreatureCard�ʼ�
                        Debug.Log("�ı� ��� ������ ī�� ����!");
                    }
                }
                caster.CmdSyncTargeting(caster.player, true);
                caster.player.CmdSetActiveDestroyPanel(caster.player);
                break;

            case "������":

                //�������� ������ ��������� �Ѿ�� �������
                if(MemoryChecker.Inst.memory < 0 && caster.player.isServer)
                { return; }
                else if(MemoryChecker.Inst.memory > 0 && !caster.player.isServer)
                { return; }
                caster.SpawnTargetingArrow(caster.card, true);

                break;

            case "���ɴе���":
                GameObject enemyField2 = Player.gameManager.enemyField.content.gameObject;

                for (int i = 0; i < enemyField2.transform.childCount; ++i)
                {
                    FieldCard enemyCard = enemyField2.transform.GetChild(i).GetComponent<FieldCard>();

                    if (enemyCard == null) { return; }

                    //�ϴ� ���� �ʵ忡 ����ī�� ���̸Ӱ� �ִ��� üũ
                    if (enemyCard.isUpperMostCard && enemyCard.strength<=4000)
                    {
                        while(!enemyCard.isUnderMostCard)
                        {
                            //��ȭ�� ���ʴ�� ���������� ����
                            enemyCard.player.deck.CmdAddGraveyard(enemyCard.player, enemyCard.card);
                            enemyCard.CmdDestroyCard(enemyCard);
                            enemyCard = enemyCard.underCard;
                        }
                        //������ ���ϴ� ī�嵵 ����
                        enemyCard.player.deck.CmdAddGraveyard(enemyCard.player, enemyCard.card);
                        enemyCard.CmdDestroyCard(enemyCard);
                    }
                }
                break;

            case "�Ʊ��� �ڻ�":
                List<CardInfo> filteredGraveyard = caster.player.deck.graveyard.Where(card => card.data.name.Contains("�Ʊ���")).ToList();
                caster.player.CmdClearUICardInfo();
                
                if( filteredGraveyard.Count > 0 )
                {
                    //������ �Ʊ��� ī�尡 1�� �̻��̶��
                    for (int i = 0; i < filteredGraveyard.Count; ++i)
                    {
                        //caster.player.UICardInfoList.Add(filteredGraveyard[i]);
                        caster.player.CmdAddUICardInfo(filteredGraveyard[i]);
                    }
                    caster.CmdSyncTargeting(caster.player, true);
                    Player.gameManager.CmdSyncCaster(caster);
                    caster.player.CmdSetActiveRevivePanel(caster.player, true);
                }
                break;

            case "�Ʊ���":
                caster.player.CmdClearUICardInfo();

                if(caster.player.deck.deckList.Count > 0 )
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        if (caster.player.deck.deckList.Count > 0)
                        {
                            caster.player.CmdAddUICardInfoAndRemoveDeckList();
                        }
                    }
                    caster.CmdSyncTargeting(caster.player, true);
                    Player.gameManager.CmdSyncCaster(caster);
                    caster.player.CmdSetActivePickUpPanel(caster.player, true);
                }
                break;
        }
    }

    public void EvoDigimonCast(FieldCard caster)
    {
        switch (cardName)
        {
            case "�ް��α׶���":
                caster.player.UICardsList.Clear();

                GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
                GameObject playerField = Player.gameManager.playerField.content.gameObject;

                bool isTamerExist = false;
                bool destroyExist = false;

                for (int i = 0; i < playerField.transform.childCount; ++i)
                {
                    FieldCard myCard = playerField.transform.GetChild(i).GetComponent<FieldCard>();

                    if (myCard == null) { return; }

                    //�ϴ� ���� �ʵ忡 ����ī�� ���̸Ӱ� �ִ��� üũ
                    if (myCard.isUpperMostCard && myCard.card.data is SpellCard spellCard && spellCard.isTamer && (spellCard.color1 == CardColor.Red || spellCard.color2 == CardColor.Red))
                    {
                        //��� ī�忡 CreatureCard�� �ƴ� ���̸ӳ� ����ī�� ������ �����Ƿ� ���ǿ� CreatureCard�ʼ�
                        isTamerExist = true;
                        break;
                    }
                }

                if (isTamerExist)
                {
                    //���̸Ӱ� �����ϸ� 3õ���� ������ 1���� �Ҹ�
                    for (int i = 0; i < enemyField.transform.childCount; ++i)
                    {
                        FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();

                        if (enemyCard == null) { return; }

                        if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && enemyCard.strength <= 3000)
                        {
                            caster.player.UICardsList.Add(enemyCard);
                            destroyExist = true;
                        }
                    }
                }

                if (destroyExist)
                {
                    caster.player.CmdSyncTargeting(caster.player, true);
                    caster.player.CmdSetActiveDestroyPanel(caster.player);
                }

                break;

            case "��ũ��":
                caster.player.UICardsList.Clear();

                GameObject enemyField2 = Player.gameManager.enemyField.content.gameObject;
                GameObject playerField2 = Player.gameManager.playerField.content.gameObject;

                bool isTamerExist2 = false;
                bool destroyExist2 = false;

                for (int i = 0; i < playerField2.transform.childCount; ++i)
                {
                    FieldCard myCard = playerField2.transform.GetChild(i).GetComponent<FieldCard>();

                    if (myCard == null) { return; }

                    //�ϴ� ���� �ʵ忡 ����ī�� ���̸Ӱ� �ִ��� üũ
                    if (myCard.isUpperMostCard && myCard.card.data is SpellCard spellCard && spellCard.isTamer && (spellCard.color1 == CardColor.Red || spellCard.color2 == CardColor.Red))
                    {
                        //��� ī�忡 CreatureCard�� �ƴ� ���̸ӳ� ����ī�� ������ �����Ƿ� ���ǿ� CreatureCard�ʼ�
                        isTamerExist2 = true;
                        break;
                    }
                }

                if (isTamerExist2)
                {
                    //���̸Ӱ� �����ϸ� 6õ���� ������ 1���� �Ҹ�
                    for (int i = 0; i < enemyField2.transform.childCount; ++i)
                    {
                        FieldCard enemyCard = enemyField2.transform.GetChild(i).GetComponent<FieldCard>();

                        if (enemyCard == null) { return; }

                        if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && enemyCard.strength <= 6000)
                        {
                            caster.player.UICardsList.Add(enemyCard);
                            destroyExist2 = true;
                        }
                    }
                }

                if (destroyExist2)
                {
                    caster.player.CmdSyncTargeting(caster.player, true);
                    caster.player.CmdSetActiveDestroyPanel(caster.player);
                }

                break;
            case "���׷��̸�":
                caster.CmdChangeSomeThing(buff, true);
                caster.CmdAddBuff(this.buff);
                break;
        }
    }

    public void BlockedCast(FieldCard caster)
    {
        //��� ���������� ��ȭ�� ĳ����
        switch (cardName)
        {
            case "�ǿ��":
                //if (caster.player.IsOurTurn())
                {
                    caster.CmdChangeSomeThing(evolutionBuff, true);
                    caster.CmdAddBuff(this.evolutionBuff);
                }
                break;

            case "����ٸ�":
                caster.player.CmdDrawDeck(1);
                break;
        }
    }

    public void DigimonCast(FieldCard caster)
    {
        if (caster.isUpperMostCard == false)
        { return; }

        switch (cardName)
        {
            case "���ɴе���":
                if (caster.player.IsOurTurn())
                {
                    caster.CmdChangeSomeThing(buff, true);
                    caster.CmdAddBuff(buff);
                }
                else
                {
                    if (caster.buffs.Count > 0)
                    {
                        {
                            //�� ���� �ƴѵ��� �׷��̸� ���� ã�� ����
                            caster.CmdChangeSomeThing(buff, false);
                            caster.CmdRemoveBuff(buff.cardname);
                        }
                    }
                }
                break;

            case "��Ż�׷��̸�(û)":
                if (caster.player.IsOurTurn())
                {
                    if (!caster.buffs.Any(bufff => bufff.cardname == buff.cardname))
                    {
                        caster.CmdChangeSomeThing(buff, true);
                        caster.CmdAddBuff(buff);
                    }
                }
                else
                {
                    if (caster.buffs.Count > 0)
                    {
                        //�� ���� �ƴѵ��� �׷��̸� ���� ã�� ����
                        caster.CmdChangeSomeThing(buff, false);
                        caster.CmdRemoveBuff(buff.cardname);
                    }
                }
                break;
        }
    }

    public void AttackEndDigimonCasts(FieldCard caster)
    {
        //�������� ��ġ����(..)�������� ���� ���� �������� ���� �Լ�
        //ex)��Ż�׷��̸�(û) -> ���ݽ� �޸� � ��������� �Ѿ�µ�
        //��ť��Ƽ �����Ͻ� ���� ��ť��Ƽ ���ݱ��� �ϼ��ؾ��ؼ� ���� ���� ����
        //�׷� ��츦 ���� ���� �Լ�

        //�ٵ� �갡 ���� ������ ���?��������
        switch (cardName)
        {
            case "��Ż�׷��̸�(û)":
                if (caster.player.isServer)
                {
                    if (caster.player.IsOurTurn() && caster.isMyTurnDigimonCastingActive == false)
                    {
                        MemoryChecker.Inst.CmdChangeMemorySameSync(MemoryChecker.Inst.memory - 5);
                        caster.isMyTurnDigimonCastingActive = true;
                    }
                }
                else
                {
                    if (caster.player.IsOurTurn() && caster.isMyTurnDigimonCastingActive == false)
                    {
                        MemoryChecker.Inst.CmdChangeMemorySameSync(MemoryChecker.Inst.memory + 5);
                        caster.isMyTurnDigimonCastingActive = true;
                    }
                }
                break;
        }
    }
}