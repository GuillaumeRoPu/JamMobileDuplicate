using UnityEngine;

public class MenuState : GameState
{
    public override void Enter()
    {
        GameManager.Of.ShowGame(false);
        GameManager.Of.ShowMenu(true);
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