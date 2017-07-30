using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Attacks/HealAbility")]
public class Attack_HealOther : Attack
{

    [SerializeField]
    public AttackCollider colliderType;

    [SerializeField]
    public float colliderSpeed = 3.0f;

    [SerializeField]
    public float maxRange = 1.0f;

    private AttackCollider currentCollider;


    public override void AttackStarted(Actor attacker, Vector3 position, Vector3 direction)
    {
        base.AttackStarted(attacker, position, direction);
        
        currentCollider = ObjectPool.GetActive(colliderType, position, Quaternion.Euler(0.0f, 0.0f, Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x)));
        currentCollider.InitFromAttack(attacker, OnHit, ValidateHit);
    }


    public override void UpdateAttack(Actor attacker)
    {
        base.UpdateAttack(attacker);

        if(currentCollider != null)
        {
            if((currentCollider.transform.position - attackPosition).sqrMagnitude < (maxRange * maxRange))
            {
                currentCollider.transform.position += attackDirection * colliderSpeed * Time.deltaTime;
            }
            else
            {
                currentCollider.transform.position = attackPosition + attackDirection * maxRange;
                currentCollider.gameObject.SetActive(false);
                currentCollider = null;
            }
        }        
    }


    public override void AttackEnded(Actor attacker)
    {
        base.AttackEnded(attacker);

        if(currentCollider != null)
        {
            currentCollider.gameObject.SetActive(false);
        }        
    }


    public void OnHit(Actor hit)
    {
        hit.Heal(damage);
        currentCollider.gameObject.SetActive(false);
        currentCollider = null;
    }


    protected bool ValidateHit(Actor hit)
    {
        return hit.teamNumber == attacker.teamNumber;
    }
}