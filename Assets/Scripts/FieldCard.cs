using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class FieldCard : Entity
{
    [SyncVar/*, HideInInspector*/] public CardInfo card; // Get card info

    [Header("Card Properties")]
    public Image image; // card image on field
    public Text cardName; // Text of the card name
    public Text AbilityText; 
    public Text SecurityCheckText; 
    public Text DPbuffText;
    public Text isRestText;

    [Header("Spell / Creature Card")]
    public bool giveBuff = false; // 1회용 버프 썼는가?(스펠카드용)
    [SyncVar] public bool attacked = false; // 이 턴에 공격을 했는가?(크리처 카드용)

    [Header("Shine")]
    public Image shine;
    public Color hoverColor;
    public Color readyColor; // Shine color when ready to attack
    public Color targetColor; // Shine color when ready to attack

    [Header("Card Hover")]
    public HandCard cardHover;

    [Header("Owner")]
    [SyncVar]
    public Player player;

    [Header("Evo Route")]
    public FieldCard upperCard;
    public FieldCard underCard;
    public bool isUpperMostCard => upperCard == null; // 최상단 카드인가?
    public bool isUnderMostCard => underCard == null; // 최하단 카드인가?
    [SyncVar]public int evoCount;// 최상단에서 진화원 개수

    [Header("Security")]
    [SyncVar]public bool isSecurity = false;
    //[Header("SpellEffect")]
    //readonly public SyncList<Buffs> buffs = new SyncList<Buffs>(); // 효과 받은 수치를 저장해 두기
    /*[SyncVar]*/ public Buffs tempBuff;//텍스트 시각효과를 위해 받아놓을 버프 변수

    [Header("Buffs")]
    [SyncVar] public int securityAttack = 0;
    public int buffTargetCount = 1;
    public bool isMyTurnDigimonCastingActive = false;
    public bool isMyTurnEvoCastingActive = false;//진화원 버프가 한번 들어왔는가?(매 프레임 찰나로 인한 중복 들어옴 방지)
    [Header("Dragging")]
    public FieldCardHover cardDragHover;
    [Header("Status")]
    [SyncVar] public bool blocked = false; //블록 "당했는가"?

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        // If we have a card but no sprite, make sure the sprite is up to date since we can't SyncVar the sprite.
        // Useful to avoid bugs when a player was offline when the card spawned, or if they reconnected.
        if (image.sprite == null && (card.name != null || cardName.text == ""))
        {
            // Update Stats
            image.color = Color.white;
            image.sprite = card.image;

            //cardName.text = card.name; //터진다~

            // Update card hover info
            cardHover.UpdateFieldCardInfo(card);
        }

        //healthText.text = health.ToString();
        //strengthText.text = strength.ToString();

        if (CanAttack()) shine.color = readyColor;
        else if (CantAttack()) shine.color = Color.clear;

        ChaseUpperCard();

        //최상단 카드가 아니면 콜리전끄기test
        //if(isUpperMostCard==false) { GetComponent<BoxCollider2D>().enabled = false; }
        //else { GetComponent<BoxCollider2D>().enabled = true; }

        if (player==Player.localPlayer && casterType==Target.MY_BABY && cardDragHover!=null /*&& waitTurn <= 0*/)
        {
            if (Player.gameManager.isOurTurn && Player.localPlayer.enemyInfo.data.isTargeting==false)
            {
                cardDragHover.canDrag = true; //player.deck.CanPlayCard(manaCost); //원래는 마나 총량 넘으면 못내게 했는데 ECost라는 다른 루트땜에 일단 낼 수 있게함
            }
        }    

        if(player == Player.localPlayer && player.IsOurTurn() /*&& isMyTurnEvoCastingActive == false*/)
        {
            if(card.data is CreatureCard)
            {
                ((CreatureCard)card.data).MyTurnCast(this, FindMostUpperCard());
            }
            //isMyTurnCastingActive = true;
        }
        if (player == Player.localPlayer && player.IsOurTurn() /*&& isMyTurnDigimonCastingActive == false*/)
        {
            if (card.data is CreatureCard)
            {
                ((CreatureCard)card.data).MyTurnDigimonCast(this, this);
            }
            //isMyTurnCastingActive = true;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateWaitTurn()
    {
        CmdChangeAttacked(false);
        CmdRotation(this, Quaternion.identity);
        //Debug.LogError("Here");
        if (waitTurn > 0) { waitTurn--; }
    }

    public void ChaseUpperCard()
    {
        if(isUpperMostCard==false && upperCard!=null && !cardDragHover.isDragging)//맨 위 카드가 아니고 위 카드 정보가 null이 아니라면
        {
            // 상대편에서 내 카드 볼때 메모리체커에 가려짐 반대로 해야할듯
            //GetComponent<RectTransform>().anchoredPosition = upperCard.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, -47); 
            upperCard.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + new Vector2(0, 47);
        }
    }
    public FieldCard FindMostUpperCard()
    {
        FieldCard mostCard = this;
        while(!mostCard.isUpperMostCard)
        {
            mostCard = mostCard.upperCard;
        }
        return mostCard;
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroySpellCard()
    {
        if (card.data is SpellCard spellCard && isTargeting == false)
        {
            //player.deck.playerField.Remove(card);//안씀
            //player.deck.graveyard.Add(card);//카드를 꺼내자마자 무덤에 저장되야함
            spellCard.EndTurnEffect(player);
            
            if (!spellCard.isTamer)
            { 
                Destroy(this.gameObject); 
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeSomeThing(Buffs buff, bool isStart)
    {
        if(isStart)
        {
            //isStart는 버프를 추가할 때인지 뺄 때인지 구분용
            if (buff.isFix)
            {
                //해당 버프가 고정값을 주면
                strength = buff.buffDP;
                //strength += -((CreatureCard)card.data).strength + buff.buffDP;
            }
            else
            {
                //여기에 if(버프 whenItSpear==true)라면 최상단 카드가 hasSpear일때만 발동 아니면 return;
                //해당 버프가 더하는 값이면
                strength += buff.buffDP;
                securityAttack += buff.securityAttack;
                waitTurn += buff.waitTurn;
            }

            if(strength <= 0)
            {
                //공깎 디버프로 최종 DP가 0이하가 되면 소멸
                IsDead = true;
                player.deck.graveyard.Add(this.card);
                Destroy(this.gameObject);
            }

            if(buff.breakEvo)
            {
                //진화원 퇴화면 이쪽으로
                CmdRemoveEvo(buff.removeEvoCount);
            }

            RpcTextSetActive(buff, isStart);
        }
        else
        {
            if(buff.isFix)
            {
                strength = ((CreatureCard)card.data).strength;
            }
            else
            {
                //tempBuff.buffDP -= buff.buffDP;//??이게 왜 여기에?
                strength -= buff.buffDP;
            }
            securityAttack = 0;

            
            tempBuff.securityAttack = 0;

            if (tempBuff.buffDP != 0)
            {
                DPbuffText.gameObject.SetActive(true);
                DPbuffText.text = "DP + " + tempBuff.buffDP.ToString();
            }
            else
            {
                DPbuffText.gameObject.SetActive(false);
            }
            RpcTextSetActive(buff, isStart);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAddBuff(Buffs buff)
    {
        Debug.Log(player + "의 " + buff.cardname + " 의 버프 추가중..");
        buffs.Add(buff);
        Debug.Log(player + "의 " + buff.cardname + " 의 버프 추가 완료!");
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveBuff(int index)
    {
        Debug.Log(player + "의 " + card.data.cardName + "의 버프인 " + buffs[index].cardname + " 제거 진행중");
        buffs.RemoveAt(index);
        Debug.Log("버프 인덱스로 제거 완료");
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveBuff(string buffName)
    {
        Buffs buff = buffs.Find(buffs => buffName.Equals(buffs.cardname));
        if(buff == null) 
        {
            Debug.Log(buffName + " 버프 없음");
            return; 
        }
        Debug.Log(buff.cardname + " 버프 특정해 제거 진행중");
        buffs.Remove(buff);
        Debug.Log(buff.cardname + " 버프 특정해 제거 완료");
    }
    [ClientRpc]
    public void RpcTextSetActive(Buffs buff, bool isStart)
    {
        if (isStart)
        {
            tempBuff.isFix = buff.isFix;
            tempBuff.buffDP += buff.buffDP;
            tempBuff.securityAttack += buff.securityAttack;
            //tempBuff.buffTurn = buff.buffTurn;

            if (tempBuff.buffDP != 0)
            {
                DPbuffText.gameObject.SetActive(true);
                if(tempBuff.isFix)
                {
                    DPbuffText.text = "DP = " + tempBuff.buffDP.ToString();
                }
                else
                {
                    DPbuffText.text = "DP + " + tempBuff.buffDP.ToString();
                }
            }
            
            if (tempBuff.securityAttack > 0)
            {
                SecurityCheckText.gameObject.SetActive(true);
                SecurityCheckText.text = "S.C. + " + tempBuff.securityAttack.ToString();
            }

            if(tempBuff.waitTurn > 0)
            {
                isRestText.gameObject.SetActive(true);
                isRestText.text = "레스트";
            }
        }

        else
        {
            tempBuff.buffDP -= buff.buffDP;
            tempBuff.securityAttack = 0;
            
            DPbuffText.gameObject.SetActive(false);
            SecurityCheckText.gameObject.SetActive(false);
            //isRestText.gameObject.SetActive(false);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveEvo(int Count/*, bool isAscending //나중에 오름차순,내림차순 부분삭제할때 쓰기*/)
    {
        if(Count == -1)
        {
            FieldCard fieldCard = this;
            FieldCard upperMostCard = fieldCard.FindMostUpperCard();

            if (fieldCard.isUnderMostCard && fieldCard.isUpperMostCard) { return; } //최상단겸 최하단(단 한장)이면 리턴

            while(!fieldCard.isUnderMostCard)
            {
                //최하단 카드를 가져와서
                fieldCard = fieldCard.underCard;
            }
            while(!fieldCard.isUpperMostCard)
            {
                //무덤 보내기
                fieldCard.player.deck.graveyard.Add(fieldCard.card);
                //제거되는 카드이름의 버프 제거
                upperMostCard.CmdChangeSomeThing(((CreatureCard)fieldCard.card.data).evolutionBuff, false);
                upperMostCard.CmdRemoveBuff(((CreatureCard)fieldCard.card.data).evolutionBuff.cardname);
                //차례로 아래서부터 모두 파괴
                Destroy(fieldCard.gameObject);
                fieldCard = fieldCard.upperCard;
                fieldCard.underCard = null;
            }
            evoCount = 0; //진화원 전부 파괴이기에 evoCount도 맞게 초기화
            RpcRemoveEvoAfter(fieldCard);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdServerDestroyCard(FieldCard card)
    {
        NetworkServer.Destroy(card.gameObject);
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroyCard(FieldCard card)
    {
        if(isServer) { RpcDestroyCard(card); }
    }
    [ClientRpc]
    public void RpcDestroyCard(FieldCard card)
    {
        Destroy(card.gameObject);
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeAttacked(bool TorF)
    {
        attacked = TorF;
    }

    [Command(requiresAuthority = false)]
    public void CmdRotation(FieldCard card, Quaternion rotation)
    {
        card.GetComponent<RectTransform>().rotation = rotation;
        card.cardHover.GetComponent<RectTransform>().rotation = Quaternion.identity;
        RpcRotation(card, rotation);
    }

    [Command(requiresAuthority = false)]
    public void CmdSyncBlocked(bool TorF)
    {
        this.blocked = TorF;
    }

    [Command(requiresAuthority = false)]
    public void CmdDigimonCast()
    {
        RpcDigimonCast();
    }
    [ClientRpc]
    public void RpcDigimonCast()
    {
        if (Player.localPlayer == player && card.data is CreatureCard creatureCard && isUpperMostCard)
        {
            creatureCard.DigimonCast(this);
        }
    }

    [ClientRpc]
    public void RpcRotation(FieldCard card, Quaternion rotation)
    {
        card.GetComponent<RectTransform>().rotation = rotation;
        card.cardHover.GetComponent<RectTransform>().rotation = Quaternion.identity;
    }

    [ClientRpc]
    public void RpcRemoveEvoAfter(FieldCard fieldCard)
    {
        fieldCard.underCard = null;
        fieldCard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //y축 정렬용 x축은 Update로 자동 정렬됨
    }

    [Command(requiresAuthority = false)]
    public void CmdMakeSecurity(bool Tof)
    {
        isSecurity = Tof;
    }
}