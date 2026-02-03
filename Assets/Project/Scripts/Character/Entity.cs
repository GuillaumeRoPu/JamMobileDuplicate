using UnityEngine;

public class Entity : MonoBehaviour
{
    Tile _currentTile;

    public void Move(Tile tile)
    {
        transform.position = tile.transform.position;
        _currentTile = tile;
    }
}
