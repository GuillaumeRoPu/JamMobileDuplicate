using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public enum EventType
{
    Zombie
}

[System.Serializable]
public class TurnEvent
{
    public int Turn;
    public EventType EventType;
}

public class TurnManager : MonoBehaviour
{
    int _currentTurn;
    public float Turn => _currentTurn;

    [SerializeField] float _durationTurn;
    [SerializeField] LevelData _level;

    [SerializeField] PathFeedback _pathFeedback;
    public PathFeedback GetPathFeedback => _pathFeedback;

    [SerializeField] TurnChronoUI _ui;

    bool _notNext;

    int _nbPass;

    List<List<GameObject>> _zombieFeedbacks = new List<List<GameObject>>();
    List<List<Vector2Int>> _zombiePaths = new List<List<Vector2Int>>();

    public void Init()
    {
        foreach (var evnt in _level.Events)
        {
            if (evnt.EventType == EventType.Zombie)
                _ui.PlaceZombieTurn(evnt.Turn, _level.TurnMax);
        }
    }

    public void PassTurn(int nbTurnToPass)
    {
        _nbPass = nbTurnToPass;
    }

    public void NextTurn()
    {
        if (_notNext) return;

        _notNext = true;
        ZombieTurn();
        PlayTurn();
        
    }

    void PlayTurn()
    {
        _nbPass--;
        _currentTurn++;
        GameManager.Of.Player.MoveToNextTileInPath(_durationTurn);
        foreach (var zombie in GameManager.Of.Zombies)
        {
            zombie.MoveToNextTileInPath(_durationTurn);
        }
        _ui.UpdateTurn(_durationTurn, _currentTurn, _level.TurnMax);

        foreach(var evnt in _level.Events)
        {
            if (evnt.Turn == _currentTurn)
            {
                GameManager.Of.SpawnZombie();
            }
        }

        WaitEndOfTurn();
    }

    async void WaitEndOfTurn()
    {
        await Awaitable.WaitForSecondsAsync(_durationTurn);

        if (_nbPass > 0)
        {
            PlayTurn();
        }
        else
        {
            _notNext = false;
        }
    }

    public void CalculZombiePath(List<Vector2Int> playerPath)
    {
        _zombiePaths.Clear();
        ResetFeedback();
        int zIndex = 0;
        if (playerPath.Count <= 1) return;
        foreach (var zombie in GameManager.Of.Zombies)
        {
            _zombieFeedbacks.Add(new List<GameObject>());
            List<Vector2Int> diff = new List<Vector2Int>();
            List<Vector2Int> tiles = new List<Vector2Int>();
            tiles.Add(zombie.CurrentTile.CellData.GridPosition);
            for (int i = 1; i < playerPath.Count; i++)
            {
                List<Vector2Int> path = zombie.GetComponent<AIPathfinding>().CreatePath(GridManager.Tiles[playerPath[i]]);
                diff.Add(path[i] - tiles[tiles.Count - 1]);
                tiles.Add(path[i]);
            }

            Vector2Int lastDiff = Vector2Int.zero;

            int tileIndex = 1;
            foreach (var result in diff)
            {
                if (lastDiff == Vector2Int.right)
                {
                    if (result == Vector2Int.right)
                    {
                        ModifyLastFeedback(zIndex,_pathFeedback.Row, 0);
                        SpawnFeedback(zIndex,_pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 0);
                    }
                    if (result == Vector2Int.up)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Turn, 0);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 90);
                    }
                    if (result == Vector2Int.down)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Turn, 0, false, true);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), -90);
                    }
                }

                if (lastDiff == Vector2Int.left)
                {
                    if (result == Vector2Int.left)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Row, 0);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 180);
                    }
                    if (result == Vector2Int.up)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Turn, 0, true);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 90);
                    }
                    if (result == Vector2Int.down)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Turn, 0, true, true);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), -90);
                    }
                }

                if (lastDiff == Vector2Int.up)
                {
                    if (result == Vector2Int.right)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Turn, 0, true, true);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 0);
                    }
                    if (result == Vector2Int.left)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Turn, 0, false, true);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 180);
                    }
                    if (result == Vector2Int.up)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Row, 90);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 90);
                    }
                }

                if (lastDiff == Vector2Int.down)
                {
                    if (result == Vector2Int.right)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Turn, 0, true);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 0);
                    }
                    if (result == Vector2Int.left)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Turn, 0);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 180);
                    }
                    if (result == Vector2Int.down)
                    {
                        ModifyLastFeedback(zIndex, _pathFeedback.Row, 90);
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), -90);
                    }
                }

                if (lastDiff == Vector2Int.zero)
                {
                    if (result == Vector2Int.right)
                    {
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 0);
                    }
                    if (result == Vector2Int.left)
                    {
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 180);
                    }
                    if (result == Vector2Int.up)
                    {
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 90);
                    }
                    if (result == Vector2Int.down)
                    {
                        SpawnFeedback(zIndex, _pathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), -90);
                    }
                }

                lastDiff = result;
                tileIndex++;
            }
            zIndex++;
            _zombiePaths.Add(tiles);
        }
    }

    public void ZombieTurn()
    {
        ResetFeedback();
        int zombieIndex = 0;
        foreach (var zombie in GameManager.Of.Zombies)
        {
            zombie.GetComponent<Entity>().SetPath(_zombiePaths[zombieIndex]);
            zombieIndex++;
        }
    }
    
    void SpawnFeedback(int zombieIndex, Sprite sprite, Vector2 pos, float rotation, bool xFlip = false, bool yFlip = false)
    {
        GameObject feed = new GameObject("Zombie Feedback's");
        SpriteRenderer renderer = feed.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 100;
        renderer.flipX = xFlip;
        renderer.flipY = yFlip;
        renderer.sprite = sprite;
        feed.transform.position = pos;
        feed.transform.eulerAngles = new Vector3(0, 0, rotation);
        _zombieFeedbacks[zombieIndex].Add(feed);
    }
    void ModifyLastFeedback(int zombieIndex, Sprite sprite, float rotation, bool xFlip = false, bool yFlip = false)
    {
        if (_zombieFeedbacks[zombieIndex].Count == 0) return;

        GameObject feed = _zombieFeedbacks[zombieIndex][_zombieFeedbacks[zombieIndex].Count - 1];
        feed.transform.eulerAngles = new Vector3(0, 0, rotation);
        SpriteRenderer renderer = feed.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.flipX = xFlip;
        renderer.flipY = yFlip;
    }

    void ResetFeedback()
    {
        foreach (var feed in _zombieFeedbacks)
        {
            foreach (var go in feed)
            {
                Destroy(go);
            }
        }

        _zombieFeedbacks.Clear();
    }
}