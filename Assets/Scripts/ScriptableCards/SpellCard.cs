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
                case "신태일":
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

        if (type == SpellType.DESTROY)
        {
            switch (cardName)
            {
                case "오블리비언 버드":
                    GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
                    //owner.UICardsList = new List<FieldCard>();
                    owner.UICardsList.Clear();
                    bool blockExist = false;
                    for (int i = 0; i < enemyField.transform.childCount; ++i)
                    {
                        FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();

                        if (enemyCard == null) { return; }

                        //if(target의 플레이어 필드에 최상단 카드중 hasBlock이 있으면 블록 타임)
                        if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && ((CreatureCard)enemyCard.card.data).hasBlocker)
                        {
                            owner.UICardsList.Add(enemyCard);
                            
                            //상대 카드에 CreatureCard가 아닌 테이머나 스펠카드 있을수 있으므로 조건에 CreatureCard필수
                            Debug.Log("파괴 대상 디지몬 카드 있음!");
                            blockExist= true;
                        }
                    }
                    
                    if(blockExist)
                    {
                        owner.CmdSyncTargeting(owner, true);
                        owner.CmdSetActiveDestroyPanel(owner);
                    }

                    break;

                case "파이널 엘리시온":
                    GameObject enemyField2 = Player.gameManager.enemyField.content.gameObject;
                    GameObject playerField2 = Player.gameManager.playerField.content.gameObject;

                    owner.UICardsList.Clear();
                    bool isTamerExist = false;
                    bool blockExist2 = false;

                    for (int i = 0; i < playerField2.transform.childCount; ++i)
                    {
                        FieldCard myCard = playerField2.transform.GetChild(i).GetComponent<FieldCard>();

                        if (myCard == null) { return; }

                        //일단 나의 필드에 레드카드 테이머가 있는지 체크
                        if (myCard.isUpperMostCard && myCard.card.data is SpellCard spellCard && spellCard.isTamer && (spellCard.color1 == CardColor.Red || spellCard.color2 == CardColor.Red))
                        {
                            //상대 카드에 CreatureCard가 아닌 테이머나 스펠카드 있을수 있으므로 조건에 CreatureCard필수
                            Debug.Log("테이머 있음! 대상이 8000DP로 업!");
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
                case "그래비티 프레스":
                    //세큐 효과 없음
                    break;

                case "신태일":
                    card.CmdMakeSecurity(false);
                    //StartTamerCast(card.player);
                    if(owner == Player.localPlayer)
                    {
                        FindTamerTarget(Player.gameManager.playerField.content);
                    }
                    break;

                case "한소라":
                    card.CmdMakeSecurity(false);
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

        if (type == SpellType.DESTROY)
        {
            switch (cardName)
            {
                case "오블리비언 버드":
                    GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
                    //owner.UICardsList = new List<FieldCard>();
                    owner.UICardsList.Clear();
                    bool blockExist = false;
                    for (int i = 0; i < enemyField.transform.childCount; ++i)
                    {
                        FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();

                        if (enemyCard == null) { return; }

                        //if(target의 플레이어 필드에 최상단 카드중 hasBlock이 있으면 블록 타임)
                        if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && ((CreatureCard)enemyCard.card.data).hasBlocker)
                        {
                            owner.UICardsList.Add(enemyCard);

                            //상대 카드에 CreatureCard가 아닌 테이머나 스펠카드 있을수 있으므로 조건에 CreatureCard필수
                            Debug.Log("파괴 대상 디지몬 카드 있음!");
                            blockExist = true;
                        }
                    }

                    if (blockExist)
                    {
                        owner.CmdSyncTargeting(owner, true);
                        owner.CmdSetActiveDestroyPanel(owner);
                    }

                    break;

                case "파이널 엘리시온":
                    GameObject enemyField2 = Player.gameManager.enemyField.content.gameObject;
                    GameObject playerField2 = Player.gameManager.playerField.content.gameObject;

                    owner.UICardsList.Clear();
                    bool isTamerExist = false;
                    bool blockExist2 = false;

                    for (int i = 0; i < playerField2.transform.childCount; ++i)
                    {
                        FieldCard myCard = playerField2.transform.GetChild(i).GetComponent<FieldCard>();

                        if (myCard == null) { return; }

                        //일단 나의 필드에 레드카드 테이머가 있는지 체크
                        if (myCard.isUpperMostCard && myCard.card.data is SpellCard spellCard && spellCard.isTamer && (spellCard.color1 == CardColor.Red || spellCard.color2 == CardColor.Red))
                        {
                            //상대 카드에 CreatureCard가 아닌 테이머나 스펠카드 있을수 있으므로 조건에 CreatureCard필수
                            Debug.Log("테이머 있음! 대상이 8000DP로 업!");
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
            case "한소라":
                Debug.Log("한소라 있읍 얍얍");
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
        --caster.GetComponent<FieldCard>().buffTargetCount;

        if (caster.GetComponent<FieldCard>().buffTargetCount == 0)
        { caster.DestroyTargetingArrow(); }
    }
    public override void EndCast(Entity caster, Entity target)
    {
    }
    
    public void StartTamerCast(Player player)
    {
        if(cardName=="신태일")
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
                case "신태일":
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
                                    //버프는 턴이지남에따라 증가해서 buff==card.buff가 될수 없다(턴이 --되면서 다르게 되기 때문)
                                    //즉 이름으로 같은버프인지 찾아서 인덱스로 제거 
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
        //모든 필드카드가 아닌 특정 카드에 테이머 효과 부여될건지 체크하는 함수

        switch (cardName)
        {
            case "신태일":
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
                                //버프는 턴이지남에따라 증가해서 buff==card.buff가 될수 없다(턴이 --되면서 다르게 되기 때문)
                                //즉 이름으로 같은버프인지 찾아서 인덱스로 제거 
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