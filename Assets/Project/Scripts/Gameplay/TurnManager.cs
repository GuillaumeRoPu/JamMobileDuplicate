using Frynity.Utils;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum EventType
{
    Zombie, EchoEnter, EchoLeave, Witch
}

[System.Serializable]
public class TurnEvent
{
    [MinValue(1)] public int Turn;
    public EventType EventType;
    [MinValue(0)] public int SpawnCellIndex;
}

public class TurnManager : MonoBehaviour
{
    int _lastEventTurn;
    int _currentTurn;
    public float Turn => _currentTurn;

    [SerializeField] float _durationTurn;
    [SerializeField] LevelData _level;
    [SerializeField] Player _player;
    public void SetLevelData(LevelData level) {  _level = level; }
    public LevelData Level => _level;

    [SerializeField] PathFeedback _zombiePathFeedback;
    [SerializeField] GameObject _witchPathFeedback;
    [SerializeField] GameObject _spawnWarning;
    List<GameObject> _warnings = new List<GameObject>();

    [SerializeField] TurnChronoUI _ui;
    [SerializeField] GameObject _gameOverUI;
    [SerializeField] GameObject _gameWinUI;

    public bool CanPlay => !_notNext;
    bool _notNext;

    int _nbPass;

    List<List<GameObject>> _zombieFeedbacks = new List<List<GameObject>>();
    List<List<Vector2Int>> _zombiePaths = new List<List<Vector2Int>>();

    List<List<GameObject>> _witchFeedbacks = new List<List<GameObject>>();
    List<List<Vector2Int>> _witchAllTilePaths = new List<List<Vector2Int>>();
    List<List<Vector2Int>> _witchPaths = new List<List<Vector2Int>>();

    public void Init()
    {
        _level.Init();
        _currentTurn = 0;

        _notNext = false;

        _gameOverUI.GetComponent<Image>().color = new Color(255, 255, 255, 0);
        _gameWinUI.GetComponent<Image>().color = new Color(255, 255, 255, 0);

        _player.ReachedTarget();
        _player.RemoveMaterials(_player.Materials);

        foreach (var go in _warnings)
        {
            Destroy(go);
        }
        _warnings.Clear();

        foreach (var evnt in _level.Events)
        {
            if (evnt.EventType == EventType.Zombie)
                _ui.PlaceZombieTurn(evnt.Turn, _level.TurnMax);

            if (evnt.EventType == EventType.EchoEnter)
                _ui.PlaceEchoTurn(evnt.Turn, _level.TurnMax);

            if (evnt.EventType == EventType.EchoLeave)
                _ui.PlaceEchoTurn(evnt.Turn, _level.TurnMax, false);

            if (evnt.EventType == EventType.Witch)
                _ui.PlaceWicthTurn(evnt.Turn, _level.TurnMax);

        }

        _lastEventTurn = _level.Events[_level.Events.Length - 1].Turn;

        _ui.UpdateTurn(0.1f, _currentTurn, _level.TurnMax);
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
        WitchTurn();
        PlayTurn();
    }

