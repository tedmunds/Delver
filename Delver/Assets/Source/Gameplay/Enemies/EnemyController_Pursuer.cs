using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController_Pursuer : EnemyController
{
    


    protected override void Update_Idle()
    {
        base.Update_Idle();

        if(InEngagementRange())
        {
            GotoState(STATE_CHASE);
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