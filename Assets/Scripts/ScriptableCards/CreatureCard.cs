// Put all our items in the Resources folder. We use Resources.LoadAll down
// below to load our items into a cache so we can easily reference them.
using UnityEngine;
using System.Collections.Generic;

public enum CreatureType : byte { BEAST, DRAGON, ALL }

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

    [Header("Specialities")]
    public bool hasCharge = false;
    public bool hasTaunt = false;

    [Header("Death Abilities")]
    public List<CardAbility> deathcrys = new List<CardAbility>();
    [HideInInspector] public bool hasDeathCry = false; // If our card has a DEATHCRY ability

    [Header("Board Prefab")]
    public FieldCard cardPrefab;

    public virtual void Attack(Entity attacker, Entity target)
    {
        if (target is Player user)
        {
            //공격대상이 플레이어라면 세큐리티 카드[0]스폰 및 그것과 전투
            Debug.Log("세큐리티 카드 오픈");
            if(user.deck.securityCard.Count > 0)
            {
                user.deck.CmdPlaySecurityCard(user.deck.securityCard[0], user, attacker);
            }
            else
            {
                //게임 종료 attacker의 승리
                Debug.Log("게임 종료 " + attacker.GetComponentInParent<FieldCard>().player.username + "의 승리!");
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
}