using Frynity.Utils;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathFeedback
{
    public Sprite Arrow;
    public Sprite Row;
    public Sprite Turn;
}

public class AIPathfinding
{
    static Tile _target;

    static List<Vector2Int> _passedTile;
    static List<List<Vector2Int>> _paths;
    static List<Vector2Int> _notPassOn;

    static Vector2Int[] _posTests = {new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
    static Face[] _faceTests = {Face.Up, Face.Down, Face.Left, Face.Right };

    public static List<Vector2Int> CreatePath(Tile depart, Tile target, List<Vector2Int> notPassOn = null)
    {
        _target = target;
        _paths = new List<List<Vector2Int>>() { new List<Vector2Int>() };
        _notPassOn = notPassOn;

        _passedTile = new List<Vector2Int>();

        if (depart == null)
        {
            Debug.LogError("CURRENT TILE IS NULL !!!");
            return null;
        }

        _paths = GetAllPaths(new List<List<Vector2Int>>(_paths), 0, depart);

        List<Vector2Int> selectPath = _paths[0];
        foreach (var path in _paths)
        {
            if (path[^1] != target.CellData.GridPosition) continue;

            if (selectPath[^1] != target.CellData.GridPosition)
            {
                selectPath = path;
                continue;
            }

            if (path.Count < selectPath.Count) selectPath = path;
        }

        //foreach (var oh in selectPath)
        //{
        //    oh.Log();
        //}

        return selectPath;
    }

    static List<List<Vector2Int>> GetAllPaths(List<List<Vector2Int>> paths, int pathIndex, Tile nextTile)
    {
        _passedTile.Add(nextTile.CellData.GridPosition);
        paths[pathIndex].Add(nextTile.CellData.GridPosition);

        List<Vector2Int> newPaths = new(paths[pathIndex]);
        int lol = paths.Count;

        List<Tile> searchTiles = new List<Tile>();
        int lastDistance = Mathf.Abs(_target.CellData.GridPosition.x - nextTile.CellData.GridPosition.x)
             + Mathf.Abs(_target.CellData.GridPosition.y - nextTile.CellData.GridPosition.y);
        for (int i = 0; i < 4; i++)
        {
            Tile tile = null;
            GridManager.Tiles.TryGetValue(nextTile.CellData.GridPosition + _posTests[i], out tile);

            if (tile == null) continue;

            if (paths[pathIndex].Contains(tile.CellData.GridPosition)) continue;

            if (_notPassOn != null)
            {
                if (_notPassOn.Contains(tile.CellData.GridPosition)) continue;
            }

            if (tile.CellData.CanPass(_faceTests[i], nextTile.CellData))
            {
                if (tile == _target)
                {
                    paths[pathIndex].Add(tile.CellData.GridPosition);
                    searchTiles.Clear();
                    break;
                }
                else
                {
                    int distance = Mathf.Abs(_target.CellData.GridPosition.x - tile.CellData.GridPosition.x)
             + Mathf.Abs(_target.CellData.GridPosition.y - tile.CellData.GridPosition.y);

                    if (distance < lastDistance)
                    {
                        lastDistance = distance;
                        searchTiles.Clear();
                        searchTiles.Add(tile);
                    }
                    else if (distance == lastDistance || searchTiles.Count == 0)
                    {
                        lastDistance = distance;
                        searchTiles.Add(tile);
                    }
                }
            }
        }

        int pathCount = 0;
        foreach (Tile tile in searchTiles)
        {
            int index = pathIndex;

            if (pathCount == 0)
            {
                GetAllPaths(paths, index, tile);
            }
            else
            {
                index = paths.Count;
                paths.Add(new List<Vector2Int>(newPaths));
                GetAllPaths(paths, index, tile);
            }
            pathCount++;
        }

        return paths;
    }
}