using UnityEngine;

public enum Face
{
    Up,
    Down,
    Left,
    Right,
    None
}

public class CellData
{
    public Vector2Int _gridPosition;
    public Vector2Int _mainRoomCell;
    public bool _isSpawnPoint = false;
    //public Trap _trap;
    public Face _doorPosition = Face.None;
    //public Entity _entity;


    public bool CanPass(Face face, CellData originalCell)
    {
        if (_mainRoomCell == originalCell._mainRoomCell || _doorPosition == face)
        {
            return true;
        }
        return false;
    }
}
