using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Abilities/HealAbility")]
public class Ability_HealOther : Ability_SimpleMelee
{
    
    public override void OnHit(Actor hit)
    {
        hit.Heal(damage);

        RemoveAttackCollider();
    }


    protected override bool ValidateHit(Actor hit)
    {
        return hit.teamNumber == abilityUser.teamNumber;
    }
}