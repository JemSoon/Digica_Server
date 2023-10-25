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

    public bool giveBuff = false; // 1ȸ�� ���� ��°�?(����ī���)

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

    [Header("Security")]
    [SyncVar]public bool isSecurity = false;
    [Header("SpellEffect")]
    readonly public SyncList<Buffs> buffs = new SyncList<Buffs>(); // ȿ�� ���� ��ġ�� ������ �α�
    public Buffs tempBuff;//�ؽ�Ʈ �ð�ȿ���� ���� �޾Ƴ��� ���� ����

    [Header("Buffs")]
    [SyncVar] public int securityAttack = 0;
    [Header("Test")]
    public FieldCardHover cardDragHover;

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
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateWaitTurn()
    {
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

    [Command(requiresAuthority = false)]
    public void CmdDestroySpellCard()
    {
        if (card.data is SpellCard spellCard && isTargeting == false)
        {
            //player.deck.playerField.Remove(card);//�Ⱦ�
            //player.deck.graveyard.Add(card);//ī�带 �����ڸ��� ������ ����Ǿ���
            spellCard.EndTurnEffect(player);
            Destroy(this.gameObject);
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
                tempBuff.buffDP -= buff.buffDP;
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
        buffs.Add(buff);
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveBuff(int index)
    {
        buffs.RemoveAt(index);
        Debug.Log("���� ���� �Ϸ�");
    }
    [ClientRpc]
    public void RpcTextSetActive(Buffs buff, bool isStart)
    {
        if (isStart)
        {
            tempBuff.isFix = buff.isFix;
            tempBuff.buffDP += buff.buffDP;
            tempBuff.securityAttack += buff.securityAttack;

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
            isRestText.gameObject.SetActive(false);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveEvo(int Count/*, bool isAscending //���߿� ��������,�������� �κл����Ҷ� ����*/)
    {
        if(Count == -1)
        {
            FieldCard fieldCard = this;

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
                //���ʷ� �Ʒ������� ��� �ı�
                Destroy(fieldCard.gameObject);
                fieldCard = fieldCard.upperCard;
                fieldCard.underCard = null;
            }
            RpcRemoveEvoAfter(fieldCard);
        }
    }

    [ClientRpc]
    public void RpcRemoveEvoAfter(FieldCard fieldCard)
    {
        fieldCard.underCard = null;
        fieldCard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //y�� ���Ŀ� x���� Update�� �ڵ� ���ĵ�
    }
}