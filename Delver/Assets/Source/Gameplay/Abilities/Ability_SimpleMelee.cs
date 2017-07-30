using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SimpleAttack")]
public class Ability_SimpleMelee : Ability
{

    [SerializeField]
    public AttackCollider colliderType;

    [SerializeField]
    public float colliderSpeed = 3.0f;

    [SerializeField]
    public float maxRange = 1.0f;


    private AttackCollider currentCollider;
    private bool stopMovement;

    public override void AttackStarted(Actor attacker, Vector3 position, Vector3 direction)
    {
        base.AttackStarted(attacker, position, direction);
        
        currentCollider = ObjectPool.GetActive(colliderType, position, Quaternion.Euler(0.0f, 0.0f, Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x)));
        currentCollider.InitFromAttack(attacker, OnHit, ValidateHit);
    }

    public override void UpdateAttack(Actor attacker)
    {
        base.UpdateAttack(attacker);

        if(!stopMovement && (currentCollider.transform.position - attackPosition).sqrMagnitude < (maxRange * maxRange))
        {
            currentCollider.transform.position += attackDirection * colliderSpeed * Time.deltaTime;
        }
        else
        {
            currentCollider.transform.position = attackPosition + attackDirection * maxRange;
            stopMovement = true;
        }
    }

    public override void AttackEnded(Actor attacker)
    {
        base.AttackEnded(attacker);

        currentCollider.gameObject.SetActive(false);
    }

    public void OnHit(Actor hit)
    {
        hit.TakeDamage(damage);
    }

    protected bool ValidateHit(Actor hit)
    {
        return hit.teamNumber != attacker.teamNumber;
    }

}