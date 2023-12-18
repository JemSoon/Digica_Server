// Put all our items in the Resources folder. We use Resources.LoadAll down
// below to load our items into a cache so we can easily reference them.
using UnityEngine;
using System.Collections.Generic;

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
        if (target is Player user)
        {
            //���ݴ���� �÷��̾��� ��ť��Ƽ ī��[0]���� �� �װͰ� ����
            Debug.Log("��ť��Ƽ ī�� ����");
            if (user.deck.securityCard.Count > 0)
            {
                user.deck.CmdPlaySecurityCard(user.deck.securityCard[0], user, attacker);
            }
            else
            {
                //���� ���� attacker�� �¸�
                Debug.Log("���� ���� " + attacker.GetComponentInParent<FieldCard>().player.username + "�� �¸�!");
            }
        }

        else
        {
            GameObject enemyField = ((FieldCard)target).transform.parent.gameObject;
            bool foundBlocker = false; // �߰�: ��� Ÿ�� ������ ī�带 ã�Ҵ��� ����
            for (int i = 0; i < enemyField.transform.childCount; ++i)
            {
                FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();


                //if(target�� �÷��̾� �ʵ忡 �ֻ�� ī���� hasBlock�� ������ ��� Ÿ��)
                if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && ((CreatureCard)enemyCard.card.data).hasBlocker)
                {
                    foundBlocker = true;
                    Player.gameManager.CmdSyncTarget(target);
                    //���Ÿ��
                    Debug.Log("��� Ÿ�� ������ ī�� ����!");
                    enemyCard.CmdSyncTargeting(enemyCard.player, true);

                    enemyCard.player.CmdSetActiveBlockPanel(enemyCard.player, true);

                    break;
                }
            }

            if (!foundBlocker)
            {
                //���Ŀ�� ��� �ʵ忡 ���ٸ� �Ϲ� ���� ����
                attacker.combat.CmdBattle(attacker, target);
            }
        }

        attacker.DestroyTargetingArrow();

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

                buffTarget.CmdChangeSomeThing(evolutionBuff, true);
                buffTarget.CmdAddBuff(evolutionBuff);
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
                if (caster.player.IsOurTurn())
                {
                    target.CmdChangeSomeThing(evolutionBuff, true);
                    target.CmdAddBuff(evolutionBuff);
                }
                else
                {
                    if (target.buffs.Count > 0)
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
                if (caster.player.IsOurTurn())
                {
                    target.CmdChangeSomeThing(evolutionBuff, true);
                    target.CmdAddBuff(evolutionBuff);
                }
                else
                {
                    if (target.buffs.Count > 0)
                    {
                        {
                            //�� ���� �ƴѵ��� �׷��̸� ���� ã�� ����
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
                if (caster.player.IsOurTurn())
                {
                    if (((CreatureCard)target.card.data).hasSpear)
                    {
                        //FieldCard�� CreatureCard�� isSpear������ ���� bool�� �޾ƾ��� 
                        target.CmdChangeSomeThing(evolutionBuff, true);
                        target.CmdAddBuff(evolutionBuff);
                    }
                }
                else
                {
                    if (target.buffs.Count > 0 && ((CreatureCard)target.card.data).hasSpear)
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

                if (caster.player.IsOurTurn())
                {
                    if (caster.player.enemyInfo.data.deck.graveyard.Count >= 5)
                    {
                        target.CmdChangeSomeThing(evolutionBuff, true);
                        target.CmdAddBuff(evolutionBuff);
                    }
                }
                else
                {
                    if (target.buffs.Count > 0)
                    {
                        {
                            //�� ���� �ƴѵ��� �׷��̸� ���� ã�� ����
                            target.CmdChangeSomeThing(evolutionBuff, false);
                            target.CmdRemoveBuff(evolutionBuff.cardname);
                        }
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
        }
    }
}