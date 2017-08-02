using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AbilityUser))]
[RequireComponent(typeof(TileMover))]
public class PlayerController : ControlStateMachine
{
    private const string STATE_WALKING      = "Walking";
    private const string STATE_ROLLING      = "Rolling";
    private const string STATE_KNOCKDOWN    = "Knockdown";
    private const string STATE_ATTACK       = "Attack";

    private struct PendingInput
    {
        public string stateChange;
        public float inputTime;
        public Vector3 controlDirection;

        public bool isPending;
    }

    private struct ControlDirectionOverride
    {
        public bool pending;
        public Vector3 controlOverride;
    }


    #region input_bindings
    [SerializeField]
    private string InputAxis_Vertical = "Vertical";

    [SerializeField]
    private string InputAxis_Horizontal = "Horizontal";

    [SerializeField]
    private string InputAction_Roll = "Jump";

    [SerializeField]
    private string InputAction_Attack = "Fire1";

    [SerializeField]
    private float maxInputQueueTime = 0.5f;
    #endregion

    #region move_params
    [SerializeField]
    private float walkSpeed = 3.0f;
    #endregion

    #region roll_params
    [SerializeField]
    private int rollDistance = 3;

    [SerializeField]
    private float rollSpeed = 10.0f;

    [SerializeField]
    private float minRollCooldown = 0.5f;
    #endregion

    #region energy_params
    [SerializeField]
    private float energyPerRoll = 10.0f;

    [SerializeField]
    private float baseEnergy = 100.0f;

    [SerializeField]
    private float energyRecoveryRate = 50.0f;

    [SerializeField]
    private float energyRecoveryDelay = 1.0f;
    #endregion
    

    private Actor actor;
    private Animator animator;
    private AbilityUser attacker;
    private TileMover characterMover;

    // Pending input is an input made during a state that doesnt allow that change to happen until the state exits
    private PendingInput pendingInput;
    private ControlDirectionOverride controlDirectionOverride;

    // Contains sets of modifiers for various stats on the player. Modifiers are uniquely tagged to be removed individually or automatically over time
    private ModifierContainer modifierContainer;

    // Traks the last non-zero input direction
    Vector2 lastInputDirection;

    // move state variables
    private float lastRollTime;

    private float attackEndedTime;

    private float currentEnergy;
    private float lastEnergyConsumptionTime;

    private bool bDisableAttack;
    
    // Get / Set
    public float GetEnergyPercent() { return currentEnergy / baseEnergy; }
    public float GetCurrentEnergy() { return currentEnergy; }
    public float GetBaseEnergy() { return baseEnergy; }


    protected void Start()
    {
        modifierContainer = new ModifierContainer();

        actor = GetComponent<Actor>();
        animator = GetComponent<Animator>();
        attacker = GetComponent<AbilityUser>();
        characterMover = GetComponent<TileMover>();
    }

    protected void OnEnable()
    {
        currentEnergy = baseEnergy;
        pendingInput.isPending = false;

        GotoState(STATE_WALKING, false);
    }


    protected override void GlobalUpdate()
    {
        // start energy recovery
        if(GetEnergyPercent() < 1.0f && CanDoEnergyRecovery())
        {
            currentEnergy = Mathf.Min(baseEnergy, currentEnergy + energyRecoveryRate * Time.deltaTime);
        }

        // Handle attack input, from any state assuming they dont cancel the ability
        if(Input.GetButtonDown(InputAction_Attack))
        {
            if(!bDisableAttack)
            {
                GotoState(STATE_ATTACK, false);
            }
            else
            {
                MakePendingInput(STATE_ATTACK);
            }
        }

        // Just keep tracking the last non-zero input
        Vector2 inputValue = PeekControlInput();
        if(inputValue.sqrMagnitude > 0.0f)
        {
            lastInputDirection = inputValue;
        }

        modifierContainer.UpdateModifiers(Time.time);

        //animator.SetFloat("Speed", currentSpeed);
    }

    // ========================================================================================================
    #region control_states

    protected void Update_Walking()
    {
        Vector3 inputVector = PeekControlInput();

        // try to do a roll
        if(CanDoEvadeRoll(inputVector) && Input.GetButtonDown(InputAction_Roll))
        {
            GotoState(STATE_ROLLING, false);
        }
        // Otherwise normal walk inputs
        else if(inputVector.sqrMagnitude > 0.0f)
        {
            characterMover.Move(inputVector, walkSpeed * inputVector.magnitude);
        }
    }


    public void OnEnter_Rolling()
    {
        // consume the energy now, as the amount has been verified
        ConsumeEnergy(energyPerRoll);
        
        // how long it will take to cover the distance at this speed, used to set timer to end roll state
        float rollTime = (rollDistance * characterMover.GetWorld().GetTileSize()) / rollSpeed;

        // Set the movement velocity for the duration of the state
        Vector3 direction = ConsumeControlInput();
        direction = characterMover.GetDominantDirection(direction);
        characterMover.Move(direction * rollDistance, rollSpeed, true);
        
        TimerManager.SetTimer(EndRollTimer, rollTime);

        // Dont take damage while rolling
        actor.SetInvincible(true);
        bDisableAttack = true;

        //animator.SetTrigger("StartRoll");
    }

