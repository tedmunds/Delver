using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AttackPatterns/AttackaPattern_Linear")]
public class AttackPattern_Linear : AttackPattern
{
    public override Vector3[] GetColliderSpawnLocations(Ability user, Vector3 colliderBounds, int layer)
    {
        Vector3 origin = user.activatedPosition;
        Vector3 direction = user.activatedDirection;

        float delta = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? colliderBounds.x : colliderBounds.y;

        // Spawns the layers out in a line in the direction of the attack
        Vector3 nextPos = origin + (direction * (delta * (layer + 1)));

        return new Vector3[] { nextPos };
    }
}