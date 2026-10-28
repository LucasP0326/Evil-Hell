using UnityEngine;

public class StateMachine
{
    public IState CurrentState { get; private set; }

    public void Initialize(IState initialState)
    {
        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void ChangeState(IState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}

