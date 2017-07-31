using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Like attack colliders except meant to be longer lasting and apply effects to things that are overlaping them
/// </summary>
public class WorldEffectCollider : AttackCollider
{

    private OverlapEvent endOverlapEvent;


    public void InitFromAttack(Actor owner, OverlapEvent startOverlapCallback, OverlapEvent endOverlapCallback, CanHitTarget targetValidation)
    {
        InitFromAttack(owner, startOverlapCallback, targetValidation);
        endOverlapEvent = endOverlapCallback;
    }

    protected override void Update()
    {
        base.Update();

        // check for things ending overlaps: it was hit previously, but is no longer in the hits list
        if(overlapsThisFrame != null && overlapsThisFrame.Length > 0)
        {
            for(int i = hitsThisLifetime.Count - 1; i >= 0; i--)
            {
                Actor hit = hitsThisLifetime[i];

                bool stillOverlappingActor = false;
                for(int j = 0; j < overlapsThisFrame.Length; j++)
                {
                    if(overlapsThisFrame[j].gameObject == hit.gameObject)
                    {
                        // found the actor in this frames overlaps means it has not left the collider yet
                        stillOverlappingActor = true;
                        break;
                    }
                }

                if(!stillOverlappingActor)
                {
                    // callback for removing applied effects etc.
                    endOverlapEvent(hit);                    
                    hitsThisLifetime.RemoveAt(i);
                }
            }
        }        
    }

}