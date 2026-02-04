using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [SerializeField] private bool _generateGridOnPlay;
    [SerializeField] private GridLevelData _gridLevelData;

    [SerializeField] private Tile _tilesPrefab;
    [SerializeField] private Transform _tilesParent;

    [SerializeField] private float _tilesSize;

    private Vector2Int _limitsMin;
    private Vector2Int _limitsMax;

    [SerializeField] private Transform _camera;

    [SerializeField] private bool _coolColors;
    [SerializeField] private bool _coolColorsAnimated;

    //ALL THE DATA FOR THE GRID IS HERE

    private static Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();

    public static Dictionary<Vector2Int, Tile> Tiles => _tiles;


    //END OF ALL THE DATA

    //Fills the whole grid with tiles depending on width and height, as well as the origin

    void GenerateGrid(List<Vector2Int> tiles)
    {
        foreach (var tile in tiles)
        {
            AddTile(tile);
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
        foreach (var help in _gridLevelData.spawnPointsTiles)
        {
            Debug.Log("X : " + help.Key.x + ", Y : " + help.Key.y);
        }
        if (_generateGridOnPlay && _gridLevelData != null)
        {
            _limitsMax = _gridLevelData.limitsMax;
            _limitsMin = _gridLevelData.limitsMin;
            GenerateGrid(_gridLevelData.allTilesPosition);
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
        if (_tiles.ContainsKey(position) || (position.x > _limitsMax.x || position.x < _limitsMin.x) || (position.y > _limitsMax.y || position.y < _limitsMin.y))
        {
            Debug.LogWarning("There's Already A Tile Here or Its Out Of Bounds");
        }
        else
        {
            var spawnedTile = Instantiate(_tilesPrefab, new Vector2((float)position.x + 0.5f, (float)position.y + 0.5f), Quaternion.identity, _tilesParent);
            spawnedTile.name = $"Tile X:{position.x}, Y:{position.y}";
            var isOffset = ((position.x % 2 == 0 && position.y % 2 != 0) || (position.x % 2 != 0 && position.y % 2 == 0));
            if (_gridLevelData.spawnPointsTiles.ContainsKey(position))
            {
                spawnedTile.SetAsSpawnTile(_gridLevelData.spawnPointsTiles[position]);
                Debug.Log("HELP");
                if (_gridLevelData.spawnPointsTiles[position] == CellData.SpawnPointType.Player)
                {
                    Debug.Log("X :" + position.x + "Y :" + position.y);
                }
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
        if (_gridLevelData != null) 
        {
            foreach (var tile in _gridLevelData.allTilesPosition)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(new Vector2((0.5f + tile.x) * _tilesSize, (0.5f + tile.y) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
            }
            foreach (var tile in _gridLevelData.spawnPointsTiles)
            {
                if (_gridLevelData.spawnPointsTiles[tile.Key].HasFlag(CellData.SpawnPointType.Player))
                {
                    Gizmos.color = Color.orangeRed;
                    Gizmos.DrawWireCube(new Vector2((0.5f + tile.Key.x) * _tilesSize, (0.5f + tile.Key.y) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
                }
                else if (_gridLevelData.spawnPointsTiles[tile.Key] != CellData.SpawnPointType.None)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(new Vector2((0.5f + tile.Key.x) * _tilesSize, (0.5f + tile.Key.y) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
                }
            }
            Gizmos.color = Color.yellowNice;
            Gizmos.DrawWireCube(new Vector2((0.5f + (float)(_gridLevelData.limitsMax.x + _gridLevelData.limitsMin.x) / 2) * _tilesSize, (0.5f + (float)(_gridLevelData.limitsMax.y + _gridLevelData.limitsMin.y) / 2) * _tilesSize), new Vector2((float)(Mathf.Abs(_gridLevelData.limitsMax.x) + Mathf.Abs(_gridLevelData.limitsMin.x) + 1) * _tilesSize, (float)(Mathf.Abs(_gridLevelData.limitsMax.y) + Mathf.Abs(_gridLevelData.limitsMin.y) + 1) * _tilesSize));
        }
    }
}
