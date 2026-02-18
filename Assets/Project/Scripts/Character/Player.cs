using GooglePlayGames;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField, Range(1, 10)] int _maxTurnToMove = 1;
    [SerializeField] GameObject _pathOver;
    List<GameObject> _pathOverList = new List<GameObject>();
 
    List<GameObject> _feedbacks = new List<GameObject>();
    List<Vector2Int> _diffFeedbacks = new List<Vector2Int>();
    List<Vector2Int> _selectPath = new List<Vector2Int>();

    [SerializeField] PathFeedback _pathFeedback;

    public System.Action MaterialChange;
    int _materials;
    [SerializeField] TextMeshProUGUI _countMaterialText;
    public int Materials => _materials;
    public void AddMaterial() { _materials++; MaterialChange?.Invoke(); _countMaterialText.text = _materials.ToString(); }
    public void RemoveMaterials(int nb) { _materials -= nb; MaterialChange?.Invoke(); _countMaterialText.text = _materials.ToString(); }

    Tile _last;
    bool _selected = false;

    List<Face> _faceForPlaceTrap = new List<Face>();
    Trap _trap;
    int _price;

    private void Start()
    {
        MaterialChange?.Invoke();
    }

    public void SelectPath(Vector2Int tile)
    {
        if (!GameManager.Of.Turn.CanPlay) return;

        if (_selected)
        {
            if (GridManager.Tiles.ContainsKey(tile) )
            {
                if (GridManager.Tiles[tile] != _last)
                {
                    if (_last.CellData.hasNeighbour(GridManager.Tiles[tile].CellData.GridPosition))
                    {
                        if (GridManager.Tiles[tile].CellData.CanPass(GridManager.GetFace(tile - _selectPath[_selectPath.Count - 1]), GridManager.Tiles[_selectPath[_selectPath.Count - 1]].CellData))
                        {
                            if (_selectPath.Count - 1 == _maxTurnToMove) return;
                            _last = GridManager.Tiles[tile];
                            _selectPath.Add(tile);
                            CreateFeddback();
                        }
                    }
                    else
                    {
                        SendSelectedPath();
                    }
                }
            }
            else
            {
                SendSelectedPath();
            }
        }

        if (tile == GameManager.Of.Player.CurrentTile.CellData.GridPosition && !_selected)
        {
            ResetTrapsPlace();
            _last = GridManager.Tiles[tile];
            _selectPath.Clear();
            _selectPath.Add(tile);
            _selected = true;
        }
    }

    public void SendSelectedPath()
    {
        if (!_selected) return;
        GameManager.Of.Turn.CalculZombiePath(_selectPath);

        GameManager.Of.Player.SetPath(new List<Vector2Int>(_selectPath), ReachedTarget);
        if (_selectPath.Count > 1)
        {
            GameManager.Of.Turn.PassTurn(_selectPath.Count - 1);
        }
        else
        {
            GameManager.Of.Turn.PassTurn(1);
            GameManager.Of.Player.StopPath();
            ReachedTarget();
        }


       _selected = false;

        foreach(GameObject over in _pathOverList)
        {
            Destroy(over);
        }
        _pathOverList.Clear();

        foreach (GameObject go in _feedbacks)
        {
            _pathOverList.Add(Instantiate(_pathOver, go.transform.position, Quaternion.identity));
            Destroy(go);
        }

        _feedbacks.Clear();
        _diffFeedbacks.Clear();
        _selectPath.Clear();
        _last = null;
    }

    void CreateFeddback()
    {
        Vector2Int now = _selectPath[_selectPath.Count - 1];
        Vector2Int last = _selectPath[_selectPath.Count - 2];

        Vector2Int diff = now - last;

        if (_diffFeedbacks.Count > 0)
        {
            Vector2Int lastDiff = _diffFeedbacks[_diffFeedbacks.Count - 1];
            bool addFeed = true;

            if (lastDiff == Vector2Int.right)
            {
                if (diff == Vector2Int.right)
                {
                    ModifyLastFeedback(_pathFeedback.Row, 0);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 0);
                }
                if (diff == Vector2Int.left)
                {
                    DestroyLastFeedback();
                    addFeed = false;
                }
                if (diff == Vector2Int.up)
                {
                    ModifyLastFeedback(_pathFeedback.Turn, 0);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 90);
                }
                if (diff == Vector2Int.down)
                {
                    ModifyLastFeedback(_pathFeedback.Turn, 0, false, true);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), -90);
                }
            }

            if (lastDiff == Vector2Int.left)
            {
                if (diff == Vector2Int.right)
                {
                    DestroyLastFeedback();
                    addFeed = false;
                }
                if (diff == Vector2Int.left)
                {
                    ModifyLastFeedback(_pathFeedback.Row, 0);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 180);
                }
                if (diff == Vector2Int.up)
                {
                    ModifyLastFeedback(_pathFeedback.Turn, 0, true);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 90);
                }
                if (diff == Vector2Int.down)
                {
                    ModifyLastFeedback(_pathFeedback.Turn, 0, true, true);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), -90);
                }
            }

            if (lastDiff == Vector2Int.up)
            {
                if (diff == Vector2Int.right)
                {
                    ModifyLastFeedback(_pathFeedback.Turn, 0, true, true);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 0);
                }
                if (diff == Vector2Int.left)
                {
                    ModifyLastFeedback(_pathFeedback.Turn, 0, false, true);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 180);
                }
                if (diff == Vector2Int.up)
                {
                    ModifyLastFeedback(_pathFeedback.Row, 90);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 90);
                }
                if (diff == Vector2Int.down)
                {
                    DestroyLastFeedback();
                    addFeed = false;
                }
            }

            if (lastDiff == Vector2Int.down)
            {
                if (diff == Vector2Int.right)
                {
                    ModifyLastFeedback(_pathFeedback.Turn, 0, true);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 0);
                }
                if (diff == Vector2Int.left)
                {
                    ModifyLastFeedback(_pathFeedback.Turn, 0);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 180);
                }
                if (diff == Vector2Int.up)
                {
                    DestroyLastFeedback();
                    addFeed = false;
                }
                if (diff == Vector2Int.down)
                {
                    ModifyLastFeedback(_pathFeedback.Row, 90);
                    SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), -90);
                }
            }

            if (addFeed)
                _diffFeedbacks.Add(diff);
        }
        else
        {
            if (diff == Vector2Int.right)
            {
                SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 0);
            }
            if (diff == Vector2Int.left)
            {
                SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 180);
            }
            if (diff == Vector2Int.up)
            {
                SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 90);
            }
            if (diff == Vector2Int.down)
            {
                SpawnFeedback(_pathFeedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), -90);
            }
            _diffFeedbacks.Add(diff);
        }
    }

    void SpawnFeedback(Sprite sprite, Vector2 pos, float rotation, bool xFlip = false, bool yFlip = false)
    {
        GameObject feed = new GameObject("Granny Feedback's");
        SpriteRenderer renderer = feed.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = "Foreground";
        renderer.flipX = xFlip;
        renderer.flipY = yFlip;
        renderer.sprite = sprite;
        feed.transform.position = pos;
        feed.transform.eulerAngles = new Vector3(0, 0, rotation);
        _feedbacks.Add(feed);
    }

    void ModifyLastFeedback(Sprite sprite, float rotation, bool xFlip = false, bool yFlip = false)
    {
        if (_feedbacks.Count == 0) return;

        GameObject feed = _feedbacks[_feedbacks.Count - 1];
        feed.transform.eulerAngles = new Vector3(0, 0, rotation);
        SpriteRenderer renderer = feed.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.flipX = xFlip;
        renderer.flipY = yFlip;
    }

    void DestroyLastFeedback()
    {
        Destroy(_feedbacks[_feedbacks.Count - 1]);
        _diffFeedbacks.RemoveAt(_diffFeedbacks.Count - 1);
        _selectPath.RemoveAt(_selectPath.Count - 1);
        if (_selectPath.Count > 0)
            _selectPath.RemoveAt(_selectPath.Count - 1);
        _feedbacks.RemoveAt(_feedbacks.Count - 1);

        if (_diffFeedbacks.Count > 0)
        {
            if (_diffFeedbacks[_diffFeedbacks.Count - 1] == Vector2.right)
            {
                ModifyLastFeedback(_pathFeedback.Arrow, 0);
            }

            if (_diffFeedbacks[_diffFeedbacks.Count - 1] == Vector2.left)
            {
                ModifyLastFeedback(_pathFeedback.Arrow, 180);
            }

            if (_diffFeedbacks[_diffFeedbacks.Count - 1] == Vector2.up)
            {
                ModifyLastFeedback(_pathFeedback.Arrow, 90);
            }

            if (_diffFeedbacks[_diffFeedbacks.Count - 1] == Vector2.down)
            {
                ModifyLastFeedback(_pathFeedback.Arrow, -90);
            }

        }
    }

    public void ReachedTarget()
    {
        foreach (GameObject over in _pathOverList)
        {
            Destroy(over);
        }
        _pathOverList.Clear();
    }

    public void ChooseTrapToPlace(Trap trap, int price)
    {
        if (!GameManager.Of.Turn.CanPlay) return;

        _trap = trap;
        _price = price;
        Vector2Int player = GameManager.Of.Player.CurrentTile.CellData.GridPosition;


        ResetTrapsPlace();
        _last = GridManager.Tiles[player];
        _selectPath.Clear();
        _selectPath.Add(player);
        _selected = true;

        SendSelectedPath();

        Face[] faces = {Face.Up, Face.Down, Face.Right, Face.Left};
        for (int i = 0; i < 4; i++)
        {
            bool check = CheckIfTrapCanBePlaced(_trap, player, faces[i], false);
            if (check)
                _faceForPlaceTrap.Add(faces[i]);
        }
    }

    public void PlaceTrap(Vector2Int tile)
    {

        if (_faceForPlaceTrap.Count > 0)
        {
            Face face = GridManager.GetFace(tile - GameManager.Of.Player.CurrentTile.CellData.GridPosition);
            if (_faceForPlaceTrap.Contains(face))
            {
                RemoveMaterials(_price);
                CheckIfTrapCanBePlaced(_trap, GameManager.Of.Player.CurrentTile.CellData.GridPosition, face, true);
                if (_trap.TrapMode == Trap.TrapType.Single)
                {
                    PlayGamesPlatform.Instance.ReportProgress(GPGSIds.achievement_the_hunt_is_on, 100f, (bool success) => {
                        Debug.Log(success);
                    });
                }
                else if (_trap.TrapMode == Trap.TrapType.Multiple)
                {
                    PlayGamesPlatform.Instance.ReportProgress(GPGSIds.achievement_charge, 100f, (bool success) => {
                        Debug.Log(success);
                    });
                }
                else if (_trap.TrapMode == Trap.TrapType.Door)
                {
                    PlayGamesPlatform.Instance.ReportProgress(GPGSIds.achievement_you_shall_not_pass, 100f, (bool success) => {
                        Debug.Log(success);
                    });
                }
                GameManager.Of.Turn.NextTurn();
                GameManager.Of.UpdateZOrder();
                ResetTrapsPlace();
            }
            else
            {
                ResetTrapsPlace();
            }
        }
    }

    void ResetTrapsPlace()
    {
        _faceForPlaceTrap.Clear();

        ReachedTarget();
    }

    private bool CheckIfTrapCanBePlaced(Trap newTrap, Vector2Int playerPosition, Face direction, bool placeTrap)
    {
        if (newTrap != null) //checks if the trap exists
        {
            if (newTrap.TrapMode == Trap.TrapType.Single) //checks if the trap is Single (lol)
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
                    if (GridManager.Tiles[trapPosition].CellData.CanPlaceTrap(newTrap)
                        && GridManager.Tiles[trapPosition].CellData.CanPass(direction, GridManager.Tiles[playerPosition].CellData))
                    {
                        _pathOverList.Add(Instantiate(_pathOver, (Vector2)trapPosition * GameManager.Of.Grid.TileSize + Vector2.one / 2, Quaternion.identity));

                        if (placeTrap)
                        {
                            var trap = Instantiate(newTrap, (Vector2)trapPosition + new Vector2(0.5f, 0.5f), Quaternion.identity);
                            allPositions.Add(trapPosition);
                            trap.TrapPlaced(trapPosition, direction);
                            GridManager.Tiles[trapPosition].CellData.SetTrap(trap);
                            GameManager.Of.AddObjectRespectZOrder(trap.gameObject);
                        }
                        return true;
                    }
                }
                return false;
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
                    if (GridManager.Tiles[trapPosition].CellData.CanPlaceTrap(newTrap)
                        && GridManager.Tiles[trapPosition].CellData.CanPass(direction, GridManager.Tiles[playerPosition].CellData))
                    {
                        _pathOverList.Add(Instantiate(_pathOver, (Vector2)trapPosition * GameManager.Of.Grid.TileSize + Vector2.one / 2, Quaternion.identity));

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
                        if (GridManager.Tiles[trapPositionBis].CellData.CanPlaceTrap(newTrap)
                            && GridManager.Tiles[trapPosition].CellData.CanPass(direction, GridManager.Tiles[playerPosition].CellData))
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
                    if (placeTrap)
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
                        GameManager.Of.AddObjectRespectZOrder(trap.gameObject);
                        foreach (var pos in trap.AdditionalPosition)
                        {
                            GridManager.Tiles[pos].CellData.SetTrap(trap);
                        }
                    }
                    return true;
                }
                return false;
            }
            else if (newTrap.TrapMode == Trap.TrapType.Door)
            {
                var allPositions = new List<Vector2Int>();
                var trapPosition = playerPosition;
                if (GridManager.Tiles.ContainsKey(trapPosition))
                {
                    if (GridManager.Tiles[trapPosition].CellData.CanPlaceTrap(newTrap) && GridManager.Tiles[trapPosition].CellData.HasADoor())
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
                            _pathOverList.Add(Instantiate(_pathOver, (Vector2)otherDoorTile * GameManager.Of.Grid.TileSize + Vector2.one / 2, Quaternion.identity));
                            if (placeTrap)
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
                                trap.TrapPlaced(trapPosition, GridManager.Tiles[trapPosition].CellData.DoorPosition);
                                GridManager.Tiles[trapPosition].CellData.SetTrap(trap);
                                GameManager.Of.AddObjectRespectZOrder(trap.gameObject);
                            }
                            return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                Debug.Log("The Trap Mode isn't finished yet");
                return false;
            }
        }
        return false;
    }
}