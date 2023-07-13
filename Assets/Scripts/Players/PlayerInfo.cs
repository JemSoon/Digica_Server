using System;
using UnityEngine;
using Mirror;

[Serializable]
public partial struct PlayerInfo
{
    public GameObject player;

    public PlayerInfo(GameObject player)
    {
        this.player = player;
    }

    public Player data
    {
        get
        {
            // Return ScriptableItem from our cached list, based on the card's uniqueID.
            return player.GetComponentInChildren<Player>();
        }
    }

    // Player's username
    public string username => data.username;
    public Sprite portrait => data.portrait;

    // Player health and mana
    public int health => data.health;
    public int mana => data.mana;

    // Cardback image
    public Sprite cardback => data.cardback;

    // Card count for UI
    public int handCount => data.deck.hand.Count;
    public int deckCount => data.deck.deckList.Count;
    public int graveCount => data.deck.graveyard.Count;
}

// Card List
public class SyncListPlayerInfo : SyncList<PlayerInfo> { }
