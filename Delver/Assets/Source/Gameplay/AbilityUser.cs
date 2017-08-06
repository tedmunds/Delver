using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class AbilityUser : MonoBehaviour 
{
    public delegate void AbilityFinishedCallback();
    
    [SerializeField]
    private Ability baseAbility;
    
    private Actor owner;

    private float lastAttackStartedTime;
    private AbilityFinishedCallback onFinishedCallback;
    private AbilityFinishedCallback resumeMovementCallback;

    private bool isPerformingAttack;
    private bool hasControl;

    // Instance of the attack being prformed
    private Ability currentAttack;

    private TileWorldManager world;

    public Ability GetBaseAttack() { return baseAbility; }

    public void Start()
    {
        world = FindObjectOfType<TileWorldManager>();
        owner = GetComponent<Actor>();
    }

    protected void Update()
	{
		if(isPerformingAttack)
        {
            // Check if it should notify about control being returned
            if(hasControl && Time.time > lastAttackStartedTime + currentAttack.lockMovementTime)
            {
                hasControl = false;

                // allow control back to owner
                if(resumeMovementCallback != null)
                {
                    resumeMovementCallback();
                }
            }

            // Past the ability total lifetime
            if(Time.time > lastAttackStartedTime + currentAttack.totalTime)
            {
                EndAttack();
            }

            if(currentAttack != null)
            {
                currentAttack.UpdateAbility(owner);
            }
        }
	}

    /// <summary>
    /// Initiates the attack, performing its actions
    /// </summary>
    public bool StartAbility(Ability attackToPerform, Vector3 direction, AbilityFinishedCallback resumeMoveCallback, AbilityFinishedCallback completedCallback)
    {
        if(!baseAbility || !attackToPerform)
        {
            return false;
        }
        
        currentAttack = ScriptableObject.Instantiate<Ability>(attackToPerform);
        currentAttack.AbilityStarted(owner, transform.position, direction);

        lastAttackStartedTime = Time.time;
        onFinishedCallback = completedCallback;
        resumeMovementCallback = resumeMoveCallback;
        isPerformingAttack = true;
        hasControl = true;

        return true;
    }


    private void EndAttack()
    {
        if(onFinishedCallback != null)
        {
            onFinishedCallback();
        }

        isPerformingAttack = false;

        currentAttack.AbilityEnded(owner);
        Destroy(currentAttack);
        currentAttack = null;
    }
}
