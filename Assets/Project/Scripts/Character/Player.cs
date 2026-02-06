using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField, Range(1, 10)] int _maxTurnToMove = 1;
    [SerializeField] GameObject _pathOver;
    List<GameObject> _pathOverList = new List<GameObject>();
 
    List<GameObject> _feedbacks = new List<GameObject>();
    List<Vector2Int> _diffFeedbacks = new List<Vector2Int>();
    List<Vector2Int> _selectPath = new List<Vector2Int>();

    Tile _last;
    bool _selected = false;

    public void SelectPath(Vector2Int tile)
    {
        if (!GameManager.Of.CanPlay) return;

        if (_selected)
        {
            if (GridManager.Tiles.ContainsKey(tile) )
            {
                if (GridManager.Tiles[tile] != _last)
                {
                    if (_last.CellData.hasNeighbour(GridManager.Tiles[tile].CellData.GridPosition))
                    {
                        if (_selectPath.Count - 1 == _maxTurnToMove) return;
                        _last = GridManager.Tiles[tile];
                        _selectPath.Add(tile);
                        CreateFeddback();
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
            _last = GridManager.Tiles[tile];
            _selectPath.Clear();
            _selectPath.Add(tile);
            _selected = true;
        }
    }

    public void SendSelectedPath()
    {
        if (!_selected) return;
        if (_selectPath.Count > 1)
        {
            GameManager.Of.Turn.CalculZombiePath(_selectPath);
            GameManager.Of.Turn.PassTurn(_selectPath.Count - 1);
            GameManager.Of.Player.SetPath(new List<Vector2Int>(_selectPath), ReachedTarget);
        }
        else
        {
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

        PathFeedback feedback = GameManager.Of.Turn.GetPathFeedback;

        if (_diffFeedbacks.Count > 0)
        {
            Vector2Int lastDiff = _diffFeedbacks[_diffFeedbacks.Count - 1];
            bool addFeed = true;

            if (lastDiff == Vector2Int.right)
            {
                if (diff == Vector2Int.right)
                {
                    ModifyLastFeedback(feedback.Row, 0);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 0);
                }
                if (diff == Vector2Int.left)
                {
                    DestroyLastFeedback();
                    addFeed = false;
                }
                if (diff == Vector2Int.up)
                {
                    ModifyLastFeedback(feedback.Turn, 0);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 90);
                }
                if (diff == Vector2Int.down)
                {
                    ModifyLastFeedback(feedback.Turn, 0, false, true);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), -90);
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
                    ModifyLastFeedback(feedback.Row, 0);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 180);
                }
                if (diff == Vector2Int.up)
                {
                    ModifyLastFeedback(feedback.Turn, 0, true);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 90);
                }
                if (diff == Vector2Int.down)
                {
                    ModifyLastFeedback(feedback.Turn, 0, true, true);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), -90);
                }
            }

            if (lastDiff == Vector2Int.up)
            {
                if (diff == Vector2Int.right)
                {
                    ModifyLastFeedback(feedback.Turn, 0, true, true);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 0);
                }
                if (diff == Vector2Int.left)
                {
                    ModifyLastFeedback(feedback.Turn, 0, false, true);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 180);
                }
                if (diff == Vector2Int.up)
                {
                    ModifyLastFeedback(feedback.Row, 90);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 90);
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
                    ModifyLastFeedback(feedback.Turn, 0, true);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 0);
                }
                if (diff == Vector2Int.left)
                {
                    ModifyLastFeedback(feedback.Turn, 0);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 180);
                }
                if (diff == Vector2Int.up)
                {
                    DestroyLastFeedback();
                    addFeed = false;
                }
                if (diff == Vector2Int.down)
                {
                    ModifyLastFeedback(feedback.Row, 90);
                    SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), -90);
                }
            }

            if (addFeed)
                _diffFeedbacks.Add(diff);
        }
        else
        {
            if (diff == Vector2Int.right)
            {
                SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 0);
            }
            if (diff == Vector2Int.left)
            {
                SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 180);
            }
            if (diff == Vector2Int.up)
            {
                SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), 90);
            }
            if (diff == Vector2Int.down)
            {
                SpawnFeedback(feedback.Arrow, (Vector2)now * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), -90);
            }
            _diffFeedbacks.Add(diff);
        }
    }

    void SpawnFeedback(Sprite sprite, Vector2 pos, float rotation, bool xFlip = false, bool yFlip = false)
    {
        GameObject feed = new GameObject("Granny Feedback's");
        SpriteRenderer renderer = feed.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 100;
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

        PathFeedback feedback = GameManager.Of.Turn.GetPathFeedback;

        if (_diffFeedbacks.Count > 0)
        {
            if (_diffFeedbacks[_diffFeedbacks.Count - 1] == Vector2.right)
            {
                ModifyLastFeedback(feedback.Arrow, 0);
            }

            if (_diffFeedbacks[_diffFeedbacks.Count - 1] == Vector2.left)
            {
                ModifyLastFeedback(feedback.Arrow, 180);
            }

            if (_diffFeedbacks[_diffFeedbacks.Count - 1] == Vector2.up)
            {
                ModifyLastFeedback(feedback.Arrow, 90);
            }

            if (_diffFeedbacks[_diffFeedbacks.Count - 1] == Vector2.down)
            {
                ModifyLastFeedback(feedback.Arrow, -90);
            }

        }
    }

    void ReachedTarget()
    {
        foreach (GameObject over in _pathOverList)
        {
            Destroy(over);
        }
        _pathOverList.Clear();
    }
}