using System.Collections.Generic;
using UnityEngine;

public class Log : Trap
{
    public override void ActivateTrap(Entity entity)
    {
        base.ActivateTrap(entity);
        if (entity.CurrentTile.CellData.GridPosition == _position)
        {
            DestroyTrap();
        }
    }

    public override void TrapPlaced(Vector2Int position, Face face)
    {
        base.TrapPlaced(position, face);
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
