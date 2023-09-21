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

    [Header("Buff")]
    public Buffs buff;

    [Header("Board Prefab")]
    public FieldCard cardPrefab;

    [Header("Targets")]
    public List<Target> acceptableTargets = new List<Target>();

    public void AppearSpellCard(Player owner)
    {
        if(type == SpellType.MEMORY) 
        {
            switch (cardName)
            {
                case "그래비티 프레스":
                    if (owner.firstPlayer)
                    { MemoryChecker.Inst.memory += 2; }
                    else
                    {  MemoryChecker.Inst.memory -= 2;}
                    break;
            }
        }

        if(type == SpellType.DRAW)
        {
            switch (cardName)
            {
                case "뉴클리어 레이저":
                    owner.CmdDrawDeck(2);
                    break;
            }
        }
    }

    public void EndTurnEffect(Player owner)
    {
        if (type == SpellType.MEMORY)
        {
            switch (cardName)
            {
                case "그래비티 프레스":
                    if (owner.firstPlayer)
                    { MemoryChecker.Inst.memory -= 2; }
                    else
                    { MemoryChecker.Inst.memory += 2; }
                    break;
            }
        }
    }

    public override void StartCast(Entity caster, Entity target)
    {
        base.StartCast(caster, target);

        while (target.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            target = target.GetComponent<FieldCard>().upperCard; //최상위 카드 가져오기
        }

        target.GetComponent<FieldCard>().CmdChangeSomeThing(buff,true);
        target.GetComponent<FieldCard>().CmdAddBuff(buff);

        caster.DestroyTargetingArrow();
    }
    public override void EndCast(Entity caster, Entity target)
    {
    }
}