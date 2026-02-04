using UnityEngine;

public class PlayState : GameState
{
    public override void Enter()
    {
        foreach (Vector2Int posTile in GameManager.Of.Grid.GridLevelData.allTilesPosition)
        {
            CellData.SpawnPointType spawn = CellData.SpawnPointType.None;
            GameManager.Of.Grid.GridLevelData.spawnPointsTiles.TryGetValue(posTile, out spawn);

            if (spawn == CellData.SpawnPointType.Player)
            {
                GameManager.Of.CreatePlayer(posTile);
                break;
            }
        }
    }

    public override void LogicsUpdate()
    {
        // Logique du jeu
    }

    public override void PhysicsUpdate()
    {
        // Physique du jeu
    }

    public override void Exit()
    {
        // Sortie du jeu
    }
}