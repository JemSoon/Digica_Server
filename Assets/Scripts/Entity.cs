using UnityEngine;
using System;
using Mirror;

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
        Player.gameManager.CmdSyncCaster(this); //모든 게임매니저에 입력
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
        RpcOffBuffPanel(player);
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
            //원래 공격대로 진행
            Player.gameManager.caster.combat.CmdBattle(Player.gameManager.caster, Player.gameManager.target);
            Player.gameManager.CmdSyncCaster(null);
            Player.gameManager.CmdSyncTarget(null);
        }
    }
    [ClientRpc]
    public void RpcOffBuffPanel(Player player)
    {
        //얘는 꺼지면 알아서 자동 전투 (세큐리티 오픈)한다..추가로 오픈 시키면 안됨
        if (player == Player.localPlayer && Player.gameManager.buffPanel.activeSelf)
        {
            Player.gameManager.buffPanel.SetActive(false);
            Player.gameManager.CmdSyncCaster(null);
            Player.gameManager.CmdSyncTarget(null);
        }
    }
}
