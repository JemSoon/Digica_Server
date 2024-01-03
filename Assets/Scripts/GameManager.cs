using UnityEngine;
using Mirror;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
public enum PanelType : byte { Revive, PickUp, }

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
    public List<Image> reviveUIImage;

    [Header("PickUp")]
    public GameObject pickUpPanel;
    public List<Image> pickUpButtonImage;
    public List<Image> pickUpUIImage;

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
        RpcSetEndTurn();
    }

    [Command(requiresAuthority = false)]
    public void CmdPassTurn()
    {
        RpcSetPass();
    }

    [Command(requiresAuthority = false)]
    public void CmdStartTurn()
    {
        RpcSetStartTurn();
    }

    [ClientRpc]
    public void RpcSetEndTurn()
    {
        // If isOurTurn was true, set it false. If it was false, set it true.
        isOurTurn = !isOurTurn;
        endTurnButton.SetActive(isOurTurn);
        ++turnCount;
        // If isOurTurn (after updating the bool above)
        //if (isOurTurn)
        //{
        //    playerField.UpdateFieldCards();
        //    playerRaiseField.UpdateRaiseCards();
        //    Player.localPlayer.deck.CmdStartNewTurn();
        //}
        if (!isOurTurn) 
        {
            //내 턴이 아니게 된 플레이어는 CmdEndTurn함수를 통해 정돈할것 정돈
            playerField.UpdateTamerEffect();//턴 끝날때 버프 제거하는 함수
            Player.localPlayer.deck.CmdEndTurn();
            Player.gameManager.isDigitamaOpenOrMove = false; // 턴 끝나면서 디지타마 오픈 상태 초기화

            //디지몬 효과로 인한 메모리 땡김(ex:메탈그레이몬)정돈
            if (Player.localPlayer.isServer)
            {
                MemoryChecker.Inst.CmdChangeMemory((MemoryChecker.Inst.memory) - (MemoryChecker.Inst.buffMemory));
                if (MemoryChecker.Inst.memory < -10) 
                { MemoryChecker.Inst.CmdChangeMemory(-10); }
                else if(MemoryChecker.Inst.memory > 10)
                { MemoryChecker.Inst.CmdChangeMemory(10); }
            }
            else
            {
                //Debug.Log(MemoryChecker.Inst.buffMemory);
                //헷갈리지만 빼주는게 맞네..?
                //Debug.Log("변경값 준 뒤 디버프 메모리 처리할 때 의 메모리" + MemoryChecker.Inst.memory);
                MemoryChecker.Inst.CmdChangeMemory((MemoryChecker.Inst.memory) - (MemoryChecker.Inst.buffMemory));
                if (MemoryChecker.Inst.memory > 10)
                { MemoryChecker.Inst.CmdChangeMemory(10); }
                else if (MemoryChecker.Inst.memory < -10)
                { MemoryChecker.Inst.CmdChangeMemory(-10); }
            }
            MemoryChecker.Inst.buffMemory = 0;
            CmdStartTurn();
        }
        playerField.EndBuffTurnSpellCards();
        //끝나는 턴의 효과들을 마무리 한 후 넘어간 턴의 시작
        
    }

    [ClientRpc]
    public void RpcSetPass()
    {
        if(Player.localPlayer.firstPlayer && isOurTurn)
        { MemoryChecker.Inst.CmdChangeMemory(-3 - (MemoryChecker.Inst.buffMemory)); }
        
        else if(!Player.localPlayer.firstPlayer && isOurTurn)
        { MemoryChecker.Inst.CmdChangeMemory(3 - (MemoryChecker.Inst.buffMemory)); }

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

    [ClientRpc]
    public void RpcSetStartTurn()
    {
        //If isOurTurn(after updating the bool above)
        if (isOurTurn)
        {
            playerField.UpdateFieldCards();
            playerRaiseField.UpdateRaiseCards();
            Player.localPlayer.deck.CmdStartNewTurn();
        }
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
        Player.localPlayer.UICardsList[index].CmdChangeAttacked(true); //공격한 상태로 변경
        Player.localPlayer.UICardsList[index].combat.CmdIncreaseWaitTurn(); //대기시간 추가
        Player.localPlayer.UICardsList[index].CmdRotation(Player.localPlayer.UICardsList[index], Quaternion.Euler(0, 0, -90));//레스트로 돌린다
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
        Player owner = caster.GetComponent<FieldCard>().player;
        //카드정보를 받고, 핸드카드로 새로 팝시키고, 무덤에 해당 카드 삭제
        CardInfo reviveCard = owner.UICardInfoList[index];

        if (DemandReviveButtonClick(reviveCard))
        {
            owner.CmdDrawSpecificCard(reviveCard, owner);
            owner.CmdRemoveGraveyard(reviveCard);

            for(int i =0; i< reviveButtonImage.Count; i++)
            {
                reviveButtonImage[i].gameObject.SetActive(false); 
            }

            CmdSetSelectedCardImage(reviveCard,PanelType.Revive, true);
            StartCoroutine(WaitForSec(owner, 1.5f , PanelType.Revive));
        }
    }

    public void OnPickUpButtonClick(int index)
    {
        Player owner = caster.GetComponent<FieldCard>().player;
        CardInfo pickCard = owner.UICardInfoList[index];

        if (DemandReviveButtonClick(pickCard))
        {
            owner.CmdDrawSpecificCard(pickCard, owner);

            for (int i = 0; i < reviveButtonImage.Count; i++)
            {
                pickUpButtonImage[i].gameObject.SetActive(false);
            }

            CmdSetSelectedCardImage(pickCard, PanelType.PickUp, true);
            StartCoroutine(WaitForSec(owner, 1.5f, PanelType.PickUp));
        }
        else
        {
            //고를게 없는데 아무거나 누르면
            for (int i = 0; i < reviveButtonImage.Count; i++)
            {
                pickUpButtonImage[i].gameObject.SetActive(false);
            }

            CmdSetSelectedCardImage(pickCard, PanelType.PickUp, false);
            StartCoroutine(WaitForSec(owner, 1.5f, PanelType.PickUp));
        }
    }

    private bool DemandReviveButtonClick(CardInfo card)
    {
        // 원하는 조건에 따라 클릭 여부를 결정
        // 예: return true; // 항상 실행
        // 예: return someCondition; // 특정 조건을 만족할 때만 실행
        bool someCondition = false;

        switch (caster.GetComponent<FieldCard>().card.data.cardName)
        {
            case "아구몬 박사":
                someCondition = true;
                break;

            case "아구몬":
                someCondition = card.data is SpellCard spellCard && spellCard.isTamer;
                break;
            default:
                someCondition = false;
                break;
        }

        return someCondition; // 기본적으로 항상 실행
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

    IEnumerator WaitForSec(Player owner, float seconds, PanelType panel)
    {
        yield return new WaitForSeconds(seconds);
        owner.CmdSyncTargeting(owner, false);
        CmdSyncCaster(null);//캐스터 초기화
        CmdSyncTarget(null);
        
        switch(panel)
        {
            case PanelType.Revive:
                owner.CmdSetActiveRevivePanel(owner, false);
                break;

            case PanelType.PickUp:
                owner.CmdSetActivePickUpPanel(owner, false);
                break;
        }
        
    }
    [Command(requiresAuthority = false)]
    public void CmdSetSelectedCardImage(CardInfo card , PanelType type, bool isActive)
    {
        RpcSetSelectedCardImage(card, type, isActive);
    }
    [ClientRpc]
    public void RpcSetSelectedCardImage(CardInfo card, PanelType type, bool isActive) 
    {
        switch(type)
        {
            case PanelType.Revive:
                for (int i = 0; i < 8; i++)
                {
                    //전부다 끄고
                    reviveUIImage[i].sprite = null;
                    reviveUIImage[i].gameObject.SetActive(false);
                }
                //선택된 카드만 연출
                reviveUIImage[0].sprite = card.image;
                reviveUIImage[0].gameObject.SetActive(true);

                break;

            case PanelType.PickUp:
                for (int i = 0; i < 8; i++)
                {
                    //전부다 끄고
                    pickUpUIImage[i].sprite = null;
                    pickUpUIImage[i].gameObject.SetActive(false);
                }

                if (isActive)
                {
                    //선택된 카드만 연출
                    pickUpUIImage[0].sprite = card.image;
                    pickUpUIImage[0].gameObject.SetActive(true);

                    //뽑은카드 UICardInfo리스트에서 제거
                    caster.GetComponent<FieldCard>().player.CmdRemoveUICardInfo(card);
                }

                if(caster.GetComponent<FieldCard>().player == Player.localPlayer)
                {
                    //권한 있는 플레이어한테만 나머지 덱 넣어주기
                    for (int i = 0; i < caster.GetComponent<FieldCard>().player.UICardInfoList.Count; ++i)
                    {
                        //남은 UICardInfo리스트 덱에 다시 넣어주기
                        caster.GetComponent<FieldCard>().player.CmdAddDeckList(caster.GetComponent<FieldCard>().player.UICardInfoList[i]);
                    }
                }

                break;
        }
        
    }
}
