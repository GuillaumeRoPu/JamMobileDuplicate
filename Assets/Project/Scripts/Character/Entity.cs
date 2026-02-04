using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    Tile _currentTile;
    List<Vector2Int> _path;
    bool _reachTarget = true;

    bool _isMoving;
    int _pathIndex;
    float _currentMoveTime;

    Vector3 _initPosition;

    public Tile CurrentTile => _currentTile;

    public void SetCurrentTile(Vector2Int tile)
    {
        _currentTile = GridManager.Tiles[tile];
    }

    public void SetPath(List<Vector2Int> path)
    {
        _path = path;
        _reachTarget = false;
        _isMoving = true;
        _pathIndex = 0;
        _currentMoveTime = 0;
        _initPosition = transform.position;
    }

    void Update()
    {
        if (!_reachTarget)
        {
            if (_isMoving)
            {
                float ratio = _currentMoveTime / 1;

                transform.position = Vector3.Lerp(_initPosition, (Vector2)_path[_pathIndex] * GameManager.Of.Grid.TileSize, ratio);

                _currentMoveTime += Time.deltaTime;

                if (_currentMoveTime > 1)
                {
                    _isMoving = false;
                    _currentMoveTime = 0;
                }
            }
            else
            {
                _pathIndex++;
                _initPosition = transform.position;
                _isMoving = true;

                if (_pathIndex == _path.Count) _reachTarget = true;
            }
        }
    }
}