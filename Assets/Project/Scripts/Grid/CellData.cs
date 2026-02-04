using System;
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

    [Flags]
    public enum SpawnPointType
    {
        None = 0,
        Player = 1,
        Zombie = 2,
        Ghost = 4,
        Echo = 8,
        Werewolf = 16,
        Mimic = 32,
    }

    public Vector2Int GridPosition;
    public Vector2Int MainRoomCell;
    public bool IsSpawnPoint = false;
    public SpawnPointType SpawnPointType = SpawnPointType.None;
    //public Trap _trap;
    public Face DoorPosition = Face.None;
    public Entity Entity;


    public bool CanPass(Face face, CellData originalCell)
    {

        Face receptFace = face == Face.Up ? Face.Down :
            face == Face.Left ? Face.Right :
            face == Face.Down ? Face.Up :
            face == Face.Right ? Face.Left : Face.None;

        if (receptFace == Face.None) return false;

        if (Entity != null) return false;

        if (MainRoomCell == originalCell.MainRoomCell || DoorPosition == receptFace)
        {
            return true;
        }
        return false;
    }
}
