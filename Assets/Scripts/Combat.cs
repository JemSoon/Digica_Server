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

    [ClientRpc]
    public void RpcBattleCast(Entity attacker, Entity target, Player player)
    {
        if(player == Player.localPlayer)
        {
            FieldCard card = attacker.GetComponent<FieldCard>();

            while (card.isUnderMostCard == false)
            {
                card = card.underCard;
                Debug.Log("진화원 도는중 " + card.card.data.cardName);
                for (int i = 0; i < ((CreatureCard)card.card.data).evolutionType.Count; ++i)
                {
                    if (card.card.data is CreatureCard creatureCard && creatureCard.evolutionType[i] == EvolutionType.ATTACK)
                    {
                        //최상단 카드에 하단 카드들의 진화원 효과 버프를 더한다
                        //CreatureCard도 버프를 가지고 있어야함 
                        creatureCard.AttackCast(card, attacker.GetComponent<FieldCard>());
                    }
                }
            }

            CmdAfterBattle(attacker, target);
        }
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

        #region 진화원 효과 검색
        //FieldCard card = attacker.GetComponent<FieldCard>();

        //while (card.isUnderMostCard == false)
        //{
        //    card = card.underCard;

        //    for (int i = 0; i < ((CreatureCard)card.card.data).evolutionType.Count; ++i)
        //    {
        //        if (card.card.data is CreatureCard creatureCard && creatureCard.evolutionType[i] == EvolutionType.ATTACK)
        //        {
        //            //최상단 카드에 하단 카드들의 진화원 효과 버프를 더한다
        //            //CreatureCard도 버프를 가지고 있어야함 
        //            creatureCard.AttackCast(card, attacker.GetComponent<FieldCard>());
        //        }
        //    }
        //}
        RpcBattleCast(attacker, target, ((FieldCard)attacker).player);//그라우몬..
        #endregion

        //((FieldCard)attacker).CmdRotation(((FieldCard)attacker), Quaternion.Euler(0, 0, -90));

        //if (((FieldCard)target).card.data is SpellCard spellCard)
        //{
        //    if (spellCard.if_Security_Go_Hand == false)
        //    {
        //        //스펠카드가 if_Security_Go_Hand가 true라면 무덤에 가는게 아니라 손으로 돌아간 것
        //        target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card); 
        //    }

        //    target.IsDead = true;
        //    Destroy(target.gameObject);
        //}

        //else 
        //{
        //    CreatureCard attackerCreatureCard = ((CreatureCard)((FieldCard)attacker).card.data);

        //    if (attacker.strength < target.strength)
        //    {
        //        if(attackerCreatureCard.hasJamming && ((FieldCard)target).isSecurity)
        //        {
        //            //어택커의 최상단 카드가 재밍이면 소멸하지 않음 return시켜야 함
        //            //근데 타겟은 시큐리티니까 무덤으로
        //            target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
        //            Destroy(target.gameObject);
        //            return;
        //        }


        //        // 마지막 카드 전까지 모든 카드 삭제
        //        while (attacker.GetComponent<FieldCard>().isUnderMostCard == false)
        //        {
        //            attacker.IsDead = true;
        //            //죽은 공격카드 무덤 리스트 정보에 저장
        //            attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);

        //            //Battle(attacker, target);
        //            Debug.Log(attacker.GetComponent<FieldCard>().card.name + " 삭제전 카드");
        //            Destroy(attacker.gameObject);

        //            attacker = attacker.GetComponent<FieldCard>().underCard;
        //            Debug.Log(attacker.GetComponent<FieldCard>().card.name + " 삭제후 카드");
        //        }

        //        // 마지막 맨 밑 카드도 삭제
        //        attacker.IsDead = true;
        //        attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);
        //        Destroy(attacker.gameObject);
        //    }

        //    else if (attacker.strength > target.strength)
        //    {
        //        while (target.GetComponent<FieldCard>().isUnderMostCard == false)
        //        {
        //            target.IsDead = true;
        //            //죽은 공격카드 무덤 리스트 정보에 저장
        //            target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);

        //            //Battle(attacker, target);
        //            Debug.Log(target.GetComponent<FieldCard>().card.name + " 삭제전 카드");
        //            Destroy(target.gameObject);

        //            target = target.GetComponent<FieldCard>().underCard;
        //            Debug.Log(target.GetComponent<FieldCard>().card.name + " 삭제후 카드");
        //        }

        //        target.IsDead = true;
        //        target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
        //        Destroy(target.gameObject);

        //        if(attackerCreatureCard.hasSpear && !((FieldCard)target).isSecurity)
        //        {
        //            //세큐리티 추가 공격
        //            attackerCreatureCard.Attack(attacker, ((FieldCard)target).player);
        //        }
        //    }

        //    else//둘다 공격력이 같을때
        //    {
        //        while (attacker.GetComponent<FieldCard>().isUnderMostCard == false)
        //        {
        //            attacker.IsDead = true;
        //            //죽은 공격카드 무덤 리스트 정보에 저장
        //            attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);

        //            //Battle(attacker, target);
        //            Debug.Log(attacker.GetComponent<FieldCard>().card.name + " 삭제전 카드");
        //            Destroy(attacker.gameObject);

        //            attacker = attacker.GetComponent<FieldCard>().underCard;
        //            Debug.Log(attacker.GetComponent<FieldCard>().card.name + " 삭제후 카드");
        //        }
        //        attacker.IsDead = true;
        //        attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);
        //        Destroy(attacker.gameObject);

        //        while (target.GetComponent<FieldCard>().isUnderMostCard == false)
        //        {
        //            target.IsDead = true;
        //            //죽은 공격카드 무덤 리스트 정보에 저장
        //            target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);

        //            //Battle(attacker, target);
        //            Debug.Log(target.GetComponent<FieldCard>().card.name + " 삭제전 카드");
        //            Destroy(target.gameObject);

        //            target = target.GetComponent<FieldCard>().underCard;
        //            Debug.Log(target.GetComponent<FieldCard>().card.name + " 삭제후 카드");
        //        }
        //        target.IsDead = true;
        //        target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
        //        Destroy(target.gameObject);
        //    }

        //    if (target.GetComponent<FieldCard>().isSecurity == true && target.IsDead == false)
        //    {
        //        target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
        //        Destroy(target.gameObject);
        //    }
        //}
    }

    [Command(requiresAuthority = false)]
    public void CmdAfterBattle(Entity attacker, Entity target)
    {
        #region 최상단 카드 가져오기
        while (attacker.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            attacker = attacker.GetComponent<FieldCard>().upperCard;
        }

        while (target.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            target = target.GetComponent<FieldCard>().upperCard;
        }
        #endregion

        ((FieldCard)attacker).CmdRotation(((FieldCard)attacker), Quaternion.Euler(0, 0, -90));

        if (((FieldCard)target).card.data is SpellCard spellCard)
        {
            if (spellCard.if_Security_Go_Hand == false)
            {
                //스펠카드가 if_Security_Go_Hand가 true라면 무덤에 가는게 아니라 손으로 돌아간 것
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
                if (attackerCreatureCard.hasJamming && ((FieldCard)target).isSecurity)
                {
                    //어택커의 최상단 카드가 재밍이면 소멸하지 않음 return시켜야 함
                    //근데 타겟은 시큐리티니까 무덤으로
                    target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
                    Destroy(target.gameObject);
                    return;
                }


                // 마지막 카드 전까지 모든 카드 삭제
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

                // 마지막 맨 밑 카드도 삭제
                attacker.IsDead = true;
                attacker.GetComponent<FieldCard>().player.deck.graveyard.Add(attacker.GetComponent<FieldCard>().card);
                Destroy(attacker.gameObject);
            }

            else if (attacker.strength > target.strength)
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

                if (attackerCreatureCard.hasSpear && !((FieldCard)target).isSecurity)
                {
                    //세큐리티 추가 공격
                    attackerCreatureCard.Attack(attacker, ((FieldCard)target).player);
                }
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
}

