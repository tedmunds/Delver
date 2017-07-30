using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// A base class for any state based controllers. It is very handy, since it will automatically register 
/// any methods that take the form of <Method Prefix>_<State Name>, like Update_Walking(). Then you can 
/// simply call GotoState(<State Name>) and the magic all hapens behind the scenes. 
/// 
/// NOTE: Initialization all happens in Awake(), so make sure you do not override it without calling base.Awake().
/// NOTE: Do not implement Update(). Instead utilize the virtual GlobalUpdate or GlobalLateUpdate.
/// </summary>

public abstract class ControlStateMachine : MonoBehaviour
{
    // Just assume that controller will have at most 5 states most of the time
    private const int DEFAULT_MAP_SIZE = 5;

    // List of function keys to look for in the class to set up as states
    private static string[] statesMethods =
        {
        "Update",
        "LateUpdate",
        "OnEnter",
        "OnExit"
    };

    // Represents a single control state, which when active, will update every frame
    private class ControlState
    {
        // loadable state updates
        public Action update;
        public Action lateUpdate;

        // Callbacks that, if set, are called on enter / exiting state
        public Action onEnterState;
        public Action onExitState;

        public float enteredStateTime;
    }

    // current state, can be unititialzed
    private ControlState currentState;
    private bool wantsTransition;
    private string transitionTo;

    // Maps states to a name that can be used to lookup very quickly
    private Dictionary<string, ControlState> stateMap = new Dictionary<string, ControlState>(DEFAULT_MAP_SIZE);

    public ControlStateMachine()
    {
        // Look for all functions matching this identifier, with a common state name
        foreach(string methodRoot in statesMethods)
        {
            // Get only private methods
            var methods = GetType().GetMethods(System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.InvokeMethod);

            foreach(var method in methods)
            {
                string[] subnames = method.Name.Split(new[] { '_' });
                if(subnames.Length < 2)
                {
                    continue;
                }

                // This prefix must be the root title, so make sure it matches what we're looking for
                string currentRoot = subnames[0];
                // The suffix is the state name, which we will look for in the map, or otherwise create and insert
                string stateName = subnames[1];

                if(currentRoot != methodRoot)
                {
                    continue;
                }

                // Converts the method data into a usable / assignable delegate 
                Action methodDelegate = (Action)Delegate.CreateDelegate(typeof(Action), this, method, false);

                ControlState state;
                if(stateMap.TryGetValue(stateName, out state))
                {
                    AssignStateMethod(methodRoot, state, methodDelegate);
                }
                else
                {
                    // Create the state and add it to the map
                    state = new ControlState();
                    state.enteredStateTime = -1.0f;
                    state.update = null;
                    state.lateUpdate = null;
                    state.onEnterState = null;
                    state.onExitState = null;

                    AssignStateMethod(methodRoot, state, methodDelegate);
                    stateMap.Add(stateName, state);
                }
            }
        }
    }


    /// <summary>
    /// Will go into the input state at the end of the next late update, unless you force it to not delay
    /// </summary>
    public void GotoState(string state, bool delay = true)
    {
        if(delay)
        {
            wantsTransition = true;
            transitionTo = state;
        }
        else
        {
            HandleStateTransition(state);
        }
    }

    /// <summary>
    /// Returns the time that the machine has been in the current state
    /// </summary>
    public float TimeInState()
    {
        return Time.time - currentState.enteredStateTime;
    }

    private void AssignStateMethod(string methodRoot, ControlState state, Action method)
    {
        switch(methodRoot)
        {
            case "Update":
                state.update = method;
                return;
            case "LateUpdate":
                state.lateUpdate = method;
                return;
            case "OnEnter":
                state.onEnterState = method;
                return;
            case "OnExit":
                state.onExitState = method;
                return;
        }
    }


    // Handles any state transitions based on the reponse from the update method. Returns true if a transition occured
    private bool HandleStateTransition(string nextState)
    {
        ControlState previousState = currentState;

        if(stateMap.TryGetValue(nextState, out currentState))
        {
            if(previousState != null && previousState.onExitState != null) previousState.onExitState();

            if(currentState != null && currentState.onEnterState != null) currentState.onEnterState();

            currentState.enteredStateTime = Time.time;

            return true;
        }

        return false;
    }


    private void Update()
    {
        GlobalUpdate();

        if(currentState.update != null)
        {
            currentState.update();
        }
    }


    private void LateUpdate()
    {
        GlobalLateUpdate();

        if(currentState.lateUpdate != null)
        {
            // Note: late update does not transition states
            currentState.lateUpdate();
        }

        // Finally, perform transition
        if(wantsTransition)
        {
            HandleStateTransition(transitionTo);
            wantsTransition = false;
            transitionTo = "";
        }
    }

    // Overridable base updates
    protected virtual void GlobalUpdate() { }
    protected virtual void GlobalLateUpdate() { }
}
