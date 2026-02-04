using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UIElements;

public class GridDrawer : MonoBehaviour
{
    [SerializeField, Foldout("Configuration")] private Tile _tilesPrefab;
    [SerializeField, Foldout("Configuration")] private Transform _tilesParent;

    [SerializeField, Foldout("Configuration")] private float _tilesSize;

    [SerializeField, MaxValue(0), Foldout("Configuration")] private int _limitsMinX;
    [SerializeField, MinValue(0), Foldout("Configuration")] private int _limitsMaxX;
    [SerializeField, MaxValue(0), Foldout("Configuration")] private int _limitsMinY;
    [SerializeField, MinValue(0), Foldout("Configuration")] private int _limitsMaxY;

    [SerializeField, Foldout("Draw Tool")] private int _drawGridWidth;
    [SerializeField, Foldout("Draw Tool")] private int _drawGridHeight;
    [SerializeField, Foldout("Draw Tool")] private Vector2Int _drawGridOrigin;

    [Space(5)]
    [SerializeField, Foldout("Draw Tool")] private int _eraseGridWidth;
    [SerializeField, Foldout("Draw Tool")] private int _eraseGridHeight;
    [SerializeField, Foldout("Draw Tool")] private Vector2Int _eraseGridOrigin;

    [Space(5)]
    [SerializeField, Foldout("Draw Tool"), Tooltip("")] private string _gridLevelName;

    [Space(5)]
    [SerializeField, Foldout("Draw Tool"), Tooltip("The button 'Generate Grid from Data' can create Tiles using the grid's shape")] private GridLevelData _gridLevelData;

    [Space(5)]
    [SerializeField, Foldout("Draw Tool")] private Vector2Int _spawnPointPosition;
    [SerializeField, Foldout("Draw Tool")] private CellData.SpawnPointType _spawnPointTypes;


    private static Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();
    private static Dictionary<Vector2Int, CellData.SpawnPointType> _spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();

    //Fills the whole grid with tiles depending on width and height, as well as the origin
    void GenerateGrid(int width, int height, Vector2Int origin)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int tilePosition = new Vector2Int(x + origin.x, y + origin.y);
                AddTile(tilePosition);
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

    void UpdateSize(Vector2Int position)
    {
        _tiles[position].transform.position = (Vector2)_tiles[position].transform.position * _tilesSize;
        _tiles[position].transform.localScale = new Vector3(_tilesSize, _tilesSize, _tilesSize);
    }

    private void Start()
    {
        Debug.Log(_tiles.Count);
        RemoveAllTiles();
    }

    public Tile GetTileAtPosition(Vector2Int position)
    {
        if (_tiles.TryGetValue(position, out var tile))
        {
            return tile;
        }
        return null;
    }

    public void AddTile(Vector2Int position)
    {
        if (_tiles.ContainsKey(position) || (position.x > _limitsMaxX || position.x < _limitsMinX) || (position.y > _limitsMaxY || position.y < _limitsMinY))
        {
            Debug.LogWarning("There's Already A Tile Here or Its Out Of Bounds");
        }
        else
        {
            var spawnedTile = Instantiate(_tilesPrefab, new Vector2((float)position.x + 0.5f, (float)position.y + 0.5f), Quaternion.identity, _tilesParent);
            spawnedTile.name = $"Tile X:{position.x}, Y:{position.y}";
            var isOffset = ((position.x % 2 == 0 && position.y % 2 != 0) || (position.x % 2 != 0 && position.y % 2 == 0));
            spawnedTile.Initialize(isOffset, position);
            _tiles[position] = spawnedTile;
            UpdateSize(position);
            Color newcol = new Color(Mathf.Lerp(0, 1, (float)(position.x + Mathf.Abs(_limitsMinX)) / (float)(Mathf.Abs(_limitsMinX) + _limitsMaxX)), 0.5f, Mathf.Lerp(0, 1, (float)(position.y + Mathf.Abs(_limitsMinY)) / (float)(Mathf.Abs(_limitsMinY) + _limitsMaxY)));
            Debug.Log(newcol.r);
            if (_tiles.ContainsKey(position))
            {
                _tiles[position].SetColor(newcol);
            }
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
            if (Application.isPlaying)
            {
                Debug.Log("There Isn't A Tile Here : X - " + position.x + " | Y - " + position.y);
            }
        }
    }

    [Button("Generate Tiles (Green)")]
    public void GenerateTilesButton()
    {
        GenerateGrid(_drawGridWidth, _drawGridHeight, _drawGridOrigin);
    }

