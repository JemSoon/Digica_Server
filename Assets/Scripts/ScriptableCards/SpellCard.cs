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
                case "보링 스톰":
                    owner.CmdDrawDeck(1);
                    break;
                case "홀리 에스파다":
                    owner.CmdDrawDeck(owner.deck.securityCard.Count / 2);//세큐리티2장마다 드로우
                    break;
            }
        }

       if(type == SpellType.MEMORY)
        {
            switch (cardName)
            {
                case "스매시 포테이토":
                    owner.CmdAddBuff(buff);
                    owner.CmdChangeSomeThing(buff,true);
                    break;
            }
        }

    }

    public void AppearSecuritySpellCard(Player owner)
    {
        if (type == SpellType.MEMORY)
        {
            switch (cardName)
            {
                case "그래비티 프레스":
                    //세큐 효과 없음
                    break;
            }
        }

        if (type == SpellType.DRAW)
        {
            switch (cardName)
            {
                case "뉴클리어 레이저":
                    //세큐 효과 없음
                    break;
                case "보링 스톰":
                    owner.CmdDrawDeckNotMyTurn(2, owner);
                    break;
                case "홀리 에스파다":
                    owner.CmdDrawDeckNotMyTurn(owner.deck.securityCard.Count / 2, owner);//세큐리티2장마다 드로우
                    break;
            }
        }

        if (type == SpellType.DP)
        {
            switch (cardName)
            {
                case "브레이브 토네이도":
                    //손에 이 카드 추가
                    CardInfo cardInfo = new CardInfo();
                    cardInfo.cardID = CardID;
                    //cardInfo.data = ScriptableCard.Cache[cardInfo.cardID];
                    cardInfo.amount = 1;
                    owner.CmdDrawSpecificCard(cardInfo, owner);
                    break;

                case "헬 파이어":
                    //한장 카드 드로우
                    //★드로우 후 또 다른 후속 드로우 처리가있으면 ServerOnly사용★
                    owner.CmdDrawDeckServerOnly(1);

                    //손에 이 카드 추가
                    CardInfo cardInfo1 = new CardInfo();
                    cardInfo1.cardID = CardID;
                    //cardInfo.data = ScriptableCard.Cache[cardInfo.cardID];
                    cardInfo1.amount = 1;
                    owner.CmdDrawSpecificCard(cardInfo1, owner, 2);
                    break;
                case "호른 버스터":
                    CardInfo cardInfo2 = new CardInfo();
                    cardInfo2.cardID = CardID;
                    owner.CmdDrawSpecificCard(cardInfo2, owner);
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
                    { 
                        MemoryChecker.Inst.memory -= 2; 
                        
                        if(MemoryChecker.Inst.memory < -10)
                        {   
                            //메모리 -10넘어서 -11이하가 되면 -10으로 고정
                            MemoryChecker.Inst.memory = -10; 
                        }
                    }
                    else
                    { 
                        MemoryChecker.Inst.memory += 2; 

                        if (MemoryChecker.Inst.memory > 10)
                        {
                            //메모리 10넘어서 11이상이 되면 10으로 고정
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
            target = target.GetComponent<FieldCard>().upperCard; //최상위 카드 가져오기
        }

        if (caster.GetComponent<FieldCard>().isSecurity == false)
        {
            //일반 옵션카드 효과라면
            target.GetComponent<FieldCard>().CmdChangeSomeThing(buff, true);
            target.GetComponent<FieldCard>().CmdAddBuff(buff);
        }
        else
        {
            //시큐 옵션카드 효과라면
            target.GetComponent<FieldCard>().CmdChangeSomeThing(SecurityBuff, true);
            target.GetComponent<FieldCard>().CmdAddBuff(SecurityBuff);
        }

        caster.DestroyTargetingArrow();
    }
    public override void EndCast(Entity caster, Entity target)
    {
    }
}