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
        if (entity.health <= 0) Destroy(entity.gameObject);//����� ü�±������ ���� ó���ϴµ� ���߿� ���ݷ��� ������ �ı��� �ٲ����
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
        #region �ֻ�� ī�� ��������
        while (attacker.GetComponent<FieldCard>().isUpperMostCard==false)
        {
            attacker = attacker.GetComponent<FieldCard>().upperCard;
        }

        while(target.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            target = target.GetComponent<FieldCard>().upperCard;
        }
        #endregion

        #region ��ȭ�� ȿ�� �˻�
        FieldCard card = attacker.GetComponent<FieldCard>();

        while (card.isUnderMostCard == false)
        {
            card = card.underCard;

            for (int i = 0; i < ((CreatureCard)card.card.data).evolutionType.Count; ++i)
            {
                if (card.card.data is CreatureCard creatureCard && creatureCard.evolutionType[i] == EvolutionType.ATTACK)
                {
                    //�ֻ�� ī�忡 �ϴ� ī����� ��ȭ�� ȿ�� ������ ���Ѵ�
                    //CreatureCard�� ������ ������ �־���� 
                    creatureCard.AttackCast(card, attacker.GetComponent<FieldCard>());
                }
            }
        }
        #endregion

        ((FieldCard)attacker).CmdRotation(((FieldCard)attacker), Quaternion.Euler(0, 0, -90));

        if (((FieldCard)target).card.data is SpellCard spellCard)
        {
            if (spellCard.if_Security_Go_Hand == false)
            {
                //����ī�尡 if_Security_Go_Hand�� true��� ������ ���°� �ƴ϶� ������ ���ư� ��
                target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card); 
            }

            target.IsDead = true;
            Destroy(target.gameObject);
        }

        else 
        {
            CreatureCard attackerCreatureCard = ((CreatureCard)((FieldCard)attacker).card.data);

            if (attacker.strength < target.strength)
            {
                if(attackerCreatureCard.hasJamming && ((FieldCard)target).isSecurity)
                {
                    //����Ŀ�� �ֻ�� ī�尡 ����̸� �Ҹ����� ���� return���Ѿ� ��
                    //�ٵ� Ÿ���� ��ť��Ƽ�ϱ� ��������
                    target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
                    Destroy(target.gameObject);
                    return;
                }


                // ������ ī�� ������ ��� ī�� ����
                while (attacker.GetComponent<FieldCard>().isUnderMostCard == false)
                {
                    attacker.IsDead = true;
                    //���� ����ī�� ���� ����Ʈ ������ ����
                    attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);

                    //Battle(attacker, target);
                    Debug.Log(attacker.GetComponent<FieldCard>().card.name + " ������ ī��");
                    Destroy(attacker.gameObject);

                    attacker = attacker.GetComponent<FieldCard>().underCard;
                    Debug.Log(attacker.GetComponent<FieldCard>().card.name + " ������ ī��");
                }

                // ������ �� �� ī�嵵 ����
                attacker.IsDead = true;
                attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);
                Destroy(attacker.gameObject);
            }

            else if (attacker.strength > target.strength)
            {
                while (target.GetComponent<FieldCard>().isUnderMostCard == false)
                {
                    target.IsDead = true;
                    //���� ����ī�� ���� ����Ʈ ������ ����
                    target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);

                    //Battle(attacker, target);
                    Debug.Log(target.GetComponent<FieldCard>().card.name + " ������ ī��");
                    Destroy(target.gameObject);

                    target = target.GetComponent<FieldCard>().underCard;
                    Debug.Log(target.GetComponent<FieldCard>().card.name + " ������ ī��");
                }

                target.IsDead = true;
                target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
                Destroy(target.gameObject);

                if(attackerCreatureCard.hasSpear && !((FieldCard)target).isSecurity)
                {
                    //��ť��Ƽ �߰� ����
                    attackerCreatureCard.Attack(attacker, ((FieldCard)target).player);
                }
            }

            else//�Ѵ� ���ݷ��� ������
            {
                while (attacker.GetComponent<FieldCard>().isUnderMostCard == false)
                {
                    attacker.IsDead = true;
                    //���� ����ī�� ���� ����Ʈ ������ ����
                    attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);

                    //Battle(attacker, target);
                    Debug.Log(attacker.GetComponent<FieldCard>().card.name + " ������ ī��");
                    Destroy(attacker.gameObject);

                    attacker = attacker.GetComponent<FieldCard>().underCard;
                    Debug.Log(attacker.GetComponent<FieldCard>().card.name + " ������ ī��");
                }
                attacker.IsDead = true;
                attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);
                Destroy(attacker.gameObject);

                while (target.GetComponent<FieldCard>().isUnderMostCard == false)
                {
                    target.IsDead = true;
                    //���� ����ī�� ���� ����Ʈ ������ ����
                    target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);

                    //Battle(attacker, target);
                    Debug.Log(target.GetComponent<FieldCard>().card.name + " ������ ī��");
                    Destroy(target.gameObject);

                    target = target.GetComponent<FieldCard>().underCard;
                    Debug.Log(target.GetComponent<FieldCard>().card.name + " ������ ī��");
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
}