    [Button("Generate Grid From Data (Blue)")]
    public void GenerateGridDataButton()
    {
        if (_gridLevelData != null)
        {
            GenerateGrid(_gridLevelData.allTilesPosition);
            if (_gridLevelData.spawnPointsTiles != null)
            {
                foreach (var spawn in _gridLevelData.spawnPointsTiles)
                {
                    if (_tiles.ContainsKey(spawn.Key))
                    {
                        if (spawn.Value.HasFlag(CellData.SpawnPointType.Player))
                        {
                            _tiles[spawn.Key].RemoveSpawnTile();
                            Color newcol = new Color(Mathf.Lerp(0, 1, (float)spawn.Key.x + (float)Mathf.Abs(_limitsMinX) / ((float)Mathf.Abs(_limitsMinX) + (float)_limitsMaxX)), 0.5f, Mathf.Lerp(0, 1, (float)spawn.Key.y + (float)Mathf.Abs(_limitsMinY) / ((float)Mathf.Abs(_limitsMinY) + (float)_limitsMaxY)));
                            _tiles[spawn.Key].SetColor(newcol);
                        }
                    }
                    _tiles[spawn.Key].SetAsSpawnTile(spawn.Value);
                    if (_tiles[spawn.Key].CellData.spawnPointType != CellData.SpawnPointType.None)
                    {
                        _tiles[spawn.Key].RemoveSpawnTile();
                    }
                    _spawnPointsTiles[spawn.Key] = spawn.Value;
                    _tiles[spawn.Key].SetAsSpawnTile(spawn.Value);
                }
            }
        }
    }

    [Button("Erase Tiles (Red)")]
    public void EraseGridButton()
    {
        EraseGrid(_eraseGridWidth, _eraseGridHeight, _eraseGridOrigin);
    }

    [Button("Make Tile A Spawn Point (Orange)")]
    public void SetAsSpawnPoint()
    {
        if (_tiles.ContainsKey(_spawnPointPosition))
        {
            if (_spawnPointTypes.HasFlag(CellData.SpawnPointType.Player))
            {
                foreach (var spawn in _spawnPointsTiles)
                {
                    if (spawn.Value.HasFlag(CellData.SpawnPointType.Player))
                    {
                        _tiles[spawn.Key].RemoveSpawnTile();
                        Color newcol = new Color(Mathf.Lerp(0, 1, (float)spawn.Key.x + (float)Mathf.Abs(_limitsMinX) / ((float)Mathf.Abs(_limitsMinX) + (float)_limitsMaxX)), 0.5f, Mathf.Lerp(0, 1, (float)spawn.Key.y + (float)Mathf.Abs(_limitsMinY) / ((float)Mathf.Abs(_limitsMinY) + (float)_limitsMaxY)));
                        if (_tiles.ContainsKey(spawn.Key))
                        {
                            _tiles[spawn.Key].SetColor(newcol);
                        }
                        _spawnPointsTiles.Remove(spawn.Key);
                        break;
                    }
                }
            }
            if (_spawnPointTypes == CellData.SpawnPointType.None)
            {
                _tiles[_spawnPointPosition].RemoveSpawnTile();
                Color newcol = new Color(Mathf.Lerp(0, 1, (float)_spawnPointPosition.x + (float)Mathf.Abs(_limitsMinX) / ((float)Mathf.Abs(_limitsMinX) + (float)_limitsMaxX)), 0.5f, Mathf.Lerp(0, 1, (float)_spawnPointPosition.y + (float)Mathf.Abs(_limitsMinY) / ((float)Mathf.Abs(_limitsMinY) + (float)_limitsMaxY)));
                if (_tiles.ContainsKey(_spawnPointPosition))
                {
                    _tiles[_spawnPointPosition].SetColor(newcol);
                }
            }
            else
            {
                if (_tiles[_spawnPointPosition].CellData.spawnPointType != CellData.SpawnPointType.None)
                {
                    _tiles[_spawnPointPosition].RemoveSpawnTile();
                }
                _spawnPointsTiles[_spawnPointPosition] = _spawnPointTypes;
                _tiles[_spawnPointPosition].SetAsSpawnTile(_spawnPointTypes);
            }
        }
        else
        {
            Debug.LogWarning("Doesn't Exist bro :sob:");
        }
    }

    [Button("Update Grid Size")]
    void UpdateSizeAll()
    {
        foreach (var tile in _tiles)
        {
            tile.Value.transform.position = new Vector2(tile.Key.x * _tilesSize + 0.5f * _tilesSize, tile.Key.y * _tilesSize + 0.5f * _tilesSize);
            tile.Value.transform.localScale = new Vector3(_tilesSize, _tilesSize, _tilesSize);
        }
    }

    [Button("Erase All Tiles")]
    public void RemoveAllTiles()
    {
        List<Vector2Int> list = new List<Vector2Int>();
        foreach (var tile in _tiles.Keys)
        {
            list.Add(tile);
        }
        foreach (var tile in list)
        {
            RemoveTile(tile);
        }
        _tiles.Clear();

        Tile[] tiles = _tilesParent.GetComponentsInChildren<Tile>();
        foreach (var tile in tiles)
        {
            tile.AnnihilateTile();
        }

        _spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();
    }

    [Button("Save The Grid layout")]
    public void CommitValue()
    {
#if UNITY_EDITOR
        var data = GridLevelDataBase.GetOrCreate(_gridLevelName);

        Undo.RecordObject(data, "Commit Tool Value");

        //WRITE DATA YOU WANT TO SAVE

        List<Vector2Int> valueToSave = new List<Vector2Int>();
        Vector2Int _limitsMin = new Vector2Int();
        Vector2Int _limitsMax = new Vector2Int();
        foreach (var tile in _tiles.Keys)
        {
            valueToSave.Add(tile);
            if(_limitsMin.x > tile.x)
            {
                _limitsMin.x = tile.x;
            }
            if (_limitsMin.y > tile.y)
            {
                _limitsMin.y = tile.y;
            }
            if (_limitsMax.x < tile.x)
            {
                _limitsMax.x = tile.x;
            }
            if (_limitsMax.y < tile.y)
            {
                _limitsMax.y = tile.y;
            }
        }
        data.allTilesPosition = valueToSave;
        data.spawnPointsTiles = _spawnPointsTiles;
        foreach (var value in _spawnPointsTiles)
        {
            data.spawnPointsTilesPosition.Add(value.Key);
            data.spawnPointsTilesTypes.Add(value.Value);
        }
        data.limitsMin = _limitsMin;
        data.limitsMax = _limitsMax;
        Debug.Log("Min X : " + data.limitsMin.x + " | Min Y : " + data.limitsMin.y + " | Max X : " + data.limitsMax.x + " | Max Y : " + data.limitsMin.y);

        //END OF WRITING DATA EZ

        GridLevelDataBase.Save(data);

        RemoveAllTiles();
#endif
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellowNice;
        Gizmos.DrawWireCube(new Vector2((0.5f + (float)(_limitsMaxX + _limitsMinX) / 2) * _tilesSize, (0.5f + (float)(_limitsMaxY + _limitsMinY) / 2) * _tilesSize), new Vector2((float)(Mathf.Abs(_limitsMaxX) + Mathf.Abs(_limitsMinX) + 1) * _tilesSize, (float)(Mathf.Abs(_limitsMaxY) + Mathf.Abs(_limitsMinY) + 1) * _tilesSize));
        if (_gridLevelData != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var tile in _gridLevelData.allTilesPosition)
            {
                Gizmos.DrawWireCube(new Vector2((0.5f + tile.x) * _tilesSize, (0.5f + tile.y) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
            }
            //Gizmos.color = Color.softYellow;
            //Gizmos.DrawWireCube(new Vector2((0.5f + _gridLevelData.spawnTile.x) * _tilesSize, (0.5f + _gridLevelData.spawnTile.y) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector2((_drawGridOrigin.x + (float)_drawGridWidth / 2) * _tilesSize, (_drawGridOrigin.y + (float)_drawGridHeight / 2) * _tilesSize), new Vector3(_drawGridWidth * _tilesSize, _drawGridHeight * _tilesSize, 1));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2((_eraseGridOrigin.x + (float)_eraseGridWidth / 2) * _tilesSize, (_eraseGridOrigin.y + (float)_eraseGridHeight / 2) * _tilesSize), new Vector3(_eraseGridWidth * _tilesSize, _eraseGridHeight * _tilesSize, 1));
        Gizmos.color = Color.orangeRed;
        Gizmos.DrawWireCube(new Vector2((_spawnPointPosition.x + 0.5f) * _tilesSize, (_spawnPointPosition.y + 0.5f) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
    }
}
