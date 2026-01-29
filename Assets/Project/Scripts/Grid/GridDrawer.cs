using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    [SerializeField] private bool _generateGridOnPlay;

    [SerializeField] private Tile _tilesPrefab;
    [SerializeField] private Transform _tilesParent;

    [SerializeField] private float _tilesSize;

    [SerializeField, MaxValue(0)] private int _limitsMinX;
    [SerializeField, MinValue(0)] private int _limitsMaxX;
    [SerializeField, MaxValue(0)] private int _limitsMinY;
    [SerializeField, MinValue(0)] private int _limitsMaxY;

    private static Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();

    //Fills the whole grid with tiles depending on width and height, as well as the origin
    void GenerateGrid(int width, int height, Vector2Int origin)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int tilePosition = new Vector2Int(x + origin.x, y + origin.y);
                AddTile(tilePosition);
                Color newcol = new Color(Mathf.Lerp(0, 1, x / ((float)width - 1)) * 1f, 1 * 0.5f, Mathf.Lerp(0, 1, y / ((float)height - 1)) * 1f);
                if (_tiles.ContainsKey(tilePosition))
                {
                    _tiles[tilePosition].SetColor(newcol);
                }
            }
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
        if (_generateGridOnPlay)
        {
            //GenerateGrid(_gridWidth, _gridHeight, _gridOrigin);
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

    [SerializeField, Foldout("In Editor Draw")] private int _drawGridWidth;
    [SerializeField, Foldout("In Editor Draw")] private int _drawGridHeight;
    [SerializeField, Foldout("In Editor Draw")] private Vector2Int _drawGridOrigin;

    [SerializeField, Foldout("In Editor Draw")] private int _eraseGridWidth;
    [SerializeField, Foldout("In Editor Draw")] private int _eraseGridHeight;
    [SerializeField, Foldout("In Editor Draw")] private Vector2Int _eraseGridOrigin;

    [Button("Generate Grid (Green)")]
    public void GenerateGridButton()
    {
        GenerateGrid(_drawGridWidth, _drawGridHeight, _drawGridOrigin);
    }

    [Button("Erase Grid (Red)")]
    public void EraseGridButton()
    {
        EraseGrid(_eraseGridWidth, _eraseGridHeight, _eraseGridOrigin);
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
            Debug.Log("Log");
            tile.AnnihilateTile();
        }
    }

    //Test
    [SerializeField] private string _gridLevelName;

    [Button("Save The Grid layout")]
    public void CommitValue()
    {
        #if UNITY_EDITOR
        var data = GridLevelDataBase.GetOrCreate(_gridLevelName);

        Undo.RecordObject(data, "Commit Tool Value");

        //WRITE DATA YOU WANT TO SAVE

        List<Vector2Int> valueToSave = new List<Vector2Int>();
        foreach (var tile in _tiles.Keys)
        {
            valueToSave.Add(tile);
        }
        data._allTilesPositions = valueToSave;


        //END OF WRITING DATA EZ

        GridLevelDataBase.Save(data);

        RemoveAllTiles();
        #endif
    }


    //StopTest

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellowNice;
        Gizmos.DrawWireCube(new Vector2((0.5f + (float)(_limitsMaxX + _limitsMinX) /2) * _tilesSize, (0.5f + (float)(_limitsMaxY + _limitsMinY) /2) * _tilesSize), new Vector2( (float)(Mathf.Abs(_limitsMaxX) + Mathf.Abs(_limitsMinX) + 1) * _tilesSize, (float)(Mathf.Abs(_limitsMaxY) + Mathf.Abs(_limitsMinY) + 1) * _tilesSize ));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector2((_drawGridOrigin.x + (float)_drawGridWidth / 2) * _tilesSize, (_drawGridOrigin.y + (float)_drawGridHeight / 2) * _tilesSize), new Vector3(_drawGridWidth * _tilesSize, _drawGridHeight * _tilesSize, 1));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2((_eraseGridOrigin.x + (float)_eraseGridWidth / 2) * _tilesSize, (_eraseGridOrigin.y + (float)_eraseGridHeight / 2) * _tilesSize), new Vector3(_eraseGridWidth * _tilesSize, _eraseGridHeight * _tilesSize, 1));
    }
}
