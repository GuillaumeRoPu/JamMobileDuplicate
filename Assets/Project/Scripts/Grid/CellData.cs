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
    public SpawnPointType spawnPointType = SpawnPointType.None;
    //public Trap _trap;
    public Face DoorPosition = Face.None;
    public bool HasDoor = false;
    public Entity Entity;

    public bool CanPass(Face face, CellData originalCell)
    {

        Face receptFace = face == Face.Up ? Face.Down :
            face == Face.Left ? Face.Right :
            face == Face.Down ? Face.Up :
            face == Face.Right ? Face.Left : Face.None;

        if (receptFace == Face.None) return false;

        if (Entity != null) return false;

        if (MainRoomCell == originalCell.MainRoomCell || DoorPosition == receptFace || originalCell.DoorPosition == face)
        {
            return true;
        }
        return false;
    }

    public void SetDoor(Face face)
    {
        if (face != Face.None)
        {
            DoorPosition = face;
            HasDoor = true;
        }
        else
        {
            DoorPosition = face;
            HasDoor = false;
        }
    }

    public bool HasADoor()
    {
        return HasDoor;
    }

    public bool hasNeighbour(Vector2Int tile)
    {
        return tile == GridPosition + Vector2Int.right || tile == GridPosition + Vector2Int.left ||
            tile == GridPosition + Vector2Int.up || tile == GridPosition + Vector2Int.down;
    }
}
