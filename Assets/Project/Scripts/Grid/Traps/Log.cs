using System.Collections.Generic;
using UnityEngine;

public class Log : Trap
{
    public override void ActivateTrap(Entity entity)
    {
        base.ActivateTrap(entity);
        if (entity.CurrentTile.CellData.GridPosition != _position)
        {

        }
    }

    public override void TrapPlaced(Vector2Int position, Face face)
    {
        base.TrapPlaced(position, face);

    }
}
