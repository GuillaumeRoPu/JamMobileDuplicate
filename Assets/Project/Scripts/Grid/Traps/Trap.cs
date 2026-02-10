using System;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;
    private Vector2Int _position;
    public Action onDestroy;
    public Action onActivate;

    private void DestroyTrap()
    {
        onDestroy?.Invoke();
        Destroy(this.gameObject);
    }

    public virtual void ActivateTrap(Entity entity)
    {
        DestroyTrap();
    }

    public virtual void ActivateTrap()
    {
        DestroyTrap();
    }

    public virtual void PlaceTrap(Vector2Int position)
    {
        _position = position;
        _sprite.sortingOrder = 100;
    }

    [Button("ActivateTrap")]
    private void ActivateTrapButton()
    {
        ActivateTrap();
    }
}
