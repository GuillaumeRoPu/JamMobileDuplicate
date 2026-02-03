using System.Collections.Generic;
using UnityEngine;

public class AIPathfinding : MonoBehaviour
{
    Tile _target;

    List<List<Vector2Int>> _paths;

    Vector2Int[] _posTests = {new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
    Face[] _faceTests = {Face.Right, Face.Left, Face.Up, Face.Down };

    public void CreatePath(Tile target)
    {
        _target = target;
        _paths = new List<List<Vector2Int>>();
        _paths.Add(new List<Vector2Int>());
        _paths = GetAllPaths(new List<List<Vector2Int>>(_paths), 0, GetComponent<Entity>().CurrentTile);

        List<Vector2Int> selectPath = _paths[0];
        foreach (var path in _paths)
        {
            if (path.Count < selectPath.Count) selectPath = path;
        }
    }

    List<List<Vector2Int>> GetAllPaths(List<List<Vector2Int>> paths, int pathIndex, Tile nextTile)
    {
        paths[pathIndex].Add(nextTile.CellData.GridPosition);

        List<List<Vector2Int>> newPaths = new List<List<Vector2Int>>(paths);

        int pathCount = 0;
        for (int i = 0; i < 4; i++)
        {
            Tile tile = null;
            GridManager.Tiles.TryGetValue(nextTile.CellData.GridPosition + _posTests[i], out tile);

            if (tile.CellData.CanPass(_faceTests[i], nextTile.CellData))
            {
                if (tile == _target)
                {
                    paths[pathIndex].Add(tile.CellData.GridPosition);
                    break;
                }

                if (pathCount == 0)
                {
                    paths = GetAllPaths(new List<List<Vector2Int>>(paths), pathIndex, tile);
                }
                else
                {
                    paths.Add(new List<Vector2Int>(newPaths[pathIndex]));
                    paths = GetAllPaths(new List<List<Vector2Int>>(paths), paths.Count - 1, tile);
                }

                pathCount++;
            }
        }

        if (GridManager.Tiles[paths[pathIndex][paths[pathIndex].Count - 1]] == _target)
            return paths;
        else
            return newPaths;
    }
}