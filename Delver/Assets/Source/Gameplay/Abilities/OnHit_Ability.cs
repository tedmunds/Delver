using UnityEngine;
using UnityEditor;

public abstract class OnHit_Ability : ScriptableObject
{
    public abstract bool ValidateHit(Actor target, Actor user, AttackCollider currentCollider);

    public abstract void OnHit(Actor target, Actor user, AttackCollider currentCollider);
}