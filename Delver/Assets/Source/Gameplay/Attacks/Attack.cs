using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Attack")]
public class Attack : ScriptableObject
{
    // The total time the attack takes over control to perform its actions
    [SerializeField]
    public float totalTime;

    // How long until the attack can be used again
    [SerializeField]
    public float cooldown;

    // Default damamge the attack will do on hits
    [SerializeField]
    public float damage;

    // Energy performing the attack will consume
    [SerializeField]
    public float energyConsumption;

    // The minimum range this attack will work in
    [SerializeField]
    public float idealRange;

    protected Actor attacker;

    protected Vector3 attackPosition;
    protected Vector3 attackDirection;

    public virtual void AttackStarted(Actor attacker, Vector3 position, Vector3 direction)
    {
        this.attacker = attacker;
        attackPosition = position;
        attackDirection = direction.normalized;
    }

    public virtual void UpdateAttack(Actor attacker)
    {

    }

    public virtual void AttackEnded(Actor attacker)
    {

    }

}