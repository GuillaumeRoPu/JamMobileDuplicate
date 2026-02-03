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
    public Entity _entity;


    public bool CanPass(Face face, CellData originalCell)
    {

        Face receptFace = face == Face.Up ? Face.Down :
            face == Face.Left ? Face.Right :
            face == Face.Down ? Face.Up :
            face == Face.Right ? Face.Left : Face.None;

        if (receptFace == Face.None) return false;

        if (_mainRoomCell == originalCell._mainRoomCell || _doorPosition == receptFace)
        {
            return true;
        }
        return false;
    }
}
