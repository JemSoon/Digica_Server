using UnityEngine;
using Mirror;
using static UnityEngine.GraphicsBuffer;
using UnityEditor.Experimental.GraphView;

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

    [ClientRpc]
    public void RpcBattleCast(Entity attacker, Entity target, Player player)
    {
        if(player == Player.localPlayer)
        {
            FieldCard card = attacker.GetComponent<FieldCard>();

            CreatureCard creatureCard1 = card.card.data as CreatureCard;

            creatureCard1.AttackDigimonCast(card, null);

            while (card.isUnderMostCard == false)
            {
                card = card.underCard;
                Debug.Log("��ȭ�� ������ " + card.card.data.cardName);
                for (int i = 0; i < ((CreatureCard)card.card.data).evolutionType.Count; ++i)
                {
                    if (card.card.data is CreatureCard creatureCard && creatureCard.evolutionType[i] == EvolutionType.ATTACK)
                    {
                        //�ֻ�� ī�忡 �ϴ� ī����� ��ȭ�� ȿ�� ������ ���Ѵ�
                        //CreatureCard�� ������ ������ �־���� 
                        creatureCard.AttackCast(card, attacker.GetComponent<FieldCard>());
                    }

                    if (attacker.GetComponent<FieldCard>().blocked)
                    {
                        Debug.Log("��� ��ȭ�� ���� �ߵ�!");
                        //�ֻ�� ī�尡 ��ϴ��ߴٸ� ���edĳ��Ʈ�� ����
                        ((CreatureCard)card.card.data).BlockedCast(attacker.GetComponent<FieldCard>());
                    }
                }
            }

            CmdAfterBattle(attacker, target);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdBattle(Entity attacker, Entity target)
    {
        #region ������ �ֻ�� ī�� ��������
        while (attacker.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            attacker = attacker.GetComponent<FieldCard>().upperCard;
        }
        #endregion

        if (target is Player)
        {
            if(((Player)target).deck.securityCard.Count>0)
            {
                ((Player)target).deck.CmdPlaySecurityCard(((Player)target).deck.securityCard[0], ((Player)target), attacker);
            }
        }
        else
        {
            #region Ÿ�� �ֻ�� ī�� ��������
            while (target.GetComponent<FieldCard>().isUpperMostCard == false)
            {
                target = target.GetComponent<FieldCard>().upperCard;
            }
            #endregion

            #region ����Ŀ�� ��ȭ�� ȿ�� �˻�
            RpcBattleCast(attacker, target, ((FieldCard)attacker).player);//�׶���..
            #endregion
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAfterBattle(Entity attacker, Entity target)
    {
        #region �ֻ�� ī�� ��������
        while (attacker.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            attacker = attacker.GetComponent<FieldCard>().upperCard;
        }
        //Debug.Log("���� Ÿ��ī���� �ֻ�� ī�� Ȯ����");
        while (target.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            target = target.GetComponent<FieldCard>().upperCard;
        }
        #endregion

        ((FieldCard)attacker).CmdRotation(((FieldCard)attacker), Quaternion.Euler(0, 0, -90));

        if (((FieldCard)target).card.data is SpellCard spellCard)
        {
            if (spellCard.isTamer && ((FieldCard)target).isSecurity==false)
            {
                //���̸�+��ť�� �ƴ� ī��� �Ҹ� �ȵǰԲ�
                return;
            }

            if (spellCard.if_Security_Go_Hand == false)
            {
                //����ī�尡 if_Security_Go_Hand�� true��� ������ ���°� �ƴ϶� ������ ���ư� ��
                target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
            }
            target.IsDead = true;
            Destroy(target.gameObject);

            //�̰� Ȥ�� ���������ϴϱ� �ڲ� ������Ŭ�� ���Ŀ�� ���ǰɷ� �ߴ°ǰ�?RPC�� �Űܺ����ҵ�
            if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack > 0 && target.GetComponent<FieldCard>().isSecurity)
            {
                //�����ڰ� ��ť��Ƽ ������ �ؼ� ��Ƴ��Ұ� �߰� ��ť��Ƽ üũ�� �ִٸ� �� ��ť��Ƽ ����
                attacker.GetComponent<FieldCard>().tempBuff.securityAttack -= 1;
                if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack == 0)
                {
                    attacker.GetComponent<FieldCard>().SecurityCheckText.gameObject.SetActive(false);
                }
                //((CreatureCard)attacker.GetComponent<FieldCard>().card.data).Attack(attacker, ((FieldCard)target).player);
                RpcAfterBattle(attacker, ((FieldCard)target).player);
            }
        }

        else
        {
            CreatureCard attackerCreatureCard = ((CreatureCard)((FieldCard)attacker).card.data);

            if (attacker.strength < target.strength)
            {
                if (attackerCreatureCard.hasJamming && ((FieldCard)target).isSecurity)
                {
                    //����Ŀ�� �ֻ�� ī�尡 ����̸� �Ҹ����� ���� return���Ѿ� ��
                    //�ٵ� Ÿ���� ��ť��Ƽ�ϱ� ��������
                    target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
                    Destroy(target.gameObject);

                    //��� ����Ŀ �߰� ��ťüũ ������ ����
                    if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack > 0 && target.GetComponent<FieldCard>().isSecurity)
                    {
                        //�����ڰ� ��ť��Ƽ ������ �ؼ� ��Ƴ��Ұ� �߰� ��ť��Ƽ üũ�� �ִٸ� �� ��ť��Ƽ ����
                        attacker.GetComponent<FieldCard>().tempBuff.securityAttack -= 1;
                        if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack == 0)
                        {
                            attacker.GetComponent<FieldCard>().SecurityCheckText.gameObject.SetActive(false);
                        }
                        //((CreatureCard)attacker.GetComponent<FieldCard>().card.data).Attack(attacker, ((FieldCard)target).player);
                        RpcAfterBattle(attacker, ((FieldCard)target).player);
                    }

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

                if (attackerCreatureCard.hasSpear && !((FieldCard)target).isSecurity)
                {
                    //��ť��Ƽ �߰� ����
                    //attackerCreatureCard.Attack(attacker, ((FieldCard)target).player);
                    RpcAfterBattle(attacker, ((FieldCard)target).player);
                }

                if(attacker.GetComponent<FieldCard>().tempBuff.securityAttack > 0 && target.GetComponent<FieldCard>().isSecurity)
                {
                    //�����ڰ� ��ť��Ƽ ������ �ؼ� ��Ƴ��Ұ� �߰� ��ť��Ƽ üũ�� �ִٸ� �� ��ť��Ƽ ����
                    attacker.GetComponent<FieldCard>().tempBuff.securityAttack -= 1;
                    if(attacker.GetComponent<FieldCard>().tempBuff.securityAttack==0)
                    {
                        attacker.GetComponent<FieldCard>().SecurityCheckText.gameObject.SetActive(false);
                    }
                    //attackerCreatureCard.Attack(attacker, ((FieldCard)target).player);
                    RpcAfterBattle(attacker, ((FieldCard)target).player);
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

    [ClientRpc]
    public void RpcAfterBattle(Entity attacker, Entity target)
    {
        //��ť��Ƽ�� ���� �߰� Attack�� Rpc��
        if(attacker.GetComponent<FieldCard>().player==Player.localPlayer)
        {
            CreatureCard creature = (CreatureCard)attacker.GetComponent<FieldCard>().card.data;
            creature.Attack(attacker, target);
        }
    }
}

