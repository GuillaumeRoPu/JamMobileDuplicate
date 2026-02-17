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
        Echo = 4,
        Witch = 8,
    }

    public Vector2Int GridPosition;
    public Vector2Int MainRoomCell;
    public bool IsSpawnPoint = false;
    public SpawnPointType spawnPointType = SpawnPointType.None;
    public bool HasRoom;
    public Trap tileTrap;
    public Trap doorTrap;
    public bool hasATileTrap;
    public bool hasADoorTrap;
    public Face DoorPosition = Face.None;
    public bool HasDoor = false;
    public bool DoorKeyClosed;
    public Entity Entity;
    public bool hasAMaterial = false;

    public bool CanPass(Face face, CellData originalCell)
    {
        if (!HasRoom) return false;

        Face receptFace = face == Face.Up ? Face.Down :
            face == Face.Left ? Face.Right :
            face == Face.Down ? Face.Up :
            face == Face.Right ? Face.Left : Face.None;

        if (receptFace == Face.None) return false;

        if (HasADoorTrap()) return false;

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

    public bool HasEntity()
    {
        if (Entity != null)
        {
            return true;
        }
        else
        {
            return false;
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

    public bool IsTheSameRoomOrHaveDoor(Tile tile, Face face)
    {
        Face receptFace = face == Face.Up ? Face.Down :
           face == Face.Left ? Face.Right :
           face == Face.Down ? Face.Up :
           face == Face.Right ? Face.Left : Face.None;

        if (receptFace == Face.None) return false;

        return tile.CellData.MainRoomCell == MainRoomCell || DoorPosition == receptFace || tile.CellData.DoorPosition == face;
    }

    public void SetTrap(Trap newtrap)
    {
        if (newtrap.TrapMode == Trap.TrapType.Door)
        {
            if (!hasADoorTrap)
            {
                doorTrap = newtrap;
                hasADoorTrap = true;
                doorTrap.onDestroy += RemoveDoorTrap;
            }
            else
            {
                Debug.LogWarning("Tried to set a trap on another trap");
            }
        }
        else
        {
            if (!hasATileTrap)
            {
                tileTrap = newtrap;
                hasATileTrap = true;
                tileTrap.onDestroy += RemoveTileTrap;
            }
            else
            {
                Debug.LogWarning("Tried to set a trap on another trap");
            }
        }
    }

    public void RemoveTileTrap()
    {
        tileTrap.onDestroy -= RemoveTileTrap;
        tileTrap = null;
        hasATileTrap = false;
        Debug.Log("Removed a trap at : X = " + GridPosition.x + " | Y = " + GridPosition.y);
    }

    public void RemoveDoorTrap()
    {
        doorTrap.onDestroy -= RemoveDoorTrap;
        doorTrap = null;
        hasADoorTrap = false;
        Debug.Log("Removed a trap");
    }

    public bool HasATileTrap()
    {
        return hasATileTrap;
    }

    public bool HasADoorTrap()
    {
        return hasADoorTrap;
    }

    public void KillEntity()
    {
        GameManager.Of.KillAnEntity(Entity);
    }

    public void ActivateTileTrap(Entity entity)
    {
        if (HasATileTrap())
        {
            tileTrap.ActivateTrap(entity);
        }
    }

    public void ActivateDoorTrap(Entity entity)
    {
        if (HasADoorTrap())
        {
            doorTrap.ActivateTrap(entity);
        }
    }

    public (bool, Vector2Int) GiveTileAMaterial()
    {
        if (!hasAMaterial)
        {
            hasAMaterial = true;
            return (true, GridPosition);
        }
        else
        {
            if (GridManager.Tiles.ContainsKey(GridPosition + new Vector2Int(0, 1)))
            {
                return GridManager.Tiles[GridPosition + new Vector2Int(0, 1)].CellData.GiveTileAMaterial();
            }
            else if (GridManager.Tiles.ContainsKey(GridPosition + new Vector2Int(0, -1)))
            {
                return GridManager.Tiles[GridPosition + new Vector2Int(0, -1)].CellData.GiveTileAMaterial();
            }
            else if (GridManager.Tiles.ContainsKey(GridPosition + new Vector2Int(-1, 0)))
            {
                return GridManager.Tiles[GridPosition + new Vector2Int(-1, 0)].CellData.GiveTileAMaterial();
            }
            else if (GridManager.Tiles.ContainsKey(GridPosition + new Vector2Int(1, 0)))
            {
                return GridManager.Tiles[GridPosition + new Vector2Int(1, 0)].CellData.GiveTileAMaterial();
            }
            else
            {
                return (false, GridPosition);
            }
        }
    }

    /// <summary>
    /// ALWAYS CALL IT WHEN THE GRANDMA WALKS ON A TILE, it removes the materials from the tile
    /// </summary>
    public void RemoveMaterial()
    {
        hasAMaterial = false;
    }

    /// <summary>
    /// Tells you if the tile contains a material or not
    /// </summary>
    public bool ContainsMaterial()
    {
        return hasAMaterial;
    }

    /// <summary>
    /// Returns a bool that tells you wether you can place a trap on the specified tile
    /// </summary>
    public bool CanPlaceTrap(Trap newTrap)
    {
        if (newTrap.TrapMode == Trap.TrapType.Single)
        {
            if (!HasATileTrap() && !HasEntity() && GridManager.Tiles[GridPosition].IsInARoom())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (newTrap.TrapMode == Trap.TrapType.Multiple)
        {
            if (!HasATileTrap() && !HasEntity() && GridManager.Tiles[GridPosition].IsInARoom())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (newTrap.TrapMode == Trap.TrapType.Door)
        {
            if (!HasADoorTrap() && GridManager.Tiles[GridPosition].IsInARoom())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
