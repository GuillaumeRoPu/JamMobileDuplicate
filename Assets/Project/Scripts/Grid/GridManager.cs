using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width;
    [SerializeField] private int _height;

    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _tileParent;

    [SerializeField] private Transform _camera;

    private Dictionary<Vector2, Tile> _tiles = new Dictionary<Vector2, Tile>();

    void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x + 0.5f, y + 0.5f), Quaternion.identity, _tileParent);
                spawnedTile.name = $"Tile X:{x}, Y:{y}";

                var isOffset = ((x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0));
                Vector2 tilePosition = new Vector2(x, y);
                spawnedTile.Initialize(isOffset, tilePosition);
                Color newcol = new Color(Mathf.Lerp(0, 1, x / ((float)_width - 1)) * 1f, 1*0.5f, Mathf.Lerp(0, 1, y / ((float)_height - 1)) * 1f);
                spawnedTile.SetColor(newcol);
                _tiles[tilePosition] = spawnedTile;
                Debug.Log(_tiles.Count);
            }
        }

        _camera.transform.position = new Vector3((float)_width / 2, (float)_height / 2, _camera.transform.position.z);
    }

    private void Start()
    {
        GenerateGrid();
    }

    public Tile GetTileAtPosition(Vector2 position)
    {
        if (_tiles.TryGetValue(position, out var tile))
        {
            return tile; 
        } 
        return null;
    }

    private void Update()
    {
        // Visual Flair, is getting removed at some point
        foreach (var tile in _tiles.Values)
        {
            Color oldcol = tile.GetColor();
            Color.RGBToHSV(oldcol, out var H, out var S, out var V);
            H += 1 * Time.deltaTime;
            if (H >= 1)
            {
                H -= 1;
            }
            tile.SetColor(Color.HSVToRGB(H, S, V));
        }
    }
}
