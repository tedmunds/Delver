using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SimpleAttack")]
public class Ability_SimpleMelee : Ability
{

    [SerializeField]
    public AttackCollider colliderType;


    [SerializeField]
    public float maxRange = 1.0f;


    protected AttackCollider currentCollider;
    private bool stopMovement;

    public override void AbilityStarted(Actor abilityUser, Vector3 position, Vector3 direction)
    {
        base.AbilityStarted(abilityUser, position, direction);
        
        currentCollider = ObjectPool.GetActive(colliderType, position, Quaternion.Euler(0.0f, 0.0f, Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x)));
        currentCollider.InitFromAttack(abilityUser, OnHit, ValidateHit, direction, colliderSpeed);
        currentCollider.teamNumber = this.abilityUser.teamNumber;
    }

    public override void UpdateAbility(Actor abilityUser)
    {
        base.UpdateAbility(abilityUser);

        if(currentCollider != null)
        {
            if (stopMovement || (currentCollider.transform.position - activatedPosition).sqrMagnitude > (maxRange * maxRange))
            {
                currentCollider.transform.position = activatedPosition + activatedDirection * maxRange;
                RemoveAttackCollider();
            }
        }
    }

    public override void AbilityEnded(Actor abilityUser)
    {
        base.AbilityEnded(abilityUser);

        if(currentCollider != null)
        {
            RemoveAttackCollider();
        }        
    }

    public virtual void OnHit(Actor hit)
    {
        hit.TakeDamage(damage);
    }

    protected virtual bool ValidateHit(Actor hit)
    {
        return hit.teamNumber != currentCollider.teamNumber;
    }

    protected void RemoveAttackCollider()
    {
        if(currentCollider != null)
        {
            currentCollider.gameObject.SetActive(false);
            currentCollider = null;
        }
        
        stopMovement = true;
    }

}