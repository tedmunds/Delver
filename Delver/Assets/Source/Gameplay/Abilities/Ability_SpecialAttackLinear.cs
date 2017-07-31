using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SpecialAttack_Linear")]
public class Ability_SpecialAttackLinear : Ability_SpecialAttack
{

    protected override Vector3[] GetColliderSpawnLocations(Vector3 colliderBounds, int layer)
    {
        Vector3 origin = activatedPosition;
        Vector3 direction = activatedDirection;

        float delta = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)? colliderBounds.x : colliderBounds.y;

        // Spawns the layers out in a line in the direction of the attack
        Vector3 nextPos = origin + (direction * (delta * (layer + 1)));

        return new Vector3[] { nextPos };
    }
}