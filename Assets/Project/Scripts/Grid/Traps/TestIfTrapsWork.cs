using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class TestIfTrapsWork : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private Trap _trap;
    [SerializeField] private Vector2Int _playerPosition;
    [SerializeField] private Face _direction;

    [Button("Place Trap")]
    private void PlaceTrapButton()
    {
        PlaceTrap(_trap, _playerPosition, _direction);
    }

    private void PlaceTrap(Trap newTrap, Vector2Int playerPosition, Face direction)
    {
        if (newTrap != null) 
        {
            if (newTrap.TrapMode == Trap.TrapType.Single)
            {
                var allPositions = new List<Vector2Int>();
                Vector2Int offset = new Vector2Int();
                switch (direction)
                {
                    case Face.Up:
                        offset = new Vector2Int(0, 1);
                        break;
                    case Face.Down:
                        offset = new Vector2Int(0, -1);
                        break;
                    case Face.Left:
                        offset = new Vector2Int(-1, 0);
                        break;
                    case Face.Right:
                        offset = new Vector2Int(1, 0);
                        break;
                    default:
                        Debug.LogWarning("You didn't input a valid direction for the trap");
                        break;
                }
                var trapPosition = playerPosition + offset;
                if (GridManager.Tiles.ContainsKey(trapPosition))
                {
                    if (GridManager.Tiles[trapPosition].CellData.CanPlaceTrap(_trap))
                    {
                        var trap = Instantiate(newTrap, (Vector2)trapPosition + new Vector2(0.5f, 0.5f), Quaternion.identity);
                        allPositions.Add(trapPosition);
                        trap.TrapPlaced(trapPosition, direction);
                        GridManager.Tiles[trapPosition].CellData.SetTrap(trap);
                    }
                }
            }
            else if (newTrap.TrapMode == Trap.TrapType.Multiple)
            {
                Vector2Int offset = new Vector2Int();
                switch (direction)
                {
                    case Face.Up:
                        offset = new Vector2Int(0, 1);
                        break;
                    case Face.Down:
                        offset = new Vector2Int(0, -1);
                        break;
                    case Face.Left:
                        offset = new Vector2Int(-1, 0);
                        break;
                    case Face.Right:
                        offset = new Vector2Int(1, 0);
                        break;
                    default:

                        Debug.LogWarning("You didn't input a valid direction for the trap");
                        break;
                }
                var trapPosition = playerPosition + offset;
                Debug.Log("Pos : X = " + trapPosition.x + " | Y = " + trapPosition.y);
                bool canPose = true;

                if (GridManager.Tiles.ContainsKey(trapPosition))
                {
                    if (GridManager.Tiles[trapPosition].CellData.CanPlaceTrap(_trap))
                    {

                    }
                    else
                    {
                        canPose = false;
                    }
                }
                else
                {
                    canPose = false;
                }

                foreach (Vector2Int pos in newTrap.AdditionalPosition)
                {
                    Vector2Int newpos = pos;
                    switch (direction)
                    {
                        case Face.Up:
                            newpos = new Vector2Int(-pos.y, pos.x);
                            break;
                        case Face.Down:
                            newpos = new Vector2Int(pos.y, -pos.x);
                            break;
                        case Face.Left:
                            newpos = new Vector2Int(-pos.x, -pos.y);
                            break;
                        case Face.Right:
                            newpos = pos;
                            break;
                        default:
                            break;
                    }
                    var trapPositionBis = playerPosition + newpos + offset;
                    Debug.Log("Pos : X = " + trapPositionBis.x + " | Y = " + trapPositionBis.y);
                    if (GridManager.Tiles.ContainsKey(trapPositionBis))
                    {
                        if (GridManager.Tiles[trapPositionBis].CellData.CanPlaceTrap(_trap))
                        {
                            Debug.Log("Been There");
                        }
                        else
                        {
                            canPose = false;
                        }
                    }
                    else
                    {
                        canPose = false;
                    }
                }

                if (canPose == true)
                {
                    var trap = Instantiate(newTrap, (Vector2)trapPosition + new Vector2(0.5f, 0.5f), Quaternion.identity);
                    switch (direction)
                    {
                        case Face.Up:
                            trap.transform.Rotate(new Vector3(0, 0, 90));
                            break;
                        case Face.Down:
                            trap.transform.Rotate(new Vector3(0, 0, -90));
                            break;
                        case Face.Left:
                            trap.transform.Rotate(new Vector3(0, 0, 180));
                            break;
                        case Face.Right:
                            break;
                        default:
                            break;
                    }
                    trap.TrapPlaced(trapPosition, direction);
                    GridManager.Tiles[trapPosition].CellData.SetTrap(trap);
                    foreach (var pos in trap.AdditionalPosition)
                    {
                        GridManager.Tiles[pos].CellData.SetTrap(trap);
                    }
                }
            }
            else if (newTrap.TrapMode == Trap.TrapType.Door)
            {
                var allPositions = new List<Vector2Int>();
                var trapPosition = playerPosition;
                if (GridManager.Tiles.ContainsKey(trapPosition))
                {
                    if (GridManager.Tiles[trapPosition].CellData.CanPlaceTrap(_trap) && GridManager.Tiles[trapPosition].CellData.HasADoor())
                    {
                        var otherDoorTile = trapPosition;
                        switch (GridManager.Tiles[trapPosition].CellData.DoorPosition)
                        {
                            case Face.Up:
                                otherDoorTile += new Vector2Int(0, 1);
                                break;
                            case Face.Down:
                                otherDoorTile += new Vector2Int(0, -1);
                                break;
                            case Face.Left:
                                otherDoorTile += new Vector2Int(-1, 0);
                                break;
                            case Face.Right:
                                otherDoorTile += new Vector2Int(1, 0);
                                break;
                            default:
                                break;
                        }
                        bool canPlace = true;
                        if (GridManager.Tiles.ContainsKey(otherDoorTile))
                        {
                            if (GridManager.Tiles[otherDoorTile].CellData.HasADoorTrap())
                            {
                                canPlace = false;
                            }
                        }
                        if (canPlace) 
                        {
                            var offset = new Vector2(0, 0);
                            switch (GridManager.Tiles[trapPosition].CellData.DoorPosition)
                            {
                                case Face.Up:
                                    offset = new Vector2(0.5f, 1);
                                    break;
                                case Face.Down:
                                    offset = new Vector2(0.5f, 0);
                                    break;
                                case Face.Left:
                                    offset = new Vector2(0, 0.5f);
                                    break;
                                case Face.Right:
                                    offset = new Vector2(1, 0.5f);
                                    break;
                                default:
                                    break;
                            }
                            var trap = Instantiate(newTrap, (Vector2)trapPosition + offset, Quaternion.identity);
                            allPositions.Add(trapPosition);
                            trap.TrapPlaced(trapPosition, direction);
                            GridManager.Tiles[trapPosition].CellData.SetTrap(trap);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("The Trap Mode isn't finished yet");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)_playerPosition + new Vector2(0.5f, 0.5f), 0.25f);
        if (_trap.TrapMode == Trap.TrapType.Single)
        {
            Vector2Int offset = new Vector2Int();
            switch (_direction)
            {
                case Face.Up:
                    offset = new Vector2Int(0, 1);
                    break;
                case Face.Down:
                    offset = new Vector2Int(0, -1);
                    break;
                case Face.Left:
                    offset = new Vector2Int(-1, 0);
                    break;
                case Face.Right:
                    offset = new Vector2Int(1, 0);
                    break;
                default:
                    offset = new Vector2Int(0, 0);
                    break;
            }
            var trapPosition = _playerPosition + offset;
            Gizmos.color = Color.blueViolet;
            Gizmos.DrawWireSphere((Vector2)trapPosition + new Vector2(0.5f, 0.5f), 0.25f);
        }
        else if (_trap.TrapMode == Trap.TrapType.Multiple)
        {
            Gizmos.color = Color.blueViolet;
            var offset = new Vector2Int(1, 0);
            switch (_direction)
            {
                case Face.Up:
                    offset = new Vector2Int(-offset.y, offset.x);
                    break;
                case Face.Down:
                    offset = new Vector2Int(offset.y, -offset.x);
                    break;
                case Face.Left:
                    offset = new Vector2Int(-offset.x, -offset.y);
                    break;
                case Face.Right:
                    break;
                default:
                    break;
            }
            Gizmos.DrawWireSphere(_playerPosition + offset + new Vector2(0.5f, 0.5f), 0.25f);
            foreach (var pos in _trap.AdditionalPosition)
            {
                var newpos = new Vector2Int(pos.x + 1, pos.y);
                switch (_direction)
                {
                    case Face.Up:
                        newpos = new Vector2Int(-newpos.y, newpos.x);
                        break;
                    case Face.Down:
                        newpos = new Vector2Int(newpos.y, -newpos.x);
                        break;
                    case Face.Left:
                        newpos = new Vector2Int(-newpos.x, -newpos.y);
                        break;
                    case Face.Right:
                        break;
                    default:
                        break;
                }
                Gizmos.DrawWireSphere((Vector2)newpos + _playerPosition + new Vector2(0.5f, 0.5f), 0.25f);
            }
        }
        else if (_trap.TrapMode == Trap.TrapType.Door)
        {

        }
    }
}
