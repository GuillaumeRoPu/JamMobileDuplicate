using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    Tile _currentTile;
    List<Vector2Int> _path;
    bool _reachTarget;

    bool _isMoving;
    int _pathIndex;
    float _currentMoveTime;

    Vector3 _initPosition;

    public Tile CurrentTile => _currentTile;

    public void SetPath(List<Vector2Int> path)
    {
        _path = path;
        _reachTarget = false;
        _isMoving = true;
        _pathIndex = 0;

    }

    void Update()
    {
        if (!_reachTarget)
        {
            if (_isMoving)
            {
                float ratio = _currentMoveTime / 1;

                transform.position = Vector3.Lerp(_initPosition, GridManager.Tiles[_path[_pathIndex]].transform.position, ratio);

                _currentMoveTime += Time.deltaTime;

                if (_currentMoveTime > 1)
                {
                    _isMoving = false;
                }
            }
            else
            {
                _pathIndex++;
                _initPosition = transform.position;

                if (_pathIndex == _path.Count) _reachTarget = true;
            }
        }
    }
}