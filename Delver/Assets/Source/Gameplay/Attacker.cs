using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Attacker : MonoBehaviour 
{
    public delegate void AttackFinishedCallback();
    
    [SerializeField]
    private Attack baseAttack;
    
    private Actor owner;

    private float lastAttackStartedTime;
    private AttackFinishedCallback onFinishedCallback;

    private bool isPerformingAttack;

    // Instance of the attack being prformed
    private Attack currentAttack;

    private TileWorldManager world;

    public Attack GetBaseAttack() { return baseAttack; }

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
                currentAttack.UpdateAttack(owner);
            }
        }
	}

    /// <summary>
    /// Initiates the attack, performing its actions
    /// </summary>
    public bool StartAttack(Attack attackToPerform, Vector3 direction, AttackFinishedCallback callback)
    {
        if(!baseAttack || !attackToPerform)
        {
            return false;
        }
        
        currentAttack = ScriptableObject.Instantiate<Attack>(attackToPerform);
        currentAttack.AttackStarted(owner, transform.position, direction);

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

        currentAttack.AttackEnded(owner);
        Destroy(currentAttack);
        currentAttack = null;
    }
}
