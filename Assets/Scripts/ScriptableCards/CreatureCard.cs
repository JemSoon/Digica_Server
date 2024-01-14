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
    public bool makeSecurityEffectNull = false;//워그레이몬같이 세큐리티 무효화시 확인하는

    [Header("Death Abilities")]
    public List<CardAbility> deathcrys = new List<CardAbility>();
    [HideInInspector] public bool hasDeathCry = false; // If our card has a DEATHCRY ability

    [Header("Board Prefab")]
    public FieldCard cardPrefab;

    [Header("Buff")]
    public Buffs buff;//디지몬 버프
    public Buffs evolutionBuff;//진화원 버프

    public virtual void Attack(Entity attacker, Entity target)
    {
        //어태커의 타게팅 애로우 먼저 삭제(나중에 삭제하는데 어태커가 죽으면 다시 애로우 생길때 타게팅이 안됨) 
        attacker.DestroyTargetingArrow();
        //먼저 블로커가 있는지 확인
        GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
        bool foundBlocker = false; // 추가: 블록 타입 디지몬 카드를 찾았는지 여부
        for (int i = 0; i < enemyField.transform.childCount; ++i)
        {
            FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();


            //if(target의 플레이어 필드에 최상단 카드중 hasBlock이 있고 시큐아니고 타겟!=블록카드 라면 블록 타임)
            if (enemyCard.isUpperMostCard && !enemyCard.isSecurity && enemyCard.card.data is CreatureCard && ((CreatureCard)enemyCard.card.data).hasBlocker && !enemyCard.attacked && target!=enemyCard)
            {
                foundBlocker = true;
                Player.gameManager.CmdSyncTarget(target);
                //블록타임
                Debug.Log("블록 타입 디지몬 카드 있음!");
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
                //공격대상이 플레이어라면 세큐리티 카드[0]스폰 및 그것과 전투
                Debug.Log("세큐리티 카드 오픈");
                if (user.deck.securityCard.Count > 0)
                {
                    //Debug.Log("디지몬 효과 돌기전 상대 시큐카드 개수 : "+user.deck.securityCard.Count);
                    AttackDigimonCast(attacker.GetComponent<FieldCard>(),null);//1월7일 테스트
                    //Debug.Log("디지몬 효과 돈 후(AttckDigimonCast지난 후) 상대 시큐카드 개수 : " + user.deck.securityCard.Count);
                    attacker.combat.CmdBattle(attacker, target);//이건 잘받아 오네..?
                }
                else
                {
                    //게임 종료 attacker의 승리
                    Player.gameManager.CmdEndGame(attacker);
                    Debug.Log("게임 종료 " + attacker.GetComponentInParent<FieldCard>().player.username + "의 승리!");
                }
            }
        }

        else
        {
            AttackDigimonCast(attacker.GetComponent<FieldCard>(), null);
            if (!foundBlocker || ((FieldCard)target).isSecurity)
            {
                //블로커가 상대 필드에 없다거나 시큐공격으로 나온 타겟카드면 이미 블록여부를 확인한것이기에 일반 공격 진행
                attacker.combat.CmdBattle(attacker, target);
            }
        }

        //attacker.DestroyTargetingArrow();//원래 위치였던것..

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
            target = target.GetComponent<FieldCard>().upperCard; //최상위 카드 가져오기
        }

        target.GetComponent<FieldCard>().CmdChangeSomeThing(buff, true);
        target.GetComponent<FieldCard>().CmdAddBuff(buff);

        caster.DestroyTargetingArrow();
    }

    public void AttackCast(FieldCard caster, FieldCard buffTarget)//진화원 캐스트
    {
        //버프 타겟 최상단 설정
        buffTarget = caster.upperCard;
        while (buffTarget != buffTarget.isUpperMostCard)
        {
            buffTarget = buffTarget.upperCard;
        }

        switch (cardName)
        {
            case "어니몬":

                if(caster.isMyTurnEvoCastingActive==false)
                {
                    buffTarget.CmdChangeSomeThing(evolutionBuff, true);
                    buffTarget.CmdAddBuff(evolutionBuff);
                    caster.isMyTurnEvoCastingActive = true;
                }
                //buffTarget.combat.CmdAfterBattle(caster, battleTarget);
                break;

            case "그라우몬":
                GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
                //caster.player.UICardsList = new List<FieldCard>();
                caster.player.UICardsList.Clear();
                Player player = caster.player;

                bool DestroytargetOn = false;//대상 있는지 없는지 확인용 지역변수

                for (int i = 0; i < enemyField.transform.childCount; ++i)
                {
                    FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();

                    if (enemyCard == null) { return; }

                    //if(target의 플레이어 필드에 최상단 카드중 최종 DP가 2000이하면 소멸)
                    if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && enemyCard.strength <= 2000)
                    {
                        //상대 카드에 CreatureCard가 아닌 테이머나 스펠카드 있을수 있으므로 조건에 CreatureCard필수
                        caster.player.UICardsList.Add(enemyCard);
                        Debug.Log("파괴 대상 디지몬 카드 있음!");
                        DestroytargetOn = true;
                    }
                }
                if (DestroytargetOn == false)
                {
                    Debug.Log("파괴 대상 디지몬 카드 없음");
                    break;
                }
                player.CmdSyncTargeting(player, true);
                player.CmdSetActiveDestroyPanel(player);
                break;
        }
    }

    public void AttackDigimonCast(FieldCard caster, FieldCard target)//디지몬 캐스트
    {
        if (caster.isUpperMostCard == false)
        { return; }

        switch (cardName)
        {
            case "메탈그레이몬":
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

            case "메탈그레이몬(청)":
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

            case "듀크몬":
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
            case "그레이몬":

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
                        //버프목록에 없으면 추가
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
                            //내 턴이 아닌동안 그레이몬 버프 찾아 제거
                            target.CmdChangeSomeThing(evolutionBuff, false);
                            target.CmdRemoveBuff(evolutionBuff.cardname);
                            
                        }
                    }
                }
                break;

            case "메탈그레이몬(청)":
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
                        //버프목록에 없으면 추가
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
                            Debug.Log("up카드 없으면 리턴시켰는데 왜 들어오니");
                            target.CmdChangeSomeThing(evolutionBuff, false);
                            target.CmdRemoveBuff(evolutionBuff.cardname);
                            
                        }
                    }
                }
                break;

            case "베이비드몬":
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
                        //FieldCard에 CreatureCard의 isSpear정보를 따로 bool로 받아야함 
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
                            //내 턴이 아닌동안 그레이몬 버프 찾아 제거
                            target.CmdChangeSomeThing(evolutionBuff, false);
                            target.CmdRemoveBuff(evolutionBuff.cardname); 
                        }
                    }
                }
                break;

            case "길몬":
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
                        //상대 트래시가 5장 이상일때 발동
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
                        //상대 트래시가 5장 미만이 되었다면 줬던 버프 제거
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

            case "메가로그라우몬":
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
                        //상대 트래시가 5장 이상일때 발동
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
                        //상대 트래시가 5장 미만이 되었다면 줬던 버프 제거
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
            case "화염리자몬":
                if(!caster.isUpperMostCard) { return; }//최상단 카드가 아니면 리턴

                if (caster.player.IsOurTurn())
                {
                    if (caster.player.firstPlayer)
                    {
                        if (MemoryChecker.Inst.memory >= 3 && !target.buffs.Any(buffs => buffs.cardname == buff.cardname) && caster.isMyTurnDigimonCastingActive == false)
                        {
                            //서버 호스트이고 메모리가 3이상이라면
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
                        //참가자 호스트이고 메모리가 -3보다 작다면 버프부여
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
            case "스컬그레이몬":
                GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
                //caster.player.UICardsList = new List<FieldCard>();
                caster.player.UICardsList.Clear();
                for (int i = 0; i < enemyField.transform.childCount; ++i)
                {
                    FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();

                    if (enemyCard == null) { return; }

                    //if(target의 플레이어 필드에 최상단 카드중 hasBlock이 있으면 블록 타임)
                    if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && ((CreatureCard)enemyCard.card.data).hasBlocker)
                    {
                        caster.player.UICardsList.Add(enemyCard);
                        //상대 카드에 CreatureCard가 아닌 테이머나 스펠카드 있을수 있으므로 조건에 CreatureCard필수
                        Debug.Log("파괴 대상 디지몬 카드 있음!");
                    }
                }
                caster.CmdSyncTargeting(caster.player, true);
                caster.player.CmdSetActiveDestroyPanel(caster.player);
                break;

            case "버드라몬":

                //버드라몬을 냈을때 상대턴으로 넘어가면 실행안함
                if(MemoryChecker.Inst.memory < 0 && caster.player.isServer)
                { return; }
                else if(MemoryChecker.Inst.memory > 0 && !caster.player.isServer)
                { return; }
                caster.SpawnTargetingArrow(caster.card, true);

                break;

            case "볼케닉드라몬":
                GameObject enemyField2 = Player.gameManager.enemyField.content.gameObject;

                for (int i = 0; i < enemyField2.transform.childCount; ++i)
                {
                    FieldCard enemyCard = enemyField2.transform.GetChild(i).GetComponent<FieldCard>();

                    if (enemyCard == null) { return; }

                    //일단 나의 필드에 레드카드 테이머가 있는지 체크
                    if (enemyCard.isUpperMostCard && enemyCard.strength<=4000)
                    {
                        while(!enemyCard.isUnderMostCard)
                        {
                            //진화원 차례대로 위에서부터 삭제
                            enemyCard.player.deck.CmdAddGraveyard(enemyCard.player, enemyCard.card);
                            enemyCard.CmdDestroyCard(enemyCard);
                            enemyCard = enemyCard.underCard;
                        }
                        //마지막 최하단 카드도 삭제
                        enemyCard.player.deck.CmdAddGraveyard(enemyCard.player, enemyCard.card);
                        enemyCard.CmdDestroyCard(enemyCard);
                    }
                }
                break;

            case "아구몬 박사":
                List<CardInfo> filteredGraveyard = caster.player.deck.graveyard.Where(card => card.data.name.Contains("아구몬")).ToList();
                caster.player.CmdClearUICardInfo();
                
                if( filteredGraveyard.Count > 0 )
                {
                    //무덤에 아구몬 카드가 1장 이상이라면
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

            case "아구몬":
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
            case "메가로그라우몬":
                caster.player.UICardsList.Clear();

                GameObject enemyField = Player.gameManager.enemyField.content.gameObject;
                GameObject playerField = Player.gameManager.playerField.content.gameObject;

                bool isTamerExist = false;
                bool destroyExist = false;

                for (int i = 0; i < playerField.transform.childCount; ++i)
                {
                    FieldCard myCard = playerField.transform.GetChild(i).GetComponent<FieldCard>();

                    if (myCard == null) { return; }

                    //일단 나의 필드에 레드카드 테이머가 있는지 체크
                    if (myCard.isUpperMostCard && myCard.card.data is SpellCard spellCard && spellCard.isTamer && (spellCard.color1 == CardColor.Red || spellCard.color2 == CardColor.Red))
                    {
                        //상대 카드에 CreatureCard가 아닌 테이머나 스펠카드 있을수 있으므로 조건에 CreatureCard필수
                        isTamerExist = true;
                        break;
                    }
                }

                if (isTamerExist)
                {
                    //테이머가 존재하면 3천이하 디지몬 1마리 소멸
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

            case "듀크몬":
                caster.player.UICardsList.Clear();

                GameObject enemyField2 = Player.gameManager.enemyField.content.gameObject;
                GameObject playerField2 = Player.gameManager.playerField.content.gameObject;

                bool isTamerExist2 = false;
                bool destroyExist2 = false;

                for (int i = 0; i < playerField2.transform.childCount; ++i)
                {
                    FieldCard myCard = playerField2.transform.GetChild(i).GetComponent<FieldCard>();

                    if (myCard == null) { return; }

                    //일단 나의 필드에 레드카드 테이머가 있는지 체크
                    if (myCard.isUpperMostCard && myCard.card.data is SpellCard spellCard && spellCard.isTamer && (spellCard.color1 == CardColor.Red || spellCard.color2 == CardColor.Red))
                    {
                        //상대 카드에 CreatureCard가 아닌 테이머나 스펠카드 있을수 있으므로 조건에 CreatureCard필수
                        isTamerExist2 = true;
                        break;
                    }
                }

                if (isTamerExist2)
                {
                    //테이머가 존재하면 6천이하 디지몬 1마리 소멸
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
            case "워그레이몬":
                caster.CmdChangeSomeThing(buff, true);
                caster.CmdAddBuff(this.buff);
                break;
        }
    }

    public void BlockedCast(FieldCard caster)
    {
        //블록 당했을때의 진화원 캐스팅
        switch (cardName)
        {
            case "피요몬":
                //if (caster.player.IsOurTurn())
                {
                    caster.CmdChangeSomeThing(evolutionBuff, true);
                    caster.CmdAddBuff(this.evolutionBuff);
                }
                break;

            case "가루다몬":
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
            case "볼케닉드라몬":
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
                            //내 턴이 아닌동안 그레이몬 버프 찾아 제거
                            caster.CmdChangeSomeThing(buff, false);
                            caster.CmdRemoveBuff(buff.cardname);
                        }
                    }
                }
                break;

            case "메탈그레이몬(청)":
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
                        //내 턴이 아닌동안 그레이몬 버프 찾아 제거
                        caster.CmdChangeSomeThing(buff, false);
                        caster.CmdRemoveBuff(buff.cardname);
                    }
                }
                break;
        }
    }

    public void AttackEndDigimonCasts(FieldCard caster)
    {
        //디지몬이 피치못한(..)사정으로 버프 제거 못했을때 쓰는 함수
        //ex)메탈그레이몬(청) -> 공격시 메모리 까서 상대턴으로 넘어갔는데
        //세큐리티 어택일시 여분 세큐리티 공격까지 완수해야해서 버프 제거 못함
        //그런 경우를 위해 만든 함수

        //근데 얘가 먼저 죽으면 어떡함?ㅁㄴㅇㄹ
        switch (cardName)
        {
            case "메탈그레이몬(청)":
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