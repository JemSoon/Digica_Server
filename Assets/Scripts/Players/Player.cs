using System;
using UnityEngine;
using Mirror;

//Useful for UI. Whether the player is, well, a player or an enemy.
public enum PlayerType { PLAYER, ENEMY };

[RequireComponent(typeof(Deck))]
[Serializable]
public class Player : Entity
{
    [Header("Player Info")]
    [SyncVar(hook = nameof(UpdatePlayerName))] public string username; // SyncVar hook to call a command whenever a username changes (like when players load in initially).

    [Header("Portrait")]
    public Sprite portrait; // For the player's icon at the top left of the screen & in the PartyHUD.

    [Header("Deck")]
    public Deck deck;
    public Sprite cardback;
    [SyncVar, HideInInspector] public int tauntCount = 0; // Amount of taunt creatures on your side of the board.

    [Header("Stats")]
    [SyncVar] public int maxMana = 10;
    [SyncVar] public int currentMax = 0;
    [SyncVar] public int _mana = 0;
    [SyncVar] public int mana;
    //{
    //    get { return Mathf.Min(_mana, maxMana); }
    //    set { _mana = Mathf.Clamp(value, 0, maxMana); } // (현재 값,최소 보정값, 최대 보정값)
    //}

    // Quicker access for UI scripts
    [HideInInspector] public static Player localPlayer;
    [HideInInspector] public bool hasEnemy = false; // If we have set an enemy.
    [HideInInspector] public PlayerInfo enemyInfo; // We can't pass a Player class through the Network, but we can pass structs. 
    //We store all our enemy's info in a PlayerInfo struct so we can pass it through the network when needed.
     [HideInInspector] public static GameManager gameManager;
    [SyncVar, HideInInspector] public bool firstPlayer = false; // Is it player 1, player 2, etc.

    //메모리 체커
    //[Header("MemoryChecker")]
    //public RectTransform memoryChecker;

    public override void OnStartLocalPlayer()
    {
        localPlayer = this;

        //Get and update the player's username and stats
         CmdLoadPlayer(PlayerPrefs.GetString("Name"));
        CmdLoadDeck();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        deck.deckList.Callback += deck.OnDeckListChange;
        //deck.hand.Callback += deck.OnHandChange;
        deck.graveyard.Callback += deck.OnGraveyardChange;
    }

    [Command]
    public void CmdLoadPlayer(string user)
    {
        //Update the player's username, which calls a SyncVar hook.
        //Learn more here: https://mirror-networking.com/docs/Guides/Sync/SyncVarHook.html
        username = user;
    }

    // Update the player's username, as well as the box above the player's head where their name is displayed.
    void UpdatePlayerName(string oldUser, string newUser)
    {
        //Update username
        username = newUser;

        //Update game object's name in editor (only useful for debugging).
        gameObject.name = newUser;
    }

    [Command]
    public void CmdLoadDeck()
    {
        //Fill deck from startingDeck array
         for (int i = 0; i < deck.startingDeck.Length; ++i)
        {
            CardAndAmount card = deck.startingDeck[i];
            for (int v = 0; v < card.amount; ++v)
            {
                deck.deckList.Add(card.amount > 0 ? new CardInfo(card.card, 1) : new CardInfo());
                if (deck.hand.Count < 7) deck.hand.Add(new CardInfo(card.card, 1));
            }
        }
        if (deck.hand.Count == 7)
        {
            deck.hand.Shuffle();
        }
    }

    private void Start()
    {
        //memoryChecker = GameObject.Find("MemoryChecker").GetComponent<RectTransform>();
        gameManager = FindObjectOfType<GameManager>();
        health = gameManager.maxHealth;
        maxMana = gameManager.maxMana;
        deck.deckSize = gameManager.deckSize;
        deck.handSize = gameManager.handSize;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        //Get EnemyInfo as soon as another player connects. Only start updating once our Player has been loaded in properly(username will be set if loaded in).
         if (!hasEnemy && username != "")
        {
            UpdateEnemyInfo();
        }

        if (Input.GetKeyDown(KeyCode.G) && isLocalPlayer)
        {
            gameManager.StartGame();
        }
    }

