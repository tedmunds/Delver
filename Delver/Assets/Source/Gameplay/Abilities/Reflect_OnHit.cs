using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "OnHit_Abilities/Reflect")]
public class Reflect_OnHit : OnHit_Ability
{
    public override bool ValidateHit(Actor target, Actor user, AttackCollider currentCollider)
    {
        return target.teamNumber != currentCollider.teamNumber;
    }

    public override void OnHit(Actor target, Actor user, AttackCollider currentCollider)
    {
        target.Reflect(user);
    }
}