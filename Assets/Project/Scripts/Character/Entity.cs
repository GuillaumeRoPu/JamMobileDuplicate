using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    Tile _currentTile;
    public Tile CurrentTile => _currentTile;
    List<Vector2Int> _path = new List<Vector2Int>();
    public List<Vector2Int> Path => _path;
    bool _reachTarget = true;

    bool _isMoving;
    int _pathIndex;
    float _currentMoveTime;
    float _duration;

    public bool IsMoving => _isMoving;

    Vector3 _initPosition;

    System.Action _onTargetReached;

    Animator _animator;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void SetCurrentTile(Vector2Int tile)
    {
        if (_currentTile)
        {
            GameManager.Of.Grid.EntityInARoom(_currentTile.CellData.GridPosition, false);
            _currentTile.CellData.Entity = null;
        }
        _currentTile = GridManager.Tiles[tile];
        _currentTile.CellData.Entity = this;
        if (_currentTile.CellData.HasRoom)
            GameManager.Of.Grid.EntityInARoom(_currentTile.CellData.GridPosition);
    }

    public void SetPath(List<Vector2Int> path, System.Action WhenReached = null)
    {
        _path = path;
        _reachTarget = false;
        _pathIndex = 1;
        _currentMoveTime = 0;

        if (WhenReached != null)
        {
            _onTargetReached = WhenReached;
        }
    }

    void Update()
    {
        SendVelocity();

        if (!_reachTarget)
        {
            if (_isMoving)
            {
                float ratio = _currentMoveTime / _duration;

                transform.position = Vector3.Lerp(_initPosition, (Vector2)_path[_pathIndex] * GameManager.Of.Grid.TileSize, ratio);

                _currentMoveTime += Time.deltaTime;

                if (_currentMoveTime > _duration)
                {
                    SetCurrentTile(_path[_pathIndex]);
                    _isMoving = false;
                    _currentMoveTime = 0;
                    _pathIndex++;
                    if (_pathIndex == _path.Count)
                    {
                        _onTargetReached?.Invoke();
                        _reachTarget = true;
                    }
                }
            }
        }
    }

    public void MoveToNextTileInPath(float duration)
    {
        if (_reachTarget) return;

        _duration = duration;
        _initPosition = transform.position;
        _isMoving = true;
        Turn(_path[_pathIndex].x);
    }

    public void StopPath()
    {
        _reachTarget = true;
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

    // ANIMATIONS
    void SendVelocity()
    {
        _animator.SetBool("Move", _isMoving); 
    }

    void SendDead(bool net)
    {
        if (net)
        {
            _animator.SetTrigger("DeadNet");
        }
        else
        {
            _animator.SetTrigger("DeadLog");
        }

    }


}