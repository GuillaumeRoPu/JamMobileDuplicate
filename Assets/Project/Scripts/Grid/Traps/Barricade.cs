using UnityEngine;

public class Barricade : Trap
{
    [SerializeField] private int _maxhealth = 2;
    private int _currentHealth;

    public override async Awaitable ActivateTrap(Entity entity)
    {
        await base.ActivateTrap(entity);
        _currentHealth -= 1;
        if (_currentHealth <= 0)
        {
            GridManager.Tiles[_position].GiveTileAMaterial();
            AudioManager.Of.PlayClipAt(AudioManager.Of.fx[3], Vector3.zero, true);

            DestroyTrap();
        }
    }

    public override void TrapPlaced(Vector2Int position, Face face)
    {
        base.TrapPlaced(position, face);
        _currentHealth = _maxhealth;
    }

    [Button("ActivateTrap")]
    private void ActivateTrapButton()
    {
        ActivateTrap(new Entity());
    }

    [Button("DestroyTrap")]
    private void DestroyTrapButton()
    {
        DestroyTrap();
    }
}
