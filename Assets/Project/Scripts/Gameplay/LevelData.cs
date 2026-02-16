using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Granny/Level Data")]
public class LevelData : ScriptableObject
{
    [field: SerializeField] public GridLevelData GridLevel { get; private set; }
    [field: SerializeField, MinValue(1)] public int TurnMax { get; private set; }
    [field: SerializeField] public TurnEvent[] Events { get; private set; }

    Dictionary<int, Vector2Int> _spawnTiles;
    public Vector2Int GetNextSpawn(int turn)
    {
        return _spawnTiles[turn];
    }

    public void Init()
    {
        _spawnTiles = new Dictionary<int, Vector2Int>();
        foreach (var evnt in Events)
        {
            if (evnt.EventType != EventType.Zombie && evnt.EventType != EventType.Witch) continue;

            List<Vector2Int> tilesPos = new List<Vector2Int>();
            foreach (Vector2Int posTile in GameManager.Of.Grid.GridLevelData.allTilesPosition)
            {
                CellData.SpawnPointType spawn = CellData.SpawnPointType.None;
                GameManager.Of.Grid.GridLevelData.spawnPointsTiles.TryGetValue(posTile, out spawn);

                if (spawn.HasFlag(CellData.SpawnPointType.Zombie) || spawn.HasFlag(CellData.SpawnPointType.Witch))
                {
                    tilesPos.Add(posTile);
                }
            }

            _spawnTiles.Add(evnt.Turn, GameManager.Of.Grid.GridLevelData.keys[evnt.SpawnCellIndex]);
        }
    }
}