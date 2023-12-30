using UnityEngine;
using Mirror;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    [Header("Health")]
    public int maxHealth = 30;

    [Header("Mana")]
    public int maxMana = 10;

    [Header("Hand")]
    public int handSize = 7;
    public PlayerHand playerHand;
    public PlayerHand enemyHand;

    [Header("Digitama_Test")]
    public int tamaSize = 1;
    public PlayerRaiseField playerRaiseField;
    public PlayerRaiseField enemyRaiseField;
    public bool isDigitamaOpenOrMove; // ����Ÿ���� �����ϰų� �ʵ�� �����ų� �Ѱ����� ����

    [Header("Deck")]
    public int deckSize = 50; // Maximum deck size
    public int identicalCardCount = 4; // How many identical cards we allow to have in a deck
    public static string localPlayerDeck; //�� �������� § �� ����� ����

    [Header("Battlefield")]
    public PlayerField playerField;
    public PlayerField enemyField;

    [Header("Turn Management")]
    public GameObject endTurnButton;
    [HideInInspector] public bool isOurTurn = false;
    [SyncVar, HideInInspector] public int turnCount = 1; // Start at 1
    [SyncVar] public bool isGameStart;
    public GameObject waitingPanel;
    public Text panelText;

    [Header("Block")]
    public GameObject blockPanel;
    public Image attackerImage;
    public Image targetImage;
    public List<Image> blockButtonImage;

    [Header("Destroy")]
    public GameObject destroyPanel;
    public List<Image> destroyButtonImage;

    [Header("Revive")]
    public GameObject revivePanel;
    public List<Image> reviveButtonImage;
    public Image reviveSelectedImage;

    // isHovering is only set to true on the Client that called the OnCardHover function.
    // We only want the hovering to appear on the enemy's Client, so we must exclude the OnCardHover caller from the Rpc call.
    [HideInInspector] public bool isHovering = false;
    [HideInInspector] public bool isHoveringField = false;
    [HideInInspector] public bool isSpawning = false;

    /*[HideInInspector]*/ public Entity caster; // Ÿ���� �ַο� �����뵵 + attacker����
    public Entity target; // target�� �����صα�

    //public SyncListPlayerInfo players = new SyncListPlayerInfo(); // Information of all players online. One is player, other is opponent.

    // Not sent from Player / Object with Authority, so we need to ignoreAuthority. 
    // We could also have this command run on the Player instead
    [Command(requiresAuthority = false)] 
    //���� �䱸 = false ������ ignoreAuthority(���� ����) = true �ε� ������Ʈ�� �ٲ��
    public void CmdOnCardHover(float moveBy, int index)
    {
        // Only move cards if there are any in our opponent's opponent's hand (our hand from our opponent's point of view).
        if (enemyHand.handContent.transform.childCount > 0 && isServer) 
        { RpcCardHover(moveBy, index); }
    }

    [ClientRpc]
    public void RpcCardHover(float moveBy, int index)
    {
        // Only move card for the player that isn't currently hovering
        if (!isHovering)
        {
            HandCard card = enemyHand.handContent.transform.GetChild(index).GetComponent<HandCard>();
            card.transform.localPosition = new Vector2(card.transform.localPosition.x, moveBy);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdOnFieldCardHover(GameObject cardObject, bool activateShine, bool targeting)
    {
        if (cardObject == null) { return; } // Ȥ�ó� ���� �����ڵ�
        FieldCard card = cardObject.GetComponent<FieldCard>();
        if (card == null) { return; }// Ȥ�ó� ���� �����ڵ�
        card.shine.gameObject.SetActive(true);
        if (isServer) RpcFieldCardHover(cardObject, activateShine, targeting);
    }

    [ClientRpc]
    public void RpcFieldCardHover(GameObject cardObject, bool activateShine, bool targeting)
    {
        if (!isHoveringField)
        {
            if (cardObject.IsDestroyed()) { return; }
            if(cardObject == null) { return; }
            FieldCard card = cardObject.GetComponent<FieldCard>();
            if(card==null) return; // Ȥ�ó� ���� �����ڵ� cardObject==null�� �³�?
            Color shine = activateShine ? card.hoverColor : Color.clear;
            card.shine.color = targeting ? card.targetColor : shine;
            card.shine.gameObject.SetActive(activateShine);
        }
    }

    // Ends our turn and starts our opponent's turn.
    [Command(requiresAuthority = false)]
    public void CmdEndTurn()
    {
        RpcSetTurn();
    }

    [Command(requiresAuthority = false)]
    public void CmdPassTurn()
    {
        RpcSetPass();
    }

    [ClientRpc]
    public void RpcSetTurn()
    {
        // If isOurTurn was true, set it false. If it was false, set it true.
        isOurTurn = !isOurTurn;
        endTurnButton.SetActive(isOurTurn);
        ++turnCount;

        // If isOurTurn (after updating the bool above)
        if (isOurTurn)
        {
            playerField.UpdateFieldCards();
            playerRaiseField.UpdateRaiseCards();
            Player.localPlayer.deck.CmdStartNewTurn();
        }
        else //�� ���� �ƴϰ� �� �÷��̾�� CmdEndTurn�Լ��� ���� �����Ұ� ����
        {
            playerField.UpdateTamerEffect();//�� ������ ���� �����ϴ� �Լ�
            Player.localPlayer.deck.CmdEndTurn();
            Player.gameManager.isDigitamaOpenOrMove = false; // �� �����鼭 ����Ÿ�� ���� ���� �ʱ�ȭ

            //������ ȿ���� ���� �޸� ����(ex:��Ż�׷��̸�)����
            if(Player.localPlayer.isServer)
            {
                MemoryChecker.Inst.CmdChangeMemory((MemoryChecker.Inst.memory) - (MemoryChecker.Inst.buffMemory));
            }
            else
            {
                //Debug.Log(MemoryChecker.Inst.buffMemory);
                //�򰥸����� ���ִ°� �³�..?
                MemoryChecker.Inst.CmdChangeMemory((MemoryChecker.Inst.memory) - (MemoryChecker.Inst.buffMemory));
            }
            MemoryChecker.Inst.buffMemory = 0;
        }
        playerField.EndBuffTurnSpellCards();
        //playerField.UpdateTurnEvoEffect();
    }

    [ClientRpc]
    public void RpcSetPass()
    {
        if(Player.localPlayer.firstPlayer && isOurTurn)
        { MemoryChecker.Inst.CmdChangeMemory(-3 - (MemoryChecker.Inst.buffMemory)); }
        
        else if(!Player.localPlayer.firstPlayer && isOurTurn)
        { MemoryChecker.Inst.CmdChangeMemory(3 + (MemoryChecker.Inst.buffMemory)); }

        // If isOurTurn was true, set it false. If it was false, set it true.
        isOurTurn = !isOurTurn;
        endTurnButton.SetActive(isOurTurn);
        ++turnCount;
        MemoryChecker.Inst.buffMemory = 0;

        // If isOurTurn (after updating the bool above)
        if (isOurTurn)
        {
            playerField.UpdateFieldCards();
            playerRaiseField.UpdateRaiseCards();
            Player.localPlayer.deck.CmdStartNewTurn();
        }
        else
        {
            playerField.UpdateTamerEffect();//�� ������ ���� �����ϴ� �Լ�
            Player.localPlayer.deck.CmdEndTurn();
            Player.gameManager.isDigitamaOpenOrMove = false; // �� �����鼭 ����Ÿ�� ���� ���� �ʱ�ȭ

            if (Player.localPlayer.isTargeting)
            {
                 //��� ������ �Ѿ�� Ÿ���� ���콺 ����(���� ���� ���̶� -1 ������ �ؿ� playerField.EndTurnFieldCards)
                 if (((FieldCard)caster).card.data is SpellCard spellCard && spellCard.buff.buffTurn - 1 <= 0)
                 {
                     caster.DestroyTargetingArrow();
                     caster = null;
                 }
            }
        }

        playerField.EndBuffTurnSpellCards();
        //playerField.UpdateTurnEvoEffect();
    }

    public void StartGame()
    {
        endTurnButton.SetActive(true);
        Player player = Player.localPlayer;

        if (player.firstPlayer)
        { isOurTurn = true; }
        RpcStartGame();
    }

    [ClientRpc]
    public void RpcStartGame()
    { isGameStart = true; }

    private void Update()
    {
        if(Player.localPlayer!=null && Player.localPlayer.hasEnemy&&Player.localPlayer.IsOurTurn() && Player.localPlayer.enemyInfo.data.isTargeting)
        {
            waitingPanel.SetActive(true);
            panelText.text = "���� ��밡 �ɼ�ī�� ����� ���� ���Դϴ�";
        }
        else
        {
            waitingPanel.SetActive(false);
        }

    }

    public void OnBlockButtonClick(int index)
    {
        //FieldCard card = caster.GetComponent<FieldCard>();

        //while (card.GetComponent<FieldCard>().isUnderMostCard == false)
        //{
        //    //��ϴ��ҽ� ��ȭ�� ȿ�� �ߵ� ȿ�� �ߵ�
        //    card = card.underCard;
        //    CreatureCard creatureCard = (CreatureCard)card.card.data;
        //    creatureCard.BlockedCast(card);
        //}
        caster.GetComponent<FieldCard>().CmdSyncBlocked(true); //��� ���ߴ�!
        Debug.Log("��ϴ��ߴ�!");
        caster.combat.CmdBattle(caster,Player.localPlayer.UICardsList[index]);
        Player.localPlayer.CmdSyncTargeting(Player.localPlayer,false);
        CmdSyncCaster(null);
        CmdSyncTarget(null);
        blockPanel.SetActive(false);
    }
    public void OnDestroyButtonClick(int index)
    {
        FieldCard destroyCard = Player.localPlayer.UICardsList[index];
        while(destroyCard != destroyCard.isUnderMostCard)
        {
            destroyCard.player.deck.CmdAddGraveyard(destroyCard.player,destroyCard.card);
            //NetworkServer.Destroy(destroyCard.gameObject);
            destroyCard.CmdServerDestroyCard(destroyCard);
            destroyCard = destroyCard.underCard;
            //������ �߰��ؾ�..
        }
        //NetworkServer.Destroy(destroyCard.gameObject);
        destroyCard.player.deck.CmdAddGraveyard(destroyCard.player, destroyCard.card);
        destroyCard.CmdServerDestroyCard(destroyCard);
        Player.localPlayer.CmdSyncTargeting(Player.localPlayer, false);
        CmdSyncCaster(null);
        CmdSyncTarget(null);
        destroyPanel.SetActive(false);
    }
    public void OnReviveButtonClick(int index)
    {
        //if(caster.GetComponent<FieldCard>().player == Player.localPlayer)
        {
            Player owner = caster.GetComponent<FieldCard>().player;
            //ī�������� �ް�, �ڵ�ī��� ���� �˽�Ű��, ������ �ش� ī�� ����
            CardInfo reviveCard = owner.UICardInfoList[index];
            owner.CmdDrawSpecificCard(reviveCard, owner);
            owner.CmdRemoveGraveyard(reviveCard);

            for(int i =0; i< reviveButtonImage.Count; i++)
            {
                reviveButtonImage[i].gameObject.SetActive(false); 
            }

            CmdSetSelectedCardImage(reviveCard);
            StartCoroutine(WaitForSec(owner, 1.5f));

            //owner.CmdSyncTargeting(owner, false);
            //CmdSyncCaster(null);//ĳ���� �ʱ�ȭ
            //CmdSyncTarget(null);
            ////revivePanel.SetActive(false);
            //owner.CmdSetActiveRevivePanel(owner, false);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSyncCaster(Entity caster)
    {
        Player.gameManager.caster = caster;
        RpcSyncCaster(caster);
        //attacker�� Ŭ�� ���� RPC�� targetŬ�󿡵� ����ȭ �����
        //targetŬ�󿡼� ��� ���������� ���� ����
    }
    [ClientRpc]
    public void RpcSyncCaster(Entity caster)
    {
        Player.gameManager.caster = caster;
    }

    [Command(requiresAuthority = false)]
    public void CmdSyncTarget(Entity target)
    {
        Player.gameManager.target = target;
        RpcSyncTarget(target);
    }
    [ClientRpc]
    public void RpcSyncTarget(Entity target)
    {
        Player.gameManager.target = target;
    }

    IEnumerator WaitForSec(Player owner, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        owner.CmdSyncTargeting(owner, false);
        CmdSyncCaster(null);//ĳ���� �ʱ�ȭ
        CmdSyncTarget(null);
        //revivePanel.SetActive(false);
        owner.CmdSetActiveRevivePanel(owner, false);
    }
    [Command(requiresAuthority = false)]
    public void CmdSetSelectedCardImage(CardInfo card)
    {
        RpcSetSelectedCardImage(card);
    }
    [ClientRpc]
    public void RpcSetSelectedCardImage(CardInfo card) 
    {
        reviveSelectedImage.gameObject.SetActive(true);
        reviveSelectedImage.sprite = card.image;
    }
}
