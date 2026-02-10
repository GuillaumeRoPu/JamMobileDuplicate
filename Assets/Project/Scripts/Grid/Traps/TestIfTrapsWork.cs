using UnityEngine;

public class TestIfTrapsWork : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private Trap _trap;
    [SerializeField] private Vector2Int _trapPosition;

    [Button("Place Trap")]
    private void PlaceTrapButton()
    {
        if (GridManager.Tiles.ContainsKey(_trapPosition) && _trap != null)
        {
            if (!GridManager.Tiles[_trapPosition].CellData.HasATrap())
            {
                var trap = Instantiate(_trap, (Vector2)_trapPosition + new Vector2(0.5f, 0.5f), Quaternion.identity);
                trap.PlaceTrap(_trapPosition);
                GridManager.Tiles[_trapPosition].CellData.SetTrap(trap);
            }
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)_trapPosition + new Vector2(0.5f, 0.5f), 0.25f);
    }
}
