// Learn more : https://mirror-networking.com/docs/Guides/DataTypes.html#scriptable-objects
using System;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

public enum AbilityType : byte { DAMAGE, HEAL, DRAW, DISCARD, BUFF, DEBUFF }

[Serializable]
public struct CardAbility
{
    public AbilityType abilityType; // Doesn't actually do anything. This is just to help visualize what each ScriptableAbility is doing.
    public List<Target> targets;
    //public ScriptableAbility ability;
}