using UnityEngine;
using Mirror;

public class Combat : NetworkBehaviour
{
    [Header("Entity")]
    public Entity entity;

    [Command]
    public void CmdChangeMana(int amount)
    {
        // Increase mana by amount. If 3, increase by 3. If -3, reduce by 3.
        if (entity is Player) 
        {
            //entity.GetComponent<Player>().mana += amount;
            if (entity.GetComponent<Player>().firstPlayer)
            { MemoryChecker.Inst.memory += amount; }
            else
            { MemoryChecker.Inst.memory -= amount;}
        }
    }

    public void ChangeMana(int amount)
    {
        // Increase mana by amount. If 3, increase by 3. If -3, reduce by 3.
        if (entity is Player)
        {
            entity.GetComponent<Player>().mana += amount;

        }
    }

    [Command]
    public void CmdChangeStrength(int amount)
    {
        // Increase mana by amount. If 3, increase by 3. If -3, reduce by 3.
        entity.strength += amount;
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeHealth(int amount)
    {
        // Increase health by amount. If 3, increase by 3. If -3, reduce by 3.
        entity.health += amount;
        if (entity.health <= 0) Destroy(entity.gameObject);//현재는 체력기반으로 죽음 처리하는데 나중엔 공격력이 낮은놈 파괴로 바꿔야함
    }

    [Command(requiresAuthority = false)]
    public void CmdIncreaseWaitTurn()
    {
        entity.waitTurn++;
    }

    [Command(requiresAuthority = false)]
    public void CmdBattle(Entity attacker, Entity target)
    {
        #region 최상단 카드 가져오기
        while (attacker.GetComponent<FieldCard>().isUpperMostCard==false)
        {
            attacker = attacker.GetComponent<FieldCard>().upperCard;
        }

        while(target.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            target = target.GetComponent<FieldCard>().upperCard;
        }
        #endregion

        if (attacker.strength <target.strength) 
        {
            //죽은 공격카드 무덤 리스트 정보에 저장
            attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);

            //Battle(attacker, target);
            Destroy(attacker.gameObject);
        }
        else if(attacker.strength >target.strength) 
        {
            target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
            Destroy(target.gameObject);
        }
        else//둘다 공격력이 같을때
        {
            attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);
            target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
            Destroy(attacker.gameObject);
            Destroy(target.gameObject);
        }
    }
}
