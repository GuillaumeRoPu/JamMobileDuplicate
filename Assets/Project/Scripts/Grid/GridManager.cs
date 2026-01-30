using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [SerializeField] private bool _generateGridOnPlay;
    [SerializeField] private GridLevelData _gridLevelData;

    [SerializeField] private Tile _tilesPrefab;
    [SerializeField] private Transform _tilesParent;

    [SerializeField] private float _tilesSize;

    [SerializeField] private Vector2Int _limits;

    [SerializeField] private Transform _camera;

    [SerializeField] private bool _coolColors;
    [SerializeField] private bool _coolColorsAnimated;

    //ALL THE DATA FOR THE GRID IS HERE

    private static Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();

    //END OF ALL THE DATA

    //Fills the whole grid with tiles depending on width and height, as well as the origin
    void GenerateGrid(int width, int height, Vector2Int origin)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int tilePosition = new Vector2Int(x + origin.x, y + origin.y);
                AddTile(tilePosition);
                if (_coolColors || _coolColorsAnimated)
                {
                    Color newcol = new Color(Mathf.Lerp(0, 1, x / ((float)width - 1)) * 1f, 1 * 0.5f, Mathf.Lerp(0, 1, y / ((float)height - 1)) * 1f);
                    if (_tiles.ContainsKey(tilePosition))
                    {
                        _tiles[tilePosition].SetColor(newcol);
                    }
                }
            }
        }
        UpdateSizeAll();
    }
    void GenerateGrid(List<Vector2Int> tiles)
    {
        foreach (var tile in tiles)
        {
            AddTile(tile);
        }
        UpdateSizeAll();

    }

    //Erases all the tiles in the grid depending on width and height, as well as the origin
    void EraseGrid(int width, int height, Vector2Int origin)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int tilePosition = new Vector2Int(x + origin.x, y + origin.y);
                RemoveTile(tilePosition);
            }
        }
        UpdateSizeAll();
    }

    void GenerateTiles(List<Vector2Int> tiles, Vector2Int origin)
    {
        foreach (var tile in tiles)
        {
            AddTile(tile + origin);
        }
    }

    void RelocateCamera(int width, int height, Vector2Int origin)
    {
        _camera.transform.position = new Vector3((float)width / 2 + origin.x, (float)height / 2 + origin.y, _camera.transform.position.z);
    }

    void UpdateSize(Vector2Int position)
    {
        _tiles[position].transform.position = (Vector2)_tiles[position].transform.position * _tilesSize;
        _tiles[position].transform.localScale = new Vector3(_tilesSize, _tilesSize, _tilesSize);
    }

    private void Start()
    {
        Debug.Log(_tiles.Count);
        if (_generateGridOnPlay)
        {
            GenerateGrid(_gridLevelData._allTilesPositions);
        }
    }

    public Tile GetTileAtPosition(Vector2Int position)
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
        if (_coolColorsAnimated)
        {
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

    public void AddTile(Vector2Int position)
    {
        if (_tiles.ContainsKey(position) || Mathf.Abs(position.x) > _limits.x || Mathf.Abs(position.y) > _limits.y)
        {
            Debug.LogWarning("There's Already A Tile Here or Its Out Of Bounds");
        }
        else
        {
            var spawnedTile = Instantiate(_tilesPrefab, new Vector2((float)position.x + 0.5f, (float)position.y + 0.5f), Quaternion.identity, _tilesParent);
            spawnedTile.name = $"Tile X:{position.x}, Y:{position.y}";
            var isOffset = ((position.x % 2 == 0 && position.y % 2 != 0) || (position.x % 2 != 0 && position.y % 2 == 0));
            if (position == _gridLevelData._spawnTile)
            {
                spawnedTile.SetAsSpawnTile(true);
            }
            spawnedTile.Initialize(isOffset, position);
            _tiles[position] = spawnedTile;
            UpdateSize(position);
        }
    }

    public void RemoveTile(Vector2Int position)
    {
        if (_tiles.ContainsKey(position))
        {
            _tiles[position].AnnihilateTile();
            _tiles.Remove(position);
        }
        else
        {
            Debug.LogWarning("There Isn't A Tile Here");
        }
    }

    [Button("Update Grid Size")]
    void UpdateSizeAll()
    {
        if (_tiles != null)
        {
            foreach (var tile in _tiles)
            {
                tile.Value.transform.position = new Vector2(tile.Key.x * _tilesSize + 0.5f * _tilesSize, tile.Key.y * _tilesSize + 0.5f * _tilesSize);
                tile.Value.transform.localScale = new Vector3(_tilesSize, _tilesSize, _tilesSize);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellowNice;
        Gizmos.DrawWireCube(new Vector2(0.5f *_tilesSize, 0.5f * _tilesSize), new Vector2((_limits.x * 2) * _tilesSize + 1 * _tilesSize, (_limits.y * 2) * _tilesSize + 1 * _tilesSize));
        Gizmos.color = Color.cyan;
        foreach (var tile in _gridLevelData._allTilesPositions)
        {
            Gizmos.DrawWireCube(new Vector2((0.5f + tile.x) * _tilesSize, (0.5f + tile.y) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
        }
    }
}
