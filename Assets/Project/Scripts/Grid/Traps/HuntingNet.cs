using System.Threading.Tasks;
using UnityEngine;

public class HuntingNet : Trap
{
    public override async Awaitable ActivateTrap(Entity entity)
    {
        await base.ActivateTrap(entity);
        if (entity != GameManager.Of.Player)
        {
            GridManager.Tiles[_position].CellData.KillEntity();
            DestroyTrap();
        }
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
