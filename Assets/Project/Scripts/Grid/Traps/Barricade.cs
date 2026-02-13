using UnityEngine;

public class Barricade : Trap
{
    [SerializeField] private int _maxhealth = 2;
    private int _currentHealth;

    public override void ActivateTrap(Entity entity)
    {
        base.ActivateTrap(entity);
        if (entity != GameManager.Of.Player)
        {
            _currentHealth -= 1;
            if (_currentHealth <= 0)
            {
                DestroyTrap();
            }
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
