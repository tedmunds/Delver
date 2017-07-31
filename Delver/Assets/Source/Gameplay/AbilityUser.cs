using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class AbilityUser : MonoBehaviour 
{
    public delegate void AttackFinishedCallback();
    
    [SerializeField]
    private Ability baseAbility;
    
    private Actor owner;

    private float lastAttackStartedTime;
    private AttackFinishedCallback onFinishedCallback;

    private bool isPerformingAttack;

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
            if(Time.time > lastAttackStartedTime + currentAttack.totalTime)
            {
                EndAttack();
            }
            else
            {
                currentAttack.UpdateAbility(owner);
            }
        }
	}

    /// <summary>
    /// Initiates the attack, performing its actions
    /// </summary>
    public bool StartAttack(Ability attackToPerform, Vector3 direction, AttackFinishedCallback callback)
    {
        if(!baseAbility || !attackToPerform)
        {
            return false;
        }
        
        currentAttack = ScriptableObject.Instantiate<Ability>(attackToPerform);
        currentAttack.AbilityStarted(owner, transform.position, direction);

        lastAttackStartedTime = Time.time;
        onFinishedCallback = callback;
        isPerformingAttack = true;

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
