using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/Damage & Heal", order = 111)]
public class DamgeAbility : ScriptableAbility
{
    public override void Cast(Entity target)
    {
        base.Cast(target);
    }
}
