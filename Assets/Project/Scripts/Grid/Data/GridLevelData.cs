using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Vector2IntList
{
    public List<Vector2Int> items = new();
}

public class GridLevelData : ScriptableObject, ISerializationCallbackReceiver
{
    [ReadOnly] public List<Vector2Int> allTilesPosition = new List<Vector2Int>();

    [ReadOnly] public Dictionary<Vector2Int, CellData.SpawnPointType> spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();
    [ReadOnly] public List<Vector2Int> keys = new List<Vector2Int>();
    [ReadOnly] public List<CellData.SpawnPointType> values = new List<CellData.SpawnPointType>();

    [ReadOnly] public List<RoomData> allRooms = new List<RoomData>();
    [ReadOnly] public List<Vector2Int> allRoomsPosition = new List<Vector2Int>();
    [SerializeField, ReadOnly] private List<Vector2IntList> _allRoomsCellsBis = new();
    public IReadOnlyList<Vector2IntList> allRoomsCells => _allRoomsCellsBis;
    [ReadOnly] public List<GameObject> allRoomsPrefab = new List<GameObject>();

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
        allRoomsPosition.Clear();
        _allRoomsCellsBis.Clear();
        allRoomsPrefab.Clear();
        foreach(var a in allRooms)
        {
            allRoomsPosition.Add(a.GetMainCell());

            var wrapped = new Vector2IntList();
            var cells = a.GetAllCellsInRoom();
            if (cells != null)
            {
                wrapped.items.AddRange(cells);
            }
            _allRoomsCellsBis.Add(wrapped);

            allRoomsPrefab.Add(a.GetRoomPrefab());
        }
    }

    public void OnAfterDeserialize()
    {
        spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();
        for (int i = 0; i < keys.Count; i++)
        {
            spawnPointsTiles.Add(keys[i], values[i]);
        }
        allRooms = new List<RoomData>();
        for (int i = 0; i < allRoomsPosition.Count; i++)
        {
            RoomData data = new RoomData();
            data.SetMainCell(allRoomsPosition[i]);
            data.SetAllCellsInRoom(new List<Vector2Int>(_allRoomsCellsBis[i].items));
            data.SetRoomPrefab(allRoomsPrefab[i]);
            allRooms.Add(data);
        }
    }
}
