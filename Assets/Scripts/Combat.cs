using UnityEngine;
using Mirror;
using System.Collections;

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

            CreatureCard creatureCard1 = card.card.data as CreatureCard;

            //creatureCard1.AttackDigimonCast(card, null);

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

                    if (attacker.GetComponent<FieldCard>().blocked)
                    {
                        Debug.Log("블록 진화원 버프 발동!");
                        //최상단 카드가 블록당했다면 블록ed캐스트도 실행
                        ((CreatureCard)card.card.data).BlockedCast(attacker.GetComponent<FieldCard>());
                    }
                }
            }

            //if(creatureCard1.cardName!="듀크몬")
            {
                CmdAfterBattle(attacker, target);
            }
            
            //StartCoroutine(CmdDelayAfterBattle(attacker, target, 0.1f));
        }
    }

    [ClientRpc]
    public void RpcTamerCardAttackCast(Entity attacker, Player player, Entity target)
    {
        if(player == Player.localPlayer)
        {
            PlayerField playerField = Player.gameManager.playerField;

            for(int i =0; i<playerField.content.childCount; ++i)
            {
                FieldCard TamerCard = playerField.content.transform.GetChild(i).GetComponent<FieldCard>();

                if(TamerCard.card.data is SpellCard spellCard && spellCard != null && spellCard.isTamer && !TamerCard.attacked)
                {
                    //테이머카드이고 테이머카드가 레스트 상태가 아니라면
                    //테이머 버프 패널 발동
                    //Player.gameManager.CmdSyncCaster(attacker);
                    //Player.gameManager.CmdSyncTarget(target); //타겟을 저장해 놔야함
                    //attacker.CmdSyncTargeting(player, true);
                    spellCard.AttackPlayerSpellCardCast(attacker, target, player);
                    Debug.Log("테이머 버프 부여 패널을 킵니다");
                }
            }
            StartCoroutine(DelayBattle(attacker, target));
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdBattle(Entity attacker, Entity target)
    {
        #region 공격자 최상단 카드 가져오기
        while (attacker.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            attacker = attacker.GetComponent<FieldCard>().upperCard;
        }
        #endregion

        if (target is Player)
        {
            //상대 플레이어 공격시 발동되는 테이머카드 발동
            RpcTamerCardAttackCast(attacker, attacker.GetComponent<FieldCard>().player, target);

            //★이거 주석하면 안됨!! CmdPlaySecurityCard를 여기서 스폰시켜야 참가자클라가 안꼬임!★
            //if (((Player)target).deck.securityCard.Count > 0)
            //{
            //    ((Player)target).deck.CmdPlaySecurityCard(((Player)target).deck.securityCard[0], ((Player)target), attacker);
            //}
        }
        else
        {
            #region 타겟 최상단 카드 가져오기
            while (target.GetComponent<FieldCard>().isUpperMostCard == false)
            {
                target = target.GetComponent<FieldCard>().upperCard;
            }
            #endregion
            //Debug.Log("실제 전투 상대 : "+target.GetComponent<FieldCard>().card.data.cardName);
            #region 어태커의 진화원 효과 검색
            RpcBattleCast(attacker, target, ((FieldCard)attacker).player);//그라우몬..
            #endregion
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAfterBattle(Entity attacker, Entity target)
    {
        #region 최상단 카드 가져오기
        while (attacker.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            attacker = attacker.GetComponent<FieldCard>().upperCard;
        }
        //Debug.Log("지금 타겟카드의 최상단 카드 확인중");
        while (target.GetComponent<FieldCard>().isUpperMostCard == false)
        {
            target = target.GetComponent<FieldCard>().upperCard;
        }
        #endregion

        FieldCard fieldCard = attacker.GetComponent<FieldCard>();
        CreatureCard creature = ((CreatureCard)attacker.GetComponent<FieldCard>().card.data);
        Player owner = attacker.GetComponent<FieldCard>().player;

        ((FieldCard)attacker).CmdRotation(((FieldCard)attacker), Quaternion.Euler(0, 0, -90));

        if (((FieldCard)target).card.data is SpellCard spellCard)
        {
            if (spellCard.isTamer && ((FieldCard)target).isSecurity==false)
            {
                //테이머+세큐가 아닌 카드는 소멸 안되게끔(이미 시큐로 나와도 isTamer를 false처리 먼저 하고 들어옴)

                if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack > 0 && attacker.GetComponent<FieldCard>().isSecurityAttack)
                {
                    //공격자가 세큐리티 어택을 해서 살아남았고 추가 세큐리티 체크가 있다면 또 세큐리티 어택
                    attacker.GetComponent<FieldCard>().tempBuff.securityAttack -= 1;

                    if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack == 0)
                    {
                        //시큐리티 어택이 0이 되었다면
                        attacker.GetComponent<FieldCard>().SecurityCheckText.gameObject.SetActive(false);

                        MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
                        MemoryChecker.Inst.CmdChangeInstantMemory(0);
                        if (MemoryChecker.Inst.memory < 0 && owner.firstPlayer)
                        {
                            Player.gameManager.CmdEndTurn();
                        }
                        else if (MemoryChecker.Inst.memory > 0 && !owner.firstPlayer)
                        {
                            Player.gameManager.CmdEndTurn();
                        }
                    }
                    else if (attacker.GetComponent<FieldCard>().IsDead || ((FieldCard)target).player.deck.securityCard.Count==0)
                    {
                        //시큐리티 스펠효과로 만약 죽거나 상대 세큐리티가 더는 없다면
                        MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
                        MemoryChecker.Inst.CmdChangeInstantMemory(0);
                        if (MemoryChecker.Inst.memory < 0 && owner.firstPlayer)
                        {
                            Player.gameManager.CmdEndTurn();
                        }
                        else if (MemoryChecker.Inst.memory > 0 && !owner.firstPlayer)
                        {
                            Player.gameManager.CmdEndTurn();
                        }
                    }
                    //((CreatureCard)attacker.GetComponent<FieldCard>().card.data).Attack(attacker, ((FieldCard)target).player);
                    RpcAfterBattle(attacker, ((FieldCard)target).player);
                }
                return;
            }

            if (spellCard.if_Security_Go_Hand == false)
            {
                //스펠카드가 if_Security_Go_Hand가 true라면 무덤에 가는게 아니라 손으로 돌아간 것
                target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
            }
            target.IsDead = true;
            Destroy(target.gameObject);

            if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack > 0 && attacker.GetComponent<FieldCard>().isSecurityAttack)
            {
                //공격자가 세큐리티 어택을 해서 살아남았고 추가 세큐리티 체크가 있다면 또 세큐리티 어택
                attacker.GetComponent<FieldCard>().tempBuff.securityAttack -= 1;

                if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack == 0)
                {
                    //시큐리티 어택이 0이 되었다면
                    attacker.GetComponent<FieldCard>().SecurityCheckText.gameObject.SetActive(false);
                    
                    MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
                    MemoryChecker.Inst.CmdChangeInstantMemory(0);
                    if (MemoryChecker.Inst.memory < 0 && owner.firstPlayer)
                    {
                        Player.gameManager.CmdEndTurn();
                    }
                    else if (MemoryChecker.Inst.memory > 0 && !owner.firstPlayer)
                    {
                        Player.gameManager.CmdEndTurn();
                    }
                }
                else if (attacker.GetComponent<FieldCard>().IsDead || ((FieldCard)target).player.deck.securityCard.Count == 0)
                {
                    //시큐리티 스펠효과로 만약 죽는다면
                    MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
                    MemoryChecker.Inst.CmdChangeInstantMemory(0);
                    if (MemoryChecker.Inst.memory < 0 && owner.firstPlayer)
                    {
                        Player.gameManager.CmdEndTurn();
                    }
                    else if (MemoryChecker.Inst.memory > 0 && !owner.firstPlayer)
                    {
                        Player.gameManager.CmdEndTurn();
                    }
                }
                //((CreatureCard)attacker.GetComponent<FieldCard>().card.data).Attack(attacker, ((FieldCard)target).player);
                RpcAfterBattle(attacker, ((FieldCard)target).player);
            }
        }

        else
        {
            CreatureCard attackerCreatureCard = ((CreatureCard)((FieldCard)attacker).card.data);
            int EnemySecurityCount = ((FieldCard)target).player.deck.securityCard.Count;//target이 죽으면 정보를 못받아서 미리받기

            if (attacker.strength < target.strength)
            {
                if (attackerCreatureCard.hasJamming && ((FieldCard)target).isSecurity)
                {
                    //어택커의 최상단 카드가 재밍이면 소멸하지 않음 return시켜야 함
                    //근데 타겟은 시큐리티니까 무덤으로
                    target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
                    Destroy(target.gameObject);

                    //재밍 어태커 추가 세큐체크 있으면 실행
                    if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack > 0 && attacker.GetComponent<FieldCard>().isSecurityAttack)
                    {
                        //공격자가 세큐리티 어택을 해서 살아남았고 추가 세큐리티 체크가 있다면 또 세큐리티 어택
                        attacker.GetComponent<FieldCard>().tempBuff.securityAttack -= 1;
                        if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack == 0 || ((FieldCard)target).player.deck.securityCard.Count==0)
                        {
                            //세큐리티 다 썻거나 상대 세큐리티 남은게 0개면 메탈(청) 공격시 메모리 뱉기 효과 발동
                            MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
                            MemoryChecker.Inst.CmdChangeInstantMemory(0);
                            if (MemoryChecker.Inst.memory < 0 && owner.firstPlayer)
                            {
                                Player.gameManager.CmdEndTurn();
                            }
                            else if (MemoryChecker.Inst.memory > 0 && !owner.firstPlayer)
                            {
                                Player.gameManager.CmdEndTurn();
                            }
                            attacker.GetComponent<FieldCard>().SecurityCheckText.gameObject.SetActive(false);
                        }

                        //((CreatureCard)attacker.GetComponent<FieldCard>().card.data).Attack(attacker, ((FieldCard)target).player);
                        RpcAfterBattle(attacker, ((FieldCard)target).player);
                    }

                    return;
                }

                //바로 죽으니 공격직후 뺄 메모리 제거
                MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
                MemoryChecker.Inst.CmdChangeInstantMemory(0);
                if (MemoryChecker.Inst.memory < 0 && owner.firstPlayer)
                {
                    Player.gameManager.CmdEndTurn();
                }
                else if (MemoryChecker.Inst.memory > 0 && !owner.firstPlayer)
                {
                    Player.gameManager.CmdEndTurn();
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
                    RpcAfterBattle(attacker, ((FieldCard)target).player);
                }
                if(attacker.GetComponent<FieldCard>().isSecurityAttack==false)
                {
                    //시큐리티 어택이 아닌 일반 디지몬 공격이였으니 바로 공격직후 메모리 제거
                    MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
                    MemoryChecker.Inst.CmdChangeInstantMemory(0);
                    if (MemoryChecker.Inst.memory < 0 && owner.firstPlayer)
                    {
                        Player.gameManager.CmdEndTurn();
                    }
                    else if (MemoryChecker.Inst.memory > 0 && !owner.firstPlayer)
                    {
                        Player.gameManager.CmdEndTurn();
                    }
                }

                if(attacker.GetComponent<FieldCard>().tempBuff.securityAttack > 0 && attacker.GetComponent<FieldCard>().isSecurityAttack)
                {
                    //공격자가 세큐리티 어택을 해서 살아남았고 추가 세큐리티 체크가 있다면 또 세큐리티 어택
                    attacker.GetComponent<FieldCard>().tempBuff.securityAttack -= 1;

                    if(attacker.GetComponent<FieldCard>().tempBuff.securityAttack==0 || EnemySecurityCount == 0)
                    {
                        attacker.GetComponent<FieldCard>().SecurityCheckText.gameObject.SetActive(false);
                        
                        // 메탈그레이몬(청)같이 피치못한 공격 직후 캐스팅 구현
                        MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
                        MemoryChecker.Inst.CmdChangeInstantMemory(0);
                        if (MemoryChecker.Inst.memory < 0 && owner.firstPlayer)
                        {
                            Player.gameManager.CmdEndTurn();
                        }
                        else if (MemoryChecker.Inst.memory > 0 && !owner.firstPlayer)
                        {
                            Player.gameManager.CmdEndTurn();
                        }
                    }
                    //attackerCreatureCard.Attack(attacker, ((FieldCard)target).player);
                    RpcAfterBattle(attacker, ((FieldCard)target).player);
                }
            }

            else//둘다 공격력이 같을때
            {
                if (attackerCreatureCard.hasJamming && ((FieldCard)target).isSecurity)
                {
                    //어택커의 최상단 카드가 재밍이면 소멸하지 않음 return시켜야 함
                    //근데 타겟은 시큐리티니까 무덤으로
                    target.GetComponent<FieldCard>().player.deck.graveyard.Add(target.GetComponent<FieldCard>().card);
                    Destroy(target.gameObject);

                    //재밍 어태커 추가 세큐체크 있으면 실행
                    if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack > 0 && target.GetComponent<FieldCard>().isSecurity)
                    {
                        //공격자가 세큐리티 어택을 해서 살아남았고 추가 세큐리티 체크가 있다면 또 세큐리티 어택
                        attacker.GetComponent<FieldCard>().tempBuff.securityAttack -= 1;
                        if (attacker.GetComponent<FieldCard>().tempBuff.securityAttack == 0 || EnemySecurityCount == 0)
                        {
                            //시큐어택 끝났으니까 공격 직후 뱉어야 할 메모리 뱉기
                            MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
                            MemoryChecker.Inst.CmdChangeInstantMemory(0);
                            if (MemoryChecker.Inst.memory < 0 && owner.firstPlayer)
                            {
                                Player.gameManager.CmdEndTurn();
                            }
                            else if (MemoryChecker.Inst.memory > 0 && !owner.firstPlayer)
                            {
                                Player.gameManager.CmdEndTurn();
                            }

                            attacker.GetComponent<FieldCard>().SecurityCheckText.gameObject.SetActive(false);
                        }
                        //((CreatureCard)attacker.GetComponent<FieldCard>().card.data).Attack(attacker, ((FieldCard)target).player);
                        RpcAfterBattle(attacker, ((FieldCard)target).player);
                    }

                    return;
                }

                //죽으니까 메모리 땡긴것 빼주고 초기화하고 턴 확인
                MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
                MemoryChecker.Inst.CmdChangeInstantMemory(0);
                if (MemoryChecker.Inst.memory < 0 && owner.firstPlayer)
                {
                    Player.gameManager.CmdEndTurn();
                }
                else if (MemoryChecker.Inst.memory > 0 && !owner.firstPlayer)
                {
                    Player.gameManager.CmdEndTurn();
                }

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

                //StartCoroutine(DelayDestroy(attacker));

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

    [ClientRpc]
    public void RpcAfterBattle(Entity attacker, Entity target)
    {
        //세큐리티로 인한 추가 Attack은 Rpc로
        if(attacker.GetComponent<FieldCard>().player==Player.localPlayer)
        {
            CreatureCard creature = (CreatureCard)attacker.GetComponent<FieldCard>().card.data;
            creature.Attack(attacker, target);
        }
    }
    [ClientRpc]
    public void RpcAttackEndCast(FieldCard fieldCard, Player player)
    {
        //CreatureCard creature = ((CreatureCard)fieldCard.card.data);
        //creature.AttackEndDigimonCasts(player);
        if(player==Player.localPlayer)
        {
            Debug.Log("공격 마무리 직전 인스턴트 메모리 : " + MemoryChecker.Inst.instantMemory);
            MemoryChecker.Inst.CmdChangeMemory(MemoryChecker.Inst.memory + MemoryChecker.Inst.instantMemory);
            MemoryChecker.Inst.CmdChangeInstantMemory(0);
            if (MemoryChecker.Inst.memory<0 && player.firstPlayer)
            {
                Player.gameManager.CmdEndTurn();
            }
            else if(MemoryChecker.Inst.memory<0 && !player.firstPlayer)
            {
                Player.gameManager.CmdEndTurn();
            }
        }
    }

    public IEnumerator DelayBattle(Entity attacker, Entity target)
    {
        yield return new WaitForSeconds(0.2f);
        //yield return null;

        while (attacker.GetComponent<FieldCard>().player.isTargeting)
        { yield return null; }

        if (!attacker.GetComponent<FieldCard>().player.isTargeting && ((Player)target).deck.securityCard.Count > 0)
        {
            ((Player)target).deck.CmdPlaySecurityCard(((Player)target).deck.securityCard[0], ((Player)target), attacker);
        }
    }

    public IEnumerator DelayDestroy(Entity attacker)
    {
        while(MemoryChecker.Inst.instantMemory != 0)
        {
            yield return null;
        }

        CmdDelayDestroyAttacker(attacker);
    }

    [Command(requiresAuthority = false)]
    public void CmdDelayDestroyAttacker(Entity attacker)
    {
        //RpcDelayDestroyAttacker(attacker);
        if (attacker.GetComponent<FieldCard>().player == Player.localPlayer)
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
        }
    }
    [ClientRpc]
    public void RpcDelayDestroyAttacker(Entity attacker)
    {
        if(attacker.GetComponent<FieldCard>().player==Player.localPlayer)
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
        }
        
    }
}