    void PlayTurn()
    {
        _nbPass--;
        _currentTurn++;
        Vector2Int playerPosition = GameManager.Of.Player.CurrentTile.CellData.GridPosition;
        GameManager.Of.Player.MoveToNextTileInPath(_durationTurn);
        foreach (var zombie in GameManager.Of.Zombies)
        {
            zombie.PassTurn();
            if (zombie.NextPath == GameManager.Of.Player.NextPath)
            {
                zombie.SendAttack();
            }
            else
            {
                zombie.MoveToNextTileInPath(_durationTurn);
            }
        }
        if (GameManager.Of.Echo)
        {
            if (GameManager.Of.PlayerMovement.Count > 3)
            {
                List<Vector2Int> path = new List<Vector2Int>{ GameManager.Of.Echo.CurrentTile.CellData.GridPosition, GameManager.Of.PlayerMovement[GameManager.Of.PlayerMovement.Count - 5] };
                GameManager.Of.Echo.SetPath(path);
                GameManager.Of.Echo.MoveToNextTileInPath(_durationTurn);
            }
        }
        else
        foreach (var witch in GameManager.Of.Witches)
        {
            witch.MoveToNextTileInPath(_durationTurn);
        }

        _ui.UpdateTurn(_durationTurn, _currentTurn, _level.TurnMax);

        foreach (var evnt in _level.Events)
        {
            if (evnt.Turn == _currentTurn + 2)
            {
                if (evnt.EventType == EventType.Zombie)
                {
                    GameManager.Of.SpawnZombie(_level.GetNextSpawn(_currentTurn + 2));
                }
            }

            if (evnt.Turn == _currentTurn)
            {
                if (evnt.EventType == EventType.Zombie)
                {
                    Destroy(_warnings[0]);
                    _warnings.RemoveAt(0);
                }
                if (evnt.EventType == EventType.EchoEnter)
                {
                    GameManager.Of.SpawnEcho(playerPosition);
                }
                else if (evnt.EventType == EventType.EchoLeave)
                {
                    GameManager.Of.DestroyEcho();
                }
                else if (evnt.EventType == EventType.Witch)
                {
                    GameManager.Of.SpawnWitch(_level.GetNextSpawn(_currentTurn));
                }
            }
        }

        WaitEndOfTurn();
    }

    async void WaitEndOfTurn()
    {
        await Awaitable.WaitForSecondsAsync(_durationTurn);

        var player = GameManager.Of.Player.CurrentTile.CellData.GridPosition;

        GameManager.Of.AddMovement(player);
        if (GameManager.Of.Player.CurrentTile.CellData.hasAMaterial)
        {
            GameManager.Of.Player.CurrentTile.RemoveMaterial();
            _player.AddMaterial();
        }

        foreach (var zombie in GameManager.Of.Zombies)
        {
            if (zombie.CurrentTile.CellData.HasATileTrap())
            {
                zombie.CurrentTile.CellData.ActivateTileTrap(zombie);
            }

            if (zombie.NextPath == GameManager.Of.Player.CurrentPath)
            {
                await Awaitable.WaitForSecondsAsync(1);
                _gameOverUI.GetComponent<ColorAnimation>().PlayAnimation();
                GameManager.Of.LoseLevel();
                return;
            }
        }

        int w = 0;
        foreach (var witch in GameManager.Of.Witches)
        {
            if (witch.CurrentTile.CellData.HasATileTrap())
            {
                witch.CurrentTile.CellData.ActivateTileTrap(witch);
            }
            if (witch.Path.Count >= 2)
            {
                if (_witchAllTilePaths[w].Contains(player))
                {
                    _gameOverUI.GetComponent<ColorAnimation>().PlayAnimation();
                    GameManager.Of.LoseLevel();
                    return;
                }
            }

            w++;
        }

        if (GameManager.Of.Echo)
        {
            if (GameManager.Of.Echo.CurrentTile.CellData.GridPosition == player)
            {
                _gameOverUI.GetComponent<ColorAnimation>().PlayAnimation();
                GameManager.Of.LoseLevel();
                return;
            }
        }

        // Attend la fin de la frame
        await Awaitable.EndOfFrameAsync();

        GameManager.Of.CheckEntityAlive();

        if (_currentTurn > _lastEventTurn)
        {
            if (GameManager.Of.Zombies.Count == 0 && GameManager.Of.Witches.Count == 0 && !GameManager.Of.Echo)
            {
                _gameWinUI.GetComponent<ColorAnimation>().PlayAnimation();
                GameManager.Of.WinLevel();
            }
        }

        CalculWitchPath();

        GameManager.Of.UpdateZOrder(false);
        if (_nbPass > 0)
        {
            PlayTurn();
        }
        else
        {
            _notNext = false;
            var playerPos = new List<Vector2Int> { GameManager.Of.Player.CurrentTile.CellData.GridPosition };
            CalculZombiePath(playerPos);
        }

        GameManager.Of.Grid.EntityInARoom();
    }

