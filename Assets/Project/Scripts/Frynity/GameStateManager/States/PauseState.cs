using UnityEngine;

public class PauseState : GameState
{
    public override void Enter()
    {
        GameManager.Of.ShowPause(true);
        Time.timeScale = 0;
    }

    public override void LogicsUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        
    }

    public override void Exit()
    {
        GameManager.Of.ShowSettings(false);
        Time.timeScale = 1;
    }
}