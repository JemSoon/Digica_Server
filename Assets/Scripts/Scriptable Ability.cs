// Put all our cards in the Resources folder
using UnityEngine;
using System.Collections.Generic;

// Struct for cards in your deck. Card + amount (ex : Sinister Strike x3). Used for Deck Building. Probably won't use it, just add amount to Card struct instead.
public partial class ScriptableAbility : ScriptableObject
{
    [Header("Targets")]
    public List<Target> targets = new List<Target>();

    [Header("Damage or Heal")]
    public int damage = 0;
    public int heal = 0;

    [Header("Buffs and Debuffs")]
    public int strength = 0;
    public int health = 0;

    [Header("Draw or Discard")]
    public int draw = 0;
    public int discard = 0;

    [Header("Properties")]
    public bool untilEndOfTurn = false; // Whether the buffs/debuffs last until the end of turn or are permanent. Permanent by default.

    public virtual void Cast(Entity target)
    {

    }

    private void OnValidate()
    {
        // By default, all creatures can only attack enemy creatures and our opponent. We set it here so every card get it's automatically.
        if (targets.Count == 0)
        {
            targets.Add(Target.OPPONENT);
        }
    }
}