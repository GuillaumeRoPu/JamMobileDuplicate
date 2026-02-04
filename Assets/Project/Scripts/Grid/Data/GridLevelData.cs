using System.Collections.Generic;
using UnityEngine;

public class GridLevelData : ScriptableObject, ISerializationCallbackReceiver
{
    [ReadOnly] public List<Vector2Int> allTilesPosition = new List<Vector2Int>();
    [ReadOnly] public Dictionary<Vector2Int, CellData.SpawnPointType> spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();
    [ReadOnly] public List<Vector2Int> keys = new List<Vector2Int>();
    [ReadOnly] public List<CellData.SpawnPointType> values = new List<CellData.SpawnPointType>();
    [ReadOnly] public Vector2Int limitsMin;
    [ReadOnly] public Vector2Int limitsMax;

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var kvp in spawnPointsTiles)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();
        for (int i = 0; i < keys.Count; i++)
        {
            spawnPointsTiles.Add(keys[i], values[i]);
        }
    }
}
