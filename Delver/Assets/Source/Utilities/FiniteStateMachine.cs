using System.Collections.Generic;

// Stack based functional finite state machine
public class FiniteStateMachine {

    public delegate void ActiveState();

    public bool bIsActive;
    private Stack<ActiveState> stateStack;

    public FiniteStateMachine(ActiveState initialState = null, bool bStartActive = true) {
        stateStack = new Stack<ActiveState>();
        if(initialState != null) {
            stateStack.Push(initialState);
        }

        bIsActive = bStartActive;
    }

    public void PushState(ActiveState newState) {
        stateStack.Push(newState);
    }

    public void PopState() {
        stateStack.Pop();
    }

    public void ClearStack() {
        stateStack.Clear();
    }

    public void Update() {
        ActiveState currentState = GetCurrentState();

        if(bIsActive && currentState != null) {
            currentState();
        }
    }

    private ActiveState GetCurrentState() {
        return (stateStack.Count > 0) ? stateStack.Peek() : null;
    }
}
