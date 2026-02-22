using UnityEngine;

public class Log : Trap
{
    public override async Awaitable ActivateTrap(Entity entity)
    {
        if (entity.CurrentTile.CellData.GridPosition == _position && entity != GameManager.Of.Player)
        {
            await base.ActivateTrap(entity);

            GetComponent<Animator>().SetTrigger("Active");

            await Awaitable.WaitForSecondsAsync(0.5f);
        }
    }

    public void KillEntities()
    {
        GridManager.Tiles[_position].CellData.KillEntity();

        int nbEntity = 1;
        foreach (var pos in AdditionalPosition)
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
    }

    public override void TrapPlaced(Vector2Int position, Face face)
    {
        base.TrapPlaced(position, face);
    }
}
