using UnityEngine;

public abstract class Update_Ability : ScriptableObject
{
    public abstract void UpdateAbility(Actor user, AttackCollider currentCollider);
}
