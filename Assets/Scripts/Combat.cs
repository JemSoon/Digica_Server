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
            { 
                MemoryChecker.Inst.memory += amount; 
                if(MemoryChecker.Inst.memory < 0)
                { Player.gameManager.CmdEndTurn(); }
            }
            else
            {
                MemoryChecker.Inst.memory -= amount;
                if(MemoryChecker.Inst.memory > 0)
                { Player.gameManager.CmdEndTurn();}
            }
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
    public void CmdReduceSecurityAttack()
    {
        ((FieldCard)entity).securityAttack--;
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
            // 마지막 카드 전까지 모든 카드 삭제
            while(attacker.GetComponent<FieldCard>().isUnderMostCard == false)
            {
                attacker.IsDead = true;
                //죽은 공격카드 무덤 리스트 정보에 저장
                attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);

                //Battle(attacker, target);
                Debug.Log(attacker.GetComponent<FieldCard>().card.name + " 삭제전 카드");
                Destroy(attacker.gameObject);

                attacker = attacker.GetComponent<FieldCard>().underCard;
                Debug.Log(attacker.GetComponent<FieldCard>().card.name + " 삭제후 카드");
            }

            // 마지막 맨 밑 카드도 삭제
            attacker.IsDead = true;
            attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);
            Destroy(attacker.gameObject);
        }

        else if (attacker.strength >target.strength) 
        {
            while (target.GetComponent<FieldCard>().isUnderMostCard == false)
            {
                target.IsDead = true;
                //죽은 공격카드 무덤 리스트 정보에 저장
                target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);

                //Battle(attacker, target);
                Debug.Log(target.GetComponent<FieldCard>().card.name + " 삭제전 카드");
                Destroy(target.gameObject);

                target = target.GetComponent<FieldCard>().underCard;
                Debug.Log(target.GetComponent<FieldCard>().card.name + " 삭제후 카드");
            }

            target.IsDead = true;
            target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
            Destroy(target.gameObject);
        }

        else//둘다 공격력이 같을때
        {
            while (attacker.GetComponent<FieldCard>().isUnderMostCard == false)
            {
                attacker.IsDead = true;
                //죽은 공격카드 무덤 리스트 정보에 저장
                attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);

                //Battle(attacker, target);
                Debug.Log(attacker.GetComponent<FieldCard>().card.name + " 삭제전 카드");
                Destroy(attacker.gameObject);

                attacker = attacker.GetComponent<FieldCard>().underCard;
                Debug.Log(attacker.GetComponent<FieldCard>().card.name + " 삭제후 카드");
            }
            attacker.IsDead = true;
            attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);
            Destroy(attacker.gameObject);

            while (target.GetComponent<FieldCard>().isUnderMostCard == false)
            {
                target.IsDead = true;
                //죽은 공격카드 무덤 리스트 정보에 저장
                target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);

                //Battle(attacker, target);
                Debug.Log(target.GetComponent<FieldCard>().card.name + " 삭제전 카드");
                Destroy(target.gameObject);

                target = target.GetComponent<FieldCard>().underCard;
                Debug.Log(target.GetComponent<FieldCard>().card.name + " 삭제후 카드");
            }
            target.IsDead = true;
            target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
            Destroy(target.gameObject);
        }

        if (target.GetComponent<FieldCard>().isSecurity == true && target.IsDead == false)
        {
            target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
            Destroy(target.gameObject);
        }
    }
}
