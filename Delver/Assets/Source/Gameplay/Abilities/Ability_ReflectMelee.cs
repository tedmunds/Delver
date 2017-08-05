using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/ReflectAttack")]
public class Ability_ReflectMelee : Ability_SimpleMelee
{
    public override void OnHit(Actor hit)
    {
        hit.Reflect(this.abilityUser);

        RemoveAttackCollider();
    }

    protected override bool ValidateHit(Actor hit)
    {
        return hit.teamNumber != currentCollider.teamNumber;
    }

}
