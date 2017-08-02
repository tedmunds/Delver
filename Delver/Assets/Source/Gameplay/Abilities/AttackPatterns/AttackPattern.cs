using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple non instanced object that can be used with special attacks to swap in different patterns
/// </summary>
public abstract class AttackPattern : ScriptableObject
{
    /// <summary>
    /// Returns a list of where to place new colliders for this attack
    /// </summary>
    public abstract Vector3[] GetColliderSpawnLocations(Ability user, Vector3 colliderBounds, int layer);

}