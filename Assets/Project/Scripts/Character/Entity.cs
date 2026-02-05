using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Entity : MonoBehaviour
{
    Tile _currentTile;
    List<Vector2Int> _path;
    bool _reachTarget = true;

    bool _isMoving;
    int _pathIndex;
    float _currentMoveTime;

    public bool IsMoving => _isMoving;

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
        _pathIndex = 0;
        _currentMoveTime = 0;
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
                    _currentTile = GridManager.Tiles[_path[_pathIndex]];
                    _isMoving = false;
                    _currentMoveTime = 0;
                }
            }
        }
    }

    public void MoveToNextTileInPath()
    {
        if (_reachTarget) return;

        _pathIndex++;
        _initPosition = transform.position;
        _isMoving = true;

        if (_pathIndex == _path.Count) _reachTarget = true;
        else Turn(_path[_pathIndex].x);
    }

    void Turn(float xDirection)
    {
        Transform spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;
        if ((xDirection < _currentTile.CellData.GridPosition.x && spriteTransform.localScale.x > 0) || (xDirection > _currentTile.CellData.GridPosition.x && spriteTransform.localScale.x < 0))
        {
            Vector3 scale = spriteTransform.localScale;
            scale.x *= -1;
            spriteTransform.localScale = scale;
        }
    }
}