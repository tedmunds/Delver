using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the modes that enemies use to approach combat
/// </summary>
public enum EnemyCombatType
{
    /// <summary>
    /// Chases down the player until they are in attack range
    /// </summary>
    Pursuer,    

    /// <summary>
    /// Gets into an attack position and stays there attacking from a distance
    /// </summary>
    Ranged,

    /// <summary>
    /// Tries to stay behind other enemies 
    /// </summary>
    Supporter,
}


[RequireComponent(typeof(CharacterMover))]
[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(Attacker))]
[RequireComponent(typeof(Animator))]
public class EnemyController : ControlStateMachine
{
    protected const string STATE_IDLE = "Idle";
    protected const string STATE_CHASE = "Chase";
    protected const string STATE_TELEGRAPH = "Telegraph";
    protected const string STATE_ATTACK = "Attack";


    [SerializeField]
    protected EnemyCombatType combatType;

    [SerializeField]
    protected EnemyData dataAsset;

    public EnemyCombatType GetCombatType() { return combatType; }

    // Cached components
    protected CharacterMover characterMover;
    protected Actor actor;
    protected Attacker attacker;
    protected Animator animator;

    protected Actor playerTarget;
    protected TileWorldManager world;

    protected bool currentMoveTargetValid;
    protected Vector3 moveToTarget;
    protected bool suspendMove;

    protected float lastAttackEndedTime;

    // How far can the player be from cached moved location before repath needs to happens
    protected float maxMoveTargetErrorDist = 3.0f;

    protected void Start()
	{
        world = FindObjectOfType<TileWorldManager>();

        characterMover = GetComponent<CharacterMover>();
        actor = GetComponent<Actor>();
        attacker = GetComponent<Attacker>();
        animator = GetComponent<Animator>();

        // Dont need this crap, we know some enemies wont have anims implemented yet, but we want unified anim system
        animator.logWarnings = false;

        GotoState(STATE_IDLE, false);
	}

	protected override void GlobalUpdate()
	{
		if(playerTarget == null)
        {
            playerTarget = FindPlayerTarget();
        }

        // Move towards the target tile postion
        if(currentMoveTargetValid && !suspendMove)
        {
            Vector3 toTarget = moveToTarget - transform.position;
            const float acceptanceRadius = 0.01f;

            // check that its not going to move past the target
            Vector3 resolvedLocation = transform.position + toTarget.normalized * characterMover.GetMoveSpeed() * Time.deltaTime;
            if(Vector3.Dot((toTarget).normalized, (moveToTarget - resolvedLocation).normalized) < 0.0f)
            {
                resolvedLocation = moveToTarget;
                transform.position = resolvedLocation;
            }

            if(toTarget.sqrMagnitude <= (acceptanceRadius * acceptanceRadius))
            {
                currentMoveTargetValid = false;
            }
            else
            {
                characterMover.SetControlInput(toTarget.normalized);
            }
        }

        // Update sprite facing
        Vector3 currentVelocity = characterMover.GetVelocity();
        Vector3 currentDirection = currentVelocity.sqrMagnitude > 0.0f? currentVelocity : GetAttackDirection();
        if(Mathf.Abs(currentDirection.x) > 0.0f)
        {
            actor.SetFacing(currentDirection.x > 0.0f ? Actor.Facing.Right : Actor.Facing.Left);
        }
    }

    // ==========================================================================================
    #region control_states 

    protected virtual void Update_Idle()
    {

    }
    
    protected virtual void OnEnter_Chase()
    {
        Vector2 targetTile = world.GetTilePosition(playerTarget.transform.position);
        SetMoveToTarget(targetTile);
    }

    protected virtual void Update_Chase()
    {        
        if(!currentMoveTargetValid || (moveToTarget - playerTarget.transform.position).sqrMagnitude > (maxMoveTargetErrorDist * maxMoveTargetErrorDist))
        {
            Vector2 targetTile = GetAttackPosition(playerTarget.gameObject);
            SetMoveToTarget(targetTile);
        }

        if(InAttackRange())
        {
            GotoState(STATE_TELEGRAPH);
        }
    }

    protected virtual void OnEnter_Attack()
    {
        StopMovement();

        attacker.StartAttack(attacker.GetBaseAttack(), GetAttackDirection(), EndAttack);

        animator.SetTrigger("Attack");
    }

    public virtual void EndAttack()
    {
        TimerManager.SetTimer(OnAttackCooldownEnd, attacker.GetBaseAttack().cooldown);
    }

    protected virtual void OnAttackCooldownEnd()
    {
        GotoState(STATE_IDLE);
    }

    protected virtual void OnExit_Attack()
    {
        lastAttackEndedTime = Time.time;
        suspendMove = false;
    }

    public void OnEnter_Telegraph()
    {
        StopMovement();
        animator.SetTrigger("Telegraph");
        TimerManager.SetTimer(StartAttack, dataAsset.telegraphTime);
    }    

    public void StartAttack()
    {
        GotoState(STATE_ATTACK);
    }


    #endregion
    // ==========================================================================================


    public Actor FindPlayerTarget()
    {
        if(world == null)
        {
            return null;
        }

        return world.GetLocalPlayer();
    }

    /// <summary>
    /// Returns true if this enemy should engage the target
    /// </summary>
    public bool InEngagementRange()
    {
        if(playerTarget == null)
        {
            return false;
        }

        if((playerTarget.transform.position - transform.position).sqrMagnitude < dataAsset.engageRange)
        {
            return true;
        }

        return false;
    }


    public bool InAttackRange()
    {
        if(playerTarget == null)
        {
            return false;
        }

        if((playerTarget.transform.position - transform.position).sqrMagnitude < attacker.GetBaseAttack().idealRange)
        {
            return true;
        }

        return false;
    }


    /// <summary>
    /// Gts the location this enemy wants to be in to attack the target
    /// </summary>
    public virtual Vector2 GetAttackPosition(GameObject target)
    {
        return world.GetTilePosition(target.transform.position);
    }


    /// <summary>
    /// The enemy will try to move to the target tile at whatever its current speed is
    /// </summary>    
    public void SetMoveToTarget(Vector2 targetTile)
    {
        currentMoveTargetValid = true;
        moveToTarget = world.GetTileCenter(targetTile);
    }

    /// <summary>
    /// Stops current move and optionally suspends it until otherwise resumed
    /// </summary>
    public void StopMovement(bool suspendFutureMove = true)
    {
        suspendMove = suspendFutureMove;
        characterMover.SetControlInput(Vector3.zero);
    }

    /// <summary>
    /// Returns the direction to perform an attack right now
    /// </summary>
    public virtual Vector3 GetAttackDirection()
    {
        return (playerTarget.transform.position - transform.position).normalized;
    }

}