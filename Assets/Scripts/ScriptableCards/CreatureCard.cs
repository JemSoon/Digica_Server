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
    public Buffs buff;//디지몬 버프
    public Buffs evolutionBuff;//진화원 버프

    public virtual void Attack(Entity attacker, Entity target)
    {
        if (target is Player user)
        {
            //공격대상이 플레이어라면 세큐리티 카드[0]스폰 및 그것과 전투
            Debug.Log("세큐리티 카드 오픈");
            if (user.deck.securityCard.Count > 0)
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
            GameObject enemyField = ((FieldCard)target).transform.parent.gameObject;
            bool foundBlocker = false; // 추가: 블록 타입 디지몬 카드를 찾았는지 여부
            for (int i = 0; i < enemyField.transform.childCount; ++i)
            {
                FieldCard enemyCard = enemyField.transform.GetChild(i).GetComponent<FieldCard>();


                //if(target의 플레이어 필드에 최상단 카드중 hasBlock이 있으면 블록 타임)
                if (enemyCard.isUpperMostCard && enemyCard.card.data is CreatureCard && ((CreatureCard)enemyCard.card.data).hasBlocker)
                {
                    foundBlocker = true;
                    Player.gameManager.CmdSyncTarget(target);
                    //블록타임
                    Debug.Log("블록 타입 디지몬 카드 있음!");
                    enemyCard.CmdSyncTargeting(enemyCard.player, true);

                    enemyCard.player.CmdSetActiveBlockPanel(enemyCard.player, true);

                    break;
                }
            }

            if (!foundBlocker)
            {
                //블로커가 상대 필드에 없다면 일반 공격 진행
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

                buffTarget.CmdChangeSomeThing(evolutionBuff, true);
                buffTarget.CmdAddBuff(evolutionBuff);
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
                            //내 턴이 아닌동안 그레이몬 버프 찾아 제거
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
                if (caster.player.IsOurTurn())
                {
                    if (((CreatureCard)target.card.data).hasSpear)
                    {
                        //FieldCard에 CreatureCard의 isSpear정보를 따로 bool로 받아야함 
                        target.CmdChangeSomeThing(evolutionBuff, true);
                        target.CmdAddBuff(evolutionBuff);
                    }
                }
                else
                {
                    if (target.buffs.Count > 0 && ((CreatureCard)target.card.data).hasSpear)
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
                            //내 턴이 아닌동안 그레이몬 버프 찾아 제거
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
        }
    }
}