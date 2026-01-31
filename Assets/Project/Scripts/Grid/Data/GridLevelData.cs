using System;
using System.Collections.Generic;
using UnityEngine;

public class GridLevelData : ScriptableObject
{
    [ReadOnly] public List<Vector2Int> allTilesPosition = new List<Vector2Int>();
    [ReadOnly] public Vector2Int spawnTile;
    [ReadOnly] public Vector2Int limitsMin;
    [ReadOnly] public Vector2Int limitsMax;
}
