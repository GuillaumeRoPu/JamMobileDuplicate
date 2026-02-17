using UnityEngine;

public class MenuState : GameState
{
    public override void Enter()
    {
        Camera.main.orthographicSize = 5;
        GameManager.Of.ShowMenu(true);
        GameManager.Of.ShowSettings(false);
        GameManager.Of.ShowPause(false);
        GameManager.Of.ShowGame(false);
    }

    public override void LogicsUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        
    }

    public override void Exit()
    {
        
    }
}