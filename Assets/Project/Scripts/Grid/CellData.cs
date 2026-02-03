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
    [Flags] public enum SpawnPointType
    {
        None = 0,
        Player = 1,
        Zombie = 2,
        Ghost = 4,
        Echo = 8,
        Werewolf = 16,
        Mimic = 32,
    }

    public Vector2Int gridPosition;
    public Vector2Int mainRoomCell;
    public bool isSpawnPoint = false;
    public SpawnPointType spawnPointType = SpawnPointType.None;
    //public Trap _trap;
    public Face doorPosition = Face.None;
    public Entity entity;


    public bool CanPass(Face face, CellData originalCell)
    {

        Face receptFace = face == Face.Up ? Face.Down :
            face == Face.Left ? Face.Right :
            face == Face.Down ? Face.Up :
            face == Face.Right ? Face.Left : Face.None;

        if (receptFace == Face.None) return false;

        if (mainRoomCell == originalCell.mainRoomCell || doorPosition == receptFace)
        {
            return true;
        }
        return false;
    }
}
