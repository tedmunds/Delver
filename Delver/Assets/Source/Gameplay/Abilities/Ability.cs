using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ability")]
public class Ability : ScriptableObject
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

    protected Actor abilityUser;

    protected Vector3 activatedPosition;
    protected Vector3 activatedDirection;

    protected float activatedTime;

    public virtual void AbilityStarted(Actor abilityUser, Vector3 position, Vector3 direction)
    {
        this.abilityUser = abilityUser;
        activatedPosition = position;
        activatedDirection = direction.normalized;

        activatedTime = Time.time;
    }

    public virtual void UpdateAbility(Actor abilityUser)
    {

    }

    public virtual void AbilityEnded(Actor abilityUser)
    {

    }

}