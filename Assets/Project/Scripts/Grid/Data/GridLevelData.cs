using System.Collections.Generic;
using UnityEngine;

public class GridLevelData : ScriptableObject
{
    [ReadOnly] public List<Vector2Int> allTilesPosition = new List<Vector2Int>();
    [ReadOnly] public Dictionary<Vector2Int, CellData.SpawnPointType> spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();
    [ReadOnly] public List<Vector2Int> spawnPointsTilesPosition = new List<Vector2Int>();
    [ReadOnly] public List<CellData.SpawnPointType> spawnPointsTilesTypes = new List<CellData.SpawnPointType>();
    [ReadOnly] public Vector2Int limitsMin;
    [ReadOnly] public Vector2Int limitsMax;
}
