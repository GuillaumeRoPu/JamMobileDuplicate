using System.Collections.Generic;
using UnityEngine;

public class RoomData
{
    private Vector2Int _mainCell;
    private List<Vector2Int> _allCells;
    private GameObject _roomPrefab;
    private Dictionary<Vector2Int, Face> _allDoors;

    public Vector2Int GetMainCell()
    {
        return _mainCell;
    }

    public void SetMainCell(Vector2Int cell)
    {
        _mainCell = cell;
    }

    public List<Vector2Int> GetAllCellsInRoom()
    {
        return _allCells;
    }

    public void SetAllCellsInRoom(List<Vector2Int> list)
    {
        _allCells = list;
    }

    public GameObject GetRoomPrefab()
    {
        return _roomPrefab;
    }

    public void SetRoomPrefab(GameObject room)
    {
        _roomPrefab = room;
    }

    public Dictionary<Vector2Int, Face> GetAllDoorsinRoom()
    {
        return _allDoors;
    }

    public void SetAllDoorsinRoom(Dictionary<Vector2Int, Face> allDoors)
    {
        _allDoors = allDoors;
    }
}
