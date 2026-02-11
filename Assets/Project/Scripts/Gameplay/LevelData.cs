using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Granny/Level Data")]
public class LevelData : ScriptableObject
{
    [field: SerializeField] public GridLevelData GridLevel { get; private set; }
    [field: SerializeField] public int TurnMax { get; private set; }
    [field: SerializeField] public TurnEvent[] Events { get; private set; }

    Dictionary<int, Vector2Int> _zombieSpawnTiles;
    public Vector2Int GetNextSpawn(int turn)
    {
        return _zombieSpawnTiles[turn];
    }

    public void Init()
    {
        _zombieSpawnTiles = new Dictionary<int, Vector2Int>();
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

            Vector2Int pos = tilesPos[evnt.SpawnCellIndex % tilesPos.Count];
            _zombieSpawnTiles.Add(evnt.Turn, pos);
        }
    }
}
