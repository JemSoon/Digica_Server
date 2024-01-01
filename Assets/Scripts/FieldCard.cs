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
    public bool giveBuff = false; // 1ȸ�� ���� ��°�?(����ī���)
    [SyncVar] public bool attacked = false; // �� �Ͽ� ������ �ߴ°�?(ũ��ó ī���)

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
    public bool isUpperMostCard => upperCard == null; // �ֻ�� ī���ΰ�?
    public bool isUnderMostCard => underCard == null; // ���ϴ� ī���ΰ�?
    [SyncVar]public int evoCount;// �ֻ�ܿ��� ��ȭ�� ����

    [Header("Security")]
    [SyncVar]public bool isSecurity = false;
    //[Header("SpellEffect")]
    //readonly public SyncList<Buffs> buffs = new SyncList<Buffs>(); // ȿ�� ���� ��ġ�� ������ �α�
    /*[SyncVar]*/ public Buffs tempBuff;//�ؽ�Ʈ �ð�ȿ���� ���� �޾Ƴ��� ���� ����

    [Header("Buffs")]
    [SyncVar] public int securityAttack = 0;
    public int buffTargetCount = 1;
    public bool isMyTurnDigimonCastingActive = false;
    public bool isMyTurnEvoCastingActive = false;//��ȭ�� ������ �ѹ� ���Դ°�?(�� ������ ������ ���� �ߺ� ���� ����)
    [Header("Dragging")]
    public FieldCardHover cardDragHover;
    [Header("Status")]
    [SyncVar] public bool blocked = false; //��� "���ߴ°�"?

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

            //cardName.text = card.name; //������~

            // Update card hover info
            cardHover.UpdateFieldCardInfo(card);
        }

        //healthText.text = health.ToString();
        //strengthText.text = strength.ToString();

        if (CanAttack()) shine.color = readyColor;
        else if (CantAttack()) shine.color = Color.clear;

        ChaseUpperCard();

        //�ֻ�� ī�尡 �ƴϸ� �ݸ�������test
        //if(isUpperMostCard==false) { GetComponent<BoxCollider2D>().enabled = false; }
        //else { GetComponent<BoxCollider2D>().enabled = true; }

        if (player==Player.localPlayer && casterType==Target.MY_BABY && cardDragHover!=null /*&& waitTurn <= 0*/)
        {
            if (Player.gameManager.isOurTurn && Player.localPlayer.enemyInfo.data.isTargeting==false)
            {
                cardDragHover.canDrag = true; //player.deck.CanPlayCard(manaCost); //������ ���� �ѷ� ������ ������ �ߴµ� ECost��� �ٸ� ��Ʈ���� �ϴ� �� �� �ְ���
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
        if(isUpperMostCard==false && upperCard!=null && !cardDragHover.isDragging)//�� �� ī�尡 �ƴϰ� �� ī�� ������ null�� �ƴ϶��
        {
            // ������� �� ī�� ���� �޸�üĿ�� ������ �ݴ�� �ؾ��ҵ�
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
            //player.deck.playerField.Remove(card);//�Ⱦ�
            //player.deck.graveyard.Add(card);//ī�带 �����ڸ��� ������ ����Ǿ���
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
            //isStart�� ������ �߰��� ������ �� ������ ���п�
            if (buff.isFix)
            {
                //�ش� ������ �������� �ָ�
                strength = buff.buffDP;
                //strength += -((CreatureCard)card.data).strength + buff.buffDP;
            }
            else
            {
                //���⿡ if(���� whenItSpear==true)��� �ֻ�� ī�尡 hasSpear�϶��� �ߵ� �ƴϸ� return;
                //�ش� ������ ���ϴ� ���̸�
                strength += buff.buffDP;
                securityAttack += buff.securityAttack;
                waitTurn += buff.waitTurn;
            }

            if(strength <= 0)
            {
                //���� ������� ���� DP�� 0���ϰ� �Ǹ� �Ҹ�
                IsDead = true;
                player.deck.graveyard.Add(this.card);
                Destroy(this.gameObject);
            }

            if(buff.breakEvo)
            {
                //��ȭ�� ��ȭ�� ��������
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
                //tempBuff.buffDP -= buff.buffDP;//??�̰� �� ���⿡?
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
        Debug.Log(player + "�� " + buff.cardname + " �� ���� �߰���..");
        buffs.Add(buff);
        Debug.Log(player + "�� " + buff.cardname + " �� ���� �߰� �Ϸ�!");
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveBuff(int index)
    {
        Debug.Log(player + "�� " + card.data.cardName + "�� ������ " + buffs[index].cardname + " ���� ������");
        buffs.RemoveAt(index);
        Debug.Log("���� �ε����� ���� �Ϸ�");
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveBuff(string buffName)
    {
        Buffs buff = buffs.Find(buffs => buffName.Equals(buffs.cardname));
        if(buff == null) 
        {
            Debug.Log(buffName + " ���� ����");
            return; 
        }
        Debug.Log(buff.cardname + " ���� Ư���� ���� ������");
        buffs.Remove(buff);
        Debug.Log(buff.cardname + " ���� Ư���� ���� �Ϸ�");
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
                isRestText.text = "����Ʈ";
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
    public void CmdRemoveEvo(int Count/*, bool isAscending //���߿� ��������,�������� �κл����Ҷ� ����*/)
    {
        if(Count == -1)
        {
            FieldCard fieldCard = this;
            FieldCard upperMostCard = fieldCard.FindMostUpperCard();

            if (fieldCard.isUnderMostCard && fieldCard.isUpperMostCard) { return; } //�ֻ�ܰ� ���ϴ�(�� ����)�̸� ����

            while(!fieldCard.isUnderMostCard)
            {
                //���ϴ� ī�带 �����ͼ�
                fieldCard = fieldCard.underCard;
            }
            while(!fieldCard.isUpperMostCard)
            {
                //���� ������
                fieldCard.player.deck.graveyard.Add(fieldCard.card);
                //���ŵǴ� ī���̸��� ���� ����
                upperMostCard.CmdChangeSomeThing(((CreatureCard)fieldCard.card.data).evolutionBuff, false);
                upperMostCard.CmdRemoveBuff(((CreatureCard)fieldCard.card.data).evolutionBuff.cardname);
                //���ʷ� �Ʒ������� ��� �ı�
                Destroy(fieldCard.gameObject);
                fieldCard = fieldCard.upperCard;
                fieldCard.underCard = null;
            }
            evoCount = 0; //��ȭ�� ���� �ı��̱⿡ evoCount�� �°� �ʱ�ȭ
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
        fieldCard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //y�� ���Ŀ� x���� Update�� �ڵ� ���ĵ�
    }

    [Command(requiresAuthority = false)]
    public void CmdMakeSecurity(bool Tof)
    {
        isSecurity = Tof;
    }
}