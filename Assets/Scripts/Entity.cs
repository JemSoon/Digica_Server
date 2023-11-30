using UnityEngine;
using System;
using Mirror;
using System.Collections;

[Serializable]
public abstract partial class Entity : NetworkBehaviour
{
    [Header("Combat")]
    public Combat combat;

    [Header("Stats")]
    [SyncVar] public int health = 0;
    [SyncVar] public int strength = 0;

    [Header("Targeting Arrow")]
    public Target casterType;
    public TargetingArrow arrow;
    public Transform spawnOffset;
    [SyncVar] public bool isTargeting = false;
    [HideInInspector] public GameObject arrowObject;

    public bool isTargetable = true; //// If a Player/Minion can be targeted.

    [Header("Special Properties")] //// These spawn properties are set by our ScriptableCards, when the card is spawned into the game.
    [SyncVar] public int waitTurn = 1; //// What turn does this card become active? Is it active as soon as it spawns, or do we wait 1, 2, 3, etc. turns before it can attack?
    public bool taunt = false; //// Whether it's a taunt minion or not.
    // waitTurn is also used for stunning/freezing/etc. minions.

    [Header("SpellEffect")]
    readonly public SyncList<Buffs> buffs = new SyncList<Buffs>(); // 효과 받은 수치를 저장해 두기

    public bool IsDead = false;
    public bool CanAttack() => Player.gameManager.isOurTurn && waitTurn == 0 && casterType == Target.FRIENDLIES;
    public bool CantAttack() => Player.gameManager.isOurTurn && waitTurn > 0 && casterType == Target.FRIENDLIES;

    public virtual void SpawnTargetingArrow(CardInfo card, bool IsAbility = false)
    {
        //Player.localPlayer.isTargeting = true;
        //isTargeting = true;
        Player player = Player.localPlayer;
        CmdSyncTargeting(player, true);
        //Debug.Log(player.username);

        Cursor.visible = false; //Hide cursor

        // If we have a spawnOffset, use it. Otherwise, use transform position.
        Vector3 spawnPos = spawnOffset == null ? transform.position : spawnOffset.position;
        arrowObject = Instantiate(arrow.gameObject, spawnPos, Quaternion.identity);
        arrowObject.GetComponent<TargetingArrow>().DrawLine(this, card, spawnPos, IsAbility);
    }

    public virtual void SpawnTargetingArrow(CardInfo card, Player player, bool IsAbility = false)
    {
        CmdSyncTargeting(player, true);

        Cursor.visible = false; //Hide cursor

        // If we have a spawnOffset, use it. Otherwise, use transform position.
        Vector3 spawnPos = spawnOffset == null ? transform.position : spawnOffset.position;
        arrowObject = Instantiate(arrow.gameObject, spawnPos, Quaternion.identity);
        arrowObject.GetComponent<TargetingArrow>().DrawLine(this, card, spawnPos, IsAbility);
    }

    public void DestroyTargetingArrow()
    {
        //Player.localPlayer.isTargeting = false;
        //isTargeting = false;
        Player player = Player.localPlayer;
        CmdSyncTargeting(player, false);

        Cursor.visible = true;
        Destroy(arrowObject);
    }

    public virtual void Update()
    {
        if (isTargeting && Input.GetMouseButton(1))
        {
            DestroyTargetingArrow();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSyncTargeting(Player player, bool Targeting)
    {
        player.isTargeting = Targeting;
        isTargeting = Targeting;

        CheckBuff(player);
        RpcOffBlockPanel(player);
    }

    [ClientRpc]
    public void CheckBuff(Player player)
    {
        if (this.GetComponent<FieldCard>() == null) { return; }

        //만약 버프 선택과 동시에 턴이 넘어갔다면, 턴넘어간 순간 버프가 끝난다면 그냥 선택지 파괴
        //테이머 버프는 무제한이라 -1주기땜에 0보다 작거나 같은경우의 조건은 안됨
        if (player.IsOurTurn() == false && player.isTargeting == true && ((FieldCard)this).card.data is SpellCard spellCard && spellCard.buff.buffTurn - 1 == 0)
        {
            DestroyTargetingArrow();
            Player.gameManager.caster = null;
        }
    }

    [ClientRpc]
    public void RpcOffBlockPanel(Player player)
    {
        if(player == Player.localPlayer && Player.gameManager.blockPanel.activeSelf)
        {
            Player.gameManager.blockPanel.SetActive(false);
        }
    }

    public IEnumerator DelayBattle(Entity attacker, Entity target)
    {
        yield return new WaitForSeconds(1.0f);//잠깐 쿨줘야함 안그럼 isTargeting인식 못함

        while(((FieldCard)target).player.isTargeting)
        {
            //Debug.Log(((FieldCard)target).player.isTargeting);
            yield return null;
        }
        attacker.combat.CmdBattle(attacker, target);
    }
}
