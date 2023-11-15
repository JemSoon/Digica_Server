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
            if(user.deck.securityCard.Count > 0)
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
            //attacker.GetComponentInParent<FieldCard>().CmdChangeAttacked(true);
            //attacker.GetComponent<FieldCard>().CmdRotation(attacker.GetComponent<FieldCard>(), Quaternion.Euler(0, 0, -90));
            attacker.combat.CmdBattle(attacker, target); 
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
        if(card1.color1 == card2.color1 ||
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

    }

    public void AttackCast(FieldCard caster, FieldCard target)
    {
        switch(cardName)
        {
            case "��ϸ�":
                target = caster.upperCard;
                while(target!=target.isUpperMostCard)
                {
                    target = target.upperCard;
                }
                target.CmdChangeSomeThing(evolutionBuff, true);
                target.CmdAddBuff(evolutionBuff);
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
}