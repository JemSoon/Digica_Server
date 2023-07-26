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
        if (entity.health <= 0) Destroy(entity.gameObject);//����� ü�±������ ���� ó���ϴµ� ���߿� ���ݷ��� ������ �ı��� �ٲ����
    }

    [Command(requiresAuthority = false)]
    public void CmdIncreaseWaitTurn()
    {
        entity.waitTurn++;
    }

    [Command(requiresAuthority = false)]
    public void CmdBattle(Entity attacker, Entity target)
    {
         if (attacker.strength <target.strength) 
        {

            //���� ����ī�� ���� ����Ʈ ������ ����
            attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);

            //Battle(attacker, target);
            Destroy(attacker.gameObject);
        }
        else if(attacker.strength >target.strength) 
        {
            Destroy(target.gameObject);
        }
        else//�Ѵ� ���ݷ��� ������
        {
            Destroy(attacker.gameObject);
            Destroy(target.gameObject);
        }
    }
}
