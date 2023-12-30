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
    public bool isDigitamaOpenOrMove; // 디지타마를 오픈하거나 필드로 보내거나 한가지만 가능

    [Header("Deck")]
    public int deckSize = 50; // Maximum deck size
    public int identicalCardCount = 4; // How many identical cards we allow to have in a deck
    public static string localPlayerDeck; //덱 빌딩에서 짠 덱 저장용 변수

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

    /*[HideInInspector]*/ public Entity caster; // 타게팅 애로우 지우기용도 + attacker저장
    public Entity target; // target을 저장해두기

    //public SyncListPlayerInfo players = new SyncListPlayerInfo(); // Information of all players online. One is player, other is opponent.

    // Not sent from Player / Object with Authority, so we need to ignoreAuthority. 
    // We could also have this command run on the Player instead
    [Command(requiresAuthority = false)] 
    //권한 요구 = false 원본은 ignoreAuthority(권한 무시) = true 인데 업데이트로 바뀐듯
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
        if (cardObject == null) { return; } // 혹시나 싶은 안전코드
        FieldCard card = cardObject.GetComponent<FieldCard>();
        if (card == null) { return; }// 혹시나 싶은 안전코드
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
            if(card==null) return; // 혹시나 싶은 안전코드 cardObject==null이 맞나?
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
        else //내 턴이 아니게 된 플레이어는 CmdEndTurn함수를 통해 정돈할것 정돈
        {
            playerField.UpdateTamerEffect();//턴 끝날때 버프 제거하는 함수
            Player.localPlayer.deck.CmdEndTurn();
            Player.gameManager.isDigitamaOpenOrMove = false; // 턴 끝나면서 디지타마 오픈 상태 초기화

            //디지몬 효과로 인한 메모리 땡김(ex:메탈그레이몬)정돈
            if(Player.localPlayer.isServer)
            {
                MemoryChecker.Inst.CmdChangeMemory((MemoryChecker.Inst.memory) - (MemoryChecker.Inst.buffMemory));
            }
            else
            {
                //Debug.Log(MemoryChecker.Inst.buffMemory);
                //헷갈리지만 빼주는게 맞네..?
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
            playerField.UpdateTamerEffect();//턴 끝날때 버프 제거하는 함수
            Player.localPlayer.deck.CmdEndTurn();
            Player.gameManager.isDigitamaOpenOrMove = false; // 턴 끝나면서 디지타마 오픈 상태 초기화

            if (Player.localPlayer.isTargeting)
            {
                 //상대 턴으로 넘어가면 타게팅 마우스 없앰(아직 삭제 전이라 -1 삭제는 밑에 playerField.EndTurnFieldCards)
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
            panelText.text = "아직 상대가 옵션카드 대상을 고르는 중입니다";
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
        //    //블록당할시 진화원 효과 발동 효과 발동
        //    card = card.underCard;
        //    CreatureCard creatureCard = (CreatureCard)card.card.data;
        //    creatureCard.BlockedCast(card);
        //}
        caster.GetComponent<FieldCard>().CmdSyncBlocked(true); //블록 당했다!
        Debug.Log("블록당했다!");
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
            //무덤에 추가해야..
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
            //카드정보를 받고, 핸드카드로 새로 팝시키고, 무덤에 해당 카드 삭제
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
            //CmdSyncCaster(null);//캐스터 초기화
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
        //attacker의 클라 말고 RPC로 target클라에도 동기화 해줘야
        //target클라에서 블록 설정했을시 공격 가능
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
        CmdSyncCaster(null);//캐스터 초기화
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
