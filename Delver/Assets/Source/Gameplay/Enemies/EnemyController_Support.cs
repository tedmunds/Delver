using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController_Support : EnemyController
{
    protected const string STATE_HEALING = "Healing";

    [SerializeField]
    protected Ability healingAbility;

    protected float lastHealTime;

    protected List<Actor> healTargets = new List<Actor>();

    protected void OnEnable()
    {
        healTargets.Clear();

        // Grab other team members
        foreach(GameObject entity in TileWorldManager.instance.AllEntities(ignore: gameObject))
        {
            Actor healTarget = entity.GetComponent<Actor>();
            if(healTarget != null && healTarget.teamNumber > TileWorldManager.TEAM_NUMBER_PLAYER)
            {
                healTargets.Add(healTarget);
            }
        }
    }


    protected override void GlobalUpdate()
    {
        base.GlobalUpdate();
        
    }

    protected override void Update_Idle()
    {
        base.Update_Idle();

        // If there is an enemy with health below the threshold
        Actor bestHealTarget = GetBestHealTarget();
        if(bestHealTarget != null && CanPerformHeal())
        {
            GotoState(STATE_HEALING);
        }
        else if(InAttackRange())
        {
            GotoState(STATE_ATTACK);
        }
    }


    protected void OnEnter_Healing()
    {
        StopMovement();

        Actor chosenTarget = GetBestHealTarget();

        Vector3 toTarget = (chosenTarget.transform.position - transform.position).normalized;

        abilities.StartAbility(healingAbility, toTarget, HealAbilityDone, null);

        animator.SetTrigger("Attack");
    }



    protected void HealAbilityDone()
    {
        GotoState(STATE_IDLE);
    }

    protected void OnExit_Healing()
    {
        lastHealTime = Time.time;
        suspendMove = false;
    }


    protected bool CanPerformHeal()
    {
        return (Time.time - lastHealTime > healingAbility.cooldown);
    }

    
    protected Actor GetBestHealTarget()
    {
        Actor bestActor = null;
        float lowestHealth = 1.0f;
        foreach(Actor target in healTargets)
        {
            float healthPct = (target.GetCurrentHealth() / target.GetBaseHealth());
            if(!target.IsDead() && healthPct < lowestHealth)
            {
                bestActor = target;
                lowestHealth = healthPct;
            }
        }

        return bestActor;
    }

}