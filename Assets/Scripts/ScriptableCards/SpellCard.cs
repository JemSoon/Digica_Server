// Put all our cards in the Resources folder. We use Resources.LoadAll down
// below to load our cards into a cache so we can easily reference them later
using System.Collections.Generic;
using UnityEngine;

// Struct for cards in your deck. Card + amount (ex : Sinister Strike x3). Used for Deck Building. Probably won't use it, just add amount to Card struct instead.
[CreateAssetMenu(menuName = "Card/Spell Card", order = 111)]
public partial class SpellCard : ScriptableCard
{
    [Header("Propeties")]
    public bool targeted = false; // Targeted or random
    public int healthChange = 0; // If it affects a creature's stats (+X for positive changes like healing, -X for negative changes like damage)
    public int strengthChange = 0; // 
    public int cardDraw = 0; // Same as health. +X for positive (drawing cards), -X for negative (discarding)
    public bool untilEndOfTurn = false; // If the changes only purposes until end of turn.

    [Header("Board Prefab")]
    public FieldCard cardPrefab;
}