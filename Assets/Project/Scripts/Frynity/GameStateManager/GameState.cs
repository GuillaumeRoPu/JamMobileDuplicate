using UnityEngine;

public class GameState
{
    public virtual void Enter() { }
    public virtual void PhysicsUpdate() { }
    public virtual void LogicsUpdate() { }
    public virtual void Exit() { }
}