    public void CalculZombiePath(List<Vector2Int> playerPath)
    {
        WarningSpawn(playerPath.Count > 1 ? playerPath.Count - 1 : 1);
        _zombiePaths.Clear();
        ResetFeedback();
        int zIndex = 0;
        List<Vector2Int> stop = new List<Vector2Int>();;
        foreach (var zombie in GameManager.Of.Zombies)
        {
            if (GameManager.Of.Deads.Contains(zombie)) continue;
            _zombieFeedbacks.Add(new List<GameObject>());
            List<Vector2Int> diff = new List<Vector2Int>();
            List<Vector2Int> tiles = new List<Vector2Int> { zombie.CurrentTile.CellData.GridPosition };
            if (zombie.CanMove)
            {
                for (int i = 0; playerPath.Count > 1 ? i < playerPath.Count - 1 : i < playerPath.Count; i++)
                {
                    if (playerPath[i] == tiles[tiles.Count - 1]) break;

                    List<Vector2Int> newStop = new List<Vector2Int>(stop);
                    newStop.AddRange(tiles);
                    List<Vector2Int> path = AIPathfinding.CreatePath(GridManager.Tiles[tiles[tiles.Count - 1]], GridManager.Tiles[playerPath[i]], newStop);

                    if (path.Count <= 1) break;
                    bool pass = false;
                    foreach (var paths in _zombiePaths)
                    {
                        if (i + 1 >= paths.Count) continue;
                        if (paths[i + 1] == path[1])
                        {
                            diff.Add(Vector2Int.zero);
                            tiles.Add(tiles[tiles.Count - 1]);
                            pass = true;
                            break;
                        }
                    }
                    if (pass) continue;
                    diff.Add(path[1] - tiles[tiles.Count - 1]);
                    tiles.Add(path[1]);
                    if (playerPath.Count < i + 1)
                    {
                        if (path[1] == playerPath[i + 1] && i != 0)
                        {
                            break;
                        }
                    }
                }

                stop.Add(tiles[tiles.Count - 1]);

                Vector2Int lastDiff = Vector2Int.zero;

                int tileIndex = 1;
                foreach (var result in diff)
                {
                    if (lastDiff == Vector2Int.right)
                    {
                        if (result == Vector2Int.right)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Row, 0);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 0);
                        }
                        if (result == Vector2Int.up)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Turn, 0);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 90);
                        }
                        if (result == Vector2Int.down)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Turn, 0, false, true);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), -90);
                        }
                    }

                    if (lastDiff == Vector2Int.left)
                    {
                        if (result == Vector2Int.left)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Row, 0);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 180);
                        }
                        if (result == Vector2Int.up)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Turn, 0, true);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 90);
                        }
                        if (result == Vector2Int.down)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Turn, 0, true, true);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), -90);
                        }
                    }

                    if (lastDiff == Vector2Int.up)
                    {
                        if (result == Vector2Int.right)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Turn, 0, true, true);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 0);
                        }
                        if (result == Vector2Int.left)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Turn, 0, false, true);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 180);
                        }
                        if (result == Vector2Int.up)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Row, 90);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 90);
                        }
                    }

                    if (lastDiff == Vector2Int.down)
                    {
                        if (result == Vector2Int.right)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Turn, 0, true);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 0);
                        }
                        if (result == Vector2Int.left)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Turn, 0);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 180);
                        }
                        if (result == Vector2Int.down)
                        {
                            ModifyLastFeedback(zIndex, _zombiePathFeedback.Row, 90);
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), -90);
                        }
                    }

                    if (lastDiff == Vector2Int.zero)
                    {
                        if (result == Vector2Int.right)
                        {
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 0);
                        }
                        if (result == Vector2Int.left)
                        {
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 180);
                        }
                        if (result == Vector2Int.up)
                        {
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), 90);
                        }
                        if (result == Vector2Int.down)
                        {
                            SpawnFeedback(zIndex, _zombiePathFeedback.Arrow, tiles[tileIndex] + new Vector2(0.5f, 0.5f), -90);
                        }
                    }

                    lastDiff = result;
                    tileIndex++;
                }
            }

            zIndex++;
            _zombiePaths.Add(tiles);
        }
    }
    public void CalculWitchPath()
    {
        _witchFeedbacks = new List<List<GameObject>>();
        int wIndex = 0;
        _witchPaths.Clear();
        _witchAllTilePaths.Clear();
        ResetFeedback();
        foreach (Entity witch in GameManager.Of.Witches)
        {
            _witchFeedbacks.Add(new List<GameObject>());
            Vector2Int playerPos = GameManager.Of.Player.CurrentTile.CellData.GridPosition;
            Vector2Int witchPos = witch.CurrentTile.CellData.GridPosition;

            List<Vector2Int> path = new List<Vector2Int> { witchPos };
            List<Vector2Int> paths = new List<Vector2Int> { witchPos };
            Vector2Int incr = Vector2Int.zero;
            if (playerPos.x == witchPos.x)
            {
                incr = playerPos.y < witchPos.y ? Vector2Int.down : Vector2Int.up;
            }
            else if (playerPos.y == witchPos.y)
            {
                incr = playerPos.x < witchPos.x ? Vector2Int.left : Vector2Int.right;
            }

            bool search = incr == Vector2Int.zero ? false : true;
            while (search)
            {
                if (!GridManager.Tiles.ContainsKey(witchPos + incr))
                {
                    search = false;
                }
                else
                {
                    if (GridManager.Tiles[witchPos + incr].CellData.IsTheSameRoomOrHaveDoor(GridManager.Tiles[witchPos], GridManager.GetFace(incr)))
                    {
                        foreach (var zombie in GameManager.Of.Zombies)
                        {
                            if (zombie.Path[^1] == witchPos + incr)
                            {
                                search = false;
                                break;
                            }
                        }
                        witchPos += incr;
                        paths.Add(witchPos);
                        GameObject feed = Instantiate(_witchPathFeedback, ((Vector2)witchPos + Vector2.one / 2) * GameManager.Of.Grid.TileSize, Quaternion.identity);
                        _witchFeedbacks[wIndex].Add(feed);
                    }
                    else
                    {
                        search = false;
                    }
                }
            }

            _witchAllTilePaths.Add(paths);
            path.Add(witchPos);
            _witchPaths.Add(path);
            wIndex++;
        }
    }

    public void ZombieTurn()
    {
        ResetFeedback();
        int zombieIndex = 0;
        foreach (var zombie in GameManager.Of.Zombies)
        {
            if (_zombiePaths[zombieIndex].Count <= 1) continue;
            zombie.SetPath(_zombiePaths[zombieIndex]);
            zombieIndex++;
        }
    }

    public void WitchTurn()
    {
        
        _witchFeedbacks.Clear();
        int witchIndex = 0;
        foreach (var witch in GameManager.Of.Witches)
        {
            if (witchIndex < _witchFeedbacks.Count)
            {
                foreach (GameObject feed in _witchFeedbacks[witchIndex])
                {
                    Destroy(feed);
                }
            }

            if (_witchPaths[witchIndex].Count <= 1) continue;
            witch.SetPath(_witchPaths[witchIndex]);
            witchIndex++;
        }
    }
    
    void SpawnFeedback(int zombieIndex, Sprite sprite, Vector2 pos, float rotation, bool xFlip = false, bool yFlip = false)
    {
        GameObject feed = new GameObject("Zombie Feedback's");
        SpriteRenderer renderer = feed.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = "Foreground";
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
    public void ResetFeedback()
    {
        foreach (var feed in _zombieFeedbacks)
        {
            foreach (var go in feed)
            {
                Destroy(go);
            }
        }

        foreach (var feed in _witchFeedbacks)
        {
            foreach (var go in feed)
            {
                Destroy(go);
            }
        }

        _zombieFeedbacks.Clear();
        _witchFeedbacks.Clear();
    }

    void WarningSpawn(int turnWillMake)
    {
        foreach (var go in _warnings)
        {
            Destroy(go);
        }
        _warnings.Clear();

        foreach (var evnt in _level.Events)
        {
            if (evnt.EventType.HasFlag(EventType.Zombie) || evnt.EventType.HasFlag(EventType.Witch))
            {
                if (evnt.Turn <= _currentTurn + turnWillMake && evnt.Turn > _currentTurn)
                {
                    _warnings.Add(Instantiate(_spawnWarning, (Vector2)_level.GetNextSpawn(evnt.Turn) * GameManager.Of.Grid.TileSize + new Vector2(0.5f, 0.5f), Quaternion.identity));
                }
            }
        }
    }
}