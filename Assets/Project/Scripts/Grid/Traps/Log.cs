using System.Collections.Generic;
using UnityEngine;

public class Log : Trap
{
    public override void ActivateTrap(Entity entity)
    {
        base.ActivateTrap(entity);
        if (entity.CurrentTile.CellData.GridPosition == _position && entity != GameManager.Of.Player)
        {
            GridManager.Tiles[_position].CellData.KillEntity();

            int nbEntity = 1;
            foreach(var pos in AdditionalPosition)
            {
                if (GridManager.Tiles[pos].CellData.Entities.Count > 0)
                {
                    nbEntity++;
                }
                GridManager.Tiles[pos].CellData.KillEntity();
            }

            if (nbEntity == 1)
            {
                PlayGameManager.Of.UnlockAchievement(GPGSIds.achievement_overkill);
            }
            else if (nbEntity == 3)
            {
                PlayGameManager.Of.UnlockAchievement(GPGSIds.achievement_strike);
            }
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