    // Called on timer to stop roll state
    public void EndRollTimer()
    {
        GotoState(STATE_WALKING, false);
    }

    public void OnExit_Rolling()
    {
        lastRollTime = Time.time;

        actor.SetInvincible(false);
        bDisableAttack = false;
        characterMover.Move(Vector2.zero, 0.0f);
        //animator.SetTrigger("EndRoll");
    }

    
    public void OnEnter_Knockdown()
    {
        // cancels all movement
        characterMover.Move(Vector3.zero, 0.0f, true);
        bDisableAttack = true;
    }

    public void OnExit_Knockdown()
    {
        bDisableAttack = false;
    }


    public void OnEnter_Attack()
    {
        //characterMover.Move(Vector3.zero, 0.0f, true);
        bDisableAttack = true;

        // Starts the attack sequence that handles damage etc.
        Ability attacktoPerform = attacker.GetBaseAttack();
        // Attack in cardinal directions inly, use the last known direction, in case an input is not currently pressed
        Vector2 attackDirection = characterMover.GetDominantDirection(lastInputDirection);
        
        // Returns true only if the attack was valid
        bool bPerformedAttack = attacker.StartAbility(attacktoPerform, attackDirection, AttackFinished, null);
        if(bPerformedAttack)
        {
            // each attack can use up some energy too
            ConsumeEnergy(attacktoPerform.energyConsumption);
        }
        else
        {
            // just go back to walking
            GotoState(STATE_WALKING);
        }
        
        //animator.SetTrigger("DoAttack");
    }

    protected void Update_Attack()
    {
        if(CanDoEvadeRoll(PeekControlInput()) && Input.GetButtonDown(InputAction_Roll))
        {
            MakePendingInput(STATE_ROLLING);
        }
    }

    protected void AttackFinished()
    {
        // Check for pending states at the end of an attack, like another attack or a roll. Otherwise just go back to walking
        if(!CheckPendingInput())
        {
            GotoState(STATE_WALKING);
        }
    }

    public void OnExit_Attack()
    {
        attackEndedTime = Time.time;
        bDisableAttack = false;
    }


    #endregion
    // ========================================================================================================

    public bool CanAttack()
    {
        return !bDisableAttack;
    }

    public void GetUpFromKnockdown()
    {
        GotoState(STATE_WALKING);
    }

    // Gets the control input, or consumes the pending control override if there is one
    public Vector3 ConsumeControlInput()
    {
        if(controlDirectionOverride.pending)
        {
            controlDirectionOverride.pending = false;
            return controlDirectionOverride.controlOverride;
        }

        return PeekControlInput();
    }

    // checks the control input in a const way
    public Vector3 PeekControlInput()
    {
        float verticalInput = Input.GetAxis(InputAxis_Vertical);
        float horizontalInput = Input.GetAxis(InputAxis_Horizontal);

        Vector3 inputVector = new Vector3(horizontalInput, verticalInput, 0.0f);
        if(inputVector.sqrMagnitude > 1.0f)
        {
            inputVector.Normalize();
        }

        return inputVector;
    }


    public void ConsumeEnergy(float consumeAmount)
    {
        currentEnergy = Mathf.Max(currentEnergy - consumeAmount, 0.0f);
        lastEnergyConsumptionTime = Time.time;
    }

    public bool CanDoEnergyRecovery()
    {
        return (Time.time - lastEnergyConsumptionTime > energyRecoveryDelay);
    }


    // Check if play can roll right now, given energy and also some movement cooldown etc
    protected bool CanDoEvadeRoll(Vector3 inputVector)
    {
        if(inputVector.sqrMagnitude <= 0.0f)
        {
            return false;
        }

        if(Time.time - lastRollTime < minRollCooldown)
        {
            return false;
        }

        if(currentEnergy <= 0.0f)
        {
            return false;
        }

        return true;
    }

    // Sets up an input event to be consumed when entering a base state
    protected void MakePendingInput(string pendingState)
    {
        pendingInput.inputTime = Time.time;
        pendingInput.stateChange = pendingState;
        pendingInput.controlDirection = ConsumeControlInput();

        pendingInput.isPending = true;
    }

    // Checks if a pending input was waitng, and consumes it if so, returning true. Returns false otherwise
    protected bool CheckPendingInput()
    {
        bool consumedInput = false;
        if(pendingInput.isPending && Time.time - pendingInput.inputTime < maxInputQueueTime)
        {
            // The next time control input is requested, this override will be used
            controlDirectionOverride.pending = true;
            controlDirectionOverride.controlOverride = pendingInput.controlDirection;

            GotoState(pendingInput.stateChange);
            consumedInput = true;
        }

        pendingInput.isPending = false;
        return consumedInput;
    }


}
