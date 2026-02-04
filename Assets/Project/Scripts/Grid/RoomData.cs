using System.Collections.Generic;
using UnityEngine;

public class RoomData
{
    private Vector2Int _mainCell;
    private List<Vector2Int> _allCells;

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
}
