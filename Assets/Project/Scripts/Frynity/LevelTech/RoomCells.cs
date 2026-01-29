using Frynity.Window;
using System.Collections.Generic;
using UnityEngine;

public class RoomCells : MonoBehaviour
{
    List<Vector2Int> _localCells = new List<Vector2Int>();
    List<Vector2> _localDoors = new List<Vector2>();

    RoomCreator _creator;

    public void CreateRoom(List<Vector2Int> _cells, List<Vector2> _doors, RoomCreator creator)
    {
        _localCells = _cells;
        _localDoors = _doors;
        _creator = creator;
    }

    [Button]
    void ModifyRoom()
    {
        _creator.Open(this);
    }
}
