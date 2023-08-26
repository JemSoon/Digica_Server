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
    [HideInInspector] public bool isDraw = false;//test
    [SyncVar, HideInInspector] public int turnCount = 1; // Start at 1
    [SyncVar] public bool isGameStart;

    // isHovering is only set to true on the Client that called the OnCardHover function.
    // We only want the hovering to appear on the enemy's Client, so we must exclude the OnCardHover caller from the Rpc call.
    [HideInInspector] public bool isHovering = false;
    [HideInInspector] public bool isHoveringField = false;
    [HideInInspector] public bool isSpawning = false;

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
        FieldCard card = cardObject.GetComponent<FieldCard>();
        if (card == null) return; // 혹시나 싶은 안전코드
        card.shine.gameObject.SetActive(true);
        if (isServer) RpcFieldCardHover(cardObject, activateShine, targeting);
    }

    [ClientRpc]
    public void RpcFieldCardHover(GameObject cardObject, bool activateShine, bool targeting)
    {
        if (!isHoveringField)
        {
            FieldCard card = cardObject.GetComponent<FieldCard>();
            if(card==null) return; // 혹시나 싶은 안전코드
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