     public void UpdateEnemyInfo()
    {
        //Find all Players and add them to the list.
       Player[] onlinePlayers = FindObjectsOfType<Player>();

        //Loop through all online Players(should just be one other Player)
         foreach (Player players in onlinePlayers)
        {
            //Make sure the players are loaded properly(we load the usernames first)
             if (players.username != "")
            {
                //There should only be one other Player online, so if it's not us then it's the enemy.
                 if (players != this)
                {
                    //Get & Set PlayerInfo from our Enemy's gameObject
                     PlayerInfo currentPlayer = new PlayerInfo(players.gameObject);
                    enemyInfo = currentPlayer;
                    hasEnemy = true;
                    enemyInfo.data.casterType = Target.OPPONENT;
                    //Debug.LogError("Player " + username + " Enemy " + enemy.username + " / " + enemyInfo.username); // Used for Debugging
                }
            }
        }
    }

    //void MemoryChecker()
    //{
    //    switch(mana)
    //    {
    //         case 0:
    //            memoryChecker.anchoredPosition = new Vector2(0, 32);
    //            break;
    //        case 1:
    //            memoryChecker.anchoredPosition = new Vector2(-91, 32);
    //            break;
    //        case 2:
    //            memoryChecker.anchoredPosition = new Vector2(-91 * 2, 32);
    //            break;
    //        case 3:
    //            memoryChecker.anchoredPosition = new Vector2(-91 * 3, 32);
    //            break;
    //        case 4:
    //            memoryChecker.anchoredPosition = new Vector2(-91 * 4, 32);
    //            break;
    //        case 5:
    //            memoryChecker.anchoredPosition = new Vector2(-91 * 5, 32);
    //            break;
    //        case 6:
    //            memoryChecker.anchoredPosition = new Vector2(-91 * 6, 32);
    //            break;
    //        case 7:
    //            memoryChecker.anchoredPosition = new Vector2(-91 * 7, 32);
    //            break;
    //        case 8:
    //            memoryChecker.anchoredPosition = new Vector2(-91 * 8, 32);
    //            break;
    //        case 9:
    //            memoryChecker.anchoredPosition = new Vector2(-91 * 9, 32);
    //            break;
    //        case 10:
    //            memoryChecker.anchoredPosition = new Vector2(-91 * 10, 32);
    //            break;
    //        case -1:
    //            memoryChecker.anchoredPosition = new Vector2(91, 32);
    //            break;
    //        case -2:
    //            memoryChecker.anchoredPosition = new Vector2(91 * 2, 32);
    //            break;
    //        case -3:
    //            memoryChecker.anchoredPosition = new Vector2(91 * 3, 32);
    //            break;
    //        case -4:
    //            memoryChecker.anchoredPosition = new Vector2(91 * 4, 32);
    //            break;
    //        case -5:
    //            memoryChecker.anchoredPosition = new Vector2(91 * 5, 32);
    //            break;
    //        case -6:
    //            memoryChecker.anchoredPosition = new Vector2(91 * 6, 32);
    //            break;
    //        case -7:
    //            memoryChecker.anchoredPosition = new Vector2(91 * 7, 32);
    //            break;
    //        case -8:
    //            memoryChecker.anchoredPosition = new Vector2(91 * 8, 32);
    //            break;
    //        case -9:
    //            memoryChecker.anchoredPosition = new Vector2(91 * 9, 32);
    //            break;
    //        case -10:
    //            memoryChecker.anchoredPosition = new Vector2(91 * 10, 32);
    //            break;
    //        default:
    //            memoryChecker.anchoredPosition = Vector2.zero;
    //            break;
    //    }
        
    //}

    public bool IsOurTurn() => gameManager.isOurTurn;
}