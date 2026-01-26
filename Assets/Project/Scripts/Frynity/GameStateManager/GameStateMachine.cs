using UnityEngine;

public class GameStateMachine
{
    public GameState currentState {get; private set;}

    public void InitState(GameState state)
    {
        currentState = state;
        currentState.Enter();
    }

    public void ChangeState(GameState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
}