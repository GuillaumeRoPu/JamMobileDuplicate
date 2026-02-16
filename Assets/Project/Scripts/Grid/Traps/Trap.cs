using System;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public enum TrapType
    {
        Single,
        Multiple,
        Door
    }

    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Face _face;
    [SerializeField] private TrapType _type;
    protected Vector2Int _position;
    private List<Vector2Int> _allPositions;
    [SerializeField] private List<Vector2Int> _additionalPositions;
    private List<Vector2Int> _additionalPositionsUpdated = new List<Vector2Int>();
    public Action onDestroy;
    public Action onActivate;

    public TrapType TrapMode => _type;
    public List<Vector2Int> AdditionalPosition => _additionalPositions;

    public void DestroyTrap()
    {
        onDestroy?.Invoke();
        Destroy(this.gameObject);
    }

    public virtual void ActivateTrap(Entity entity)
    {
        onActivate?.Invoke();
    }

    public virtual void TrapPlaced(Vector2Int position, Face face)
    {
        _position = position;
        if (_type == TrapType.Multiple)
        {
            var adpos = new List<Vector2Int>();
            Vector2Int offset = new Vector2Int();
            switch (face)
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
            foreach (var pos in _additionalPositions)
            {
                var newpos = pos;
                switch (face)
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
                adpos.Add(newpos + _position);
            }
            _additionalPositions = adpos;
        }
    }

    [Button("ActivateTrap")]
    private void ActivateTrapButton()
    {
        ActivateTrap(new Entity());
    }

    private void OnDrawGizmosSelected()
    {
        if (TrapMode == TrapType.Multiple)
        {
            var offset = -transform.localPosition;
            Gizmos.color = Color.blueViolet;
            Gizmos.DrawWireSphere(new Vector2(0, 0), 0.25f);
            foreach (var pos in _additionalPositions)
            {
                var newpos = pos;
                switch (_face)
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
                Gizmos.DrawWireSphere((Vector2)newpos + (Vector2)transform.position + (Vector2)offset, 0.25f);
            }
            switch (_face)
            {
                case Face.Up:
                    break;
                case Face.Down:
                    break;
                case Face.Left:
                    break;
                case Face.Right:
                    break;
                default:
                    break;
            }
        }
    }
}
