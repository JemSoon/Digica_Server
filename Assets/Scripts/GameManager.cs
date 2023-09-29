using UnityEngine;
using Mirror;

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

    // isHovering is only set to true on the Client that called the OnCardHover function.
    // We only want the hovering to appear on the enemy's Client, so we must exclude the OnCardHover caller from the Rpc call.
    [HideInInspector] public bool isHovering = false;
    [HideInInspector] public bool isHoveringField = false;
    [HideInInspector] public bool isSpawning = false;

    [HideInInspector] public Entity caster; // Ÿ���� �ַο� �����뵵

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
        FieldCard card = cardObject.GetComponent<FieldCard>();
        if (card == null) return; // Ȥ�ó� ���� �����ڵ�
        card.shine.gameObject.SetActive(true);
        if (isServer) RpcFieldCardHover(cardObject, activateShine, targeting);
    }

    [ClientRpc]
    public void RpcFieldCardHover(GameObject cardObject, bool activateShine, bool targeting)
    {
        if (!isHoveringField)
        {
            if(cardObject.GetComponent<FieldCard>() == null) { return; }
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
            playerField.EndTurnFieldCards();//�� ������ �ʵ� ī���� ����ī�� ����
            Player.localPlayer.deck.CmdEndTurn();
            
            if (Player.localPlayer.isTargeting)
            { 
                caster.DestroyTargetingArrow();
                caster = null;
            }
        }
    }

    [ClientRpc]
    public void RpcSetPass()
    {
        if(Player.localPlayer.firstPlayer && isOurTurn)
        { MemoryChecker.Inst.CmdChangeMemory(-3); }
        
        else if(!Player.localPlayer.firstPlayer && isOurTurn)
        { MemoryChecker.Inst.CmdChangeMemory(3); }

        playerField.EndTurnFieldCards();

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
}
