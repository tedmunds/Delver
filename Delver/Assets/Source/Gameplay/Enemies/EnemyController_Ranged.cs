using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController_Ranged : EnemyController
{
    
    public EnemyController_Ranged() : base()
    {
        maxMoveTargetErrorDist = 25.0f;
    }


    protected override void Update_Idle()
    {
        base.Update_Idle();

        if(InEngagementRange())
        {
            GotoState(STATE_CHASE);
        }
    }


    protected override void Update_Chase()
    {
        base.Update_Chase();

        if(Time.time - lastAttackEndedTime > attacker.GetBaseAttack().cooldown && InAttackRange())
        {
            GotoState(STATE_ATTACK);
        }
    }

    public override void EndAttack()
    {
        // Override to go into chase instead of idle, after a delay
        TimerManager.SetTimer(ResumeChase, attacker.GetBaseAttack().cooldown);
    }


    public void ResumeChase()
    {
        GotoState(STATE_CHASE);
    }

}