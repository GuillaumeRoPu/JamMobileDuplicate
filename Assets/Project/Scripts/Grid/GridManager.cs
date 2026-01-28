using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{
    [SerializeField] private bool _generateGridOnPlay;
    [SerializeField] private int _gridWidth;
    [SerializeField] private int _gridHeight;
    [SerializeField] private Vector2Int _gridOrigin;

    [SerializeField] private Tile _tilesPrefab;
    [SerializeField] private Transform _tilesParent;

    [SerializeField] private float _tilesSize;

    [SerializeField] private Vector2 _limits;

    [SerializeField] private Transform _camera;

    [SerializeField] private bool _coolColors;
    [SerializeField] private bool _coolColorsAnimated;

    private Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();

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
                    _tiles[tilePosition].SetColor(newcol);
                }
            }
        }
        UpdateSizeAll();
    }

    void RelocateCamera(int width, int height, Vector2Int origin)
    {
        _camera.transform.position = new Vector3((float)width / 2 + origin.x, (float)height / 2 + origin.y, _camera.transform.position.z);
    }

    void UpdateSize(Vector2Int position)
    {
        _tiles[position].transform.position *= _tilesSize;
        _tiles[position].transform.localScale *= _tilesSize;
    }

    private void Start()
    {
        Debug.Log(_tiles.Count);
        if (_tiles == null)
        {
            _tiles = new Dictionary<Vector2Int, Tile>();
        }
        if (_generateGridOnPlay)
        {
            GenerateGrid(_gridWidth, _gridHeight, _gridOrigin);
            RelocateCamera(_gridWidth, _gridHeight, _gridOrigin);
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
        if (_tiles == null)
        {
            _tiles = new Dictionary<Vector2Int, Tile>();
        }
        if (_tiles.ContainsKey(position) && (Mathf.Abs(position.x) <= _limits.x/2 && Mathf.Abs(position.y) <= _limits.y/2))
        {
            Debug.LogWarning("There's Already A Tile Here or Its Out Of Bounds");
        }
        else
        {
            var spawnedTile = Instantiate(_tilesPrefab, new Vector3(position.x + 0.5f, position.y + 0.5f), Quaternion.identity, _tilesParent);
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

    [Button("All tiles gone.")]
    public void RemoveAllTiles()
    {
        List<Vector2Int> list = new List<Vector2Int>();
        foreach(var tile in _tiles.Keys)
        {
            list.Add(tile);
        }
        foreach (var tile in list) 
        {
            RemoveTile(tile);
        }
        _tiles.Clear();
    }

    [SerializeField, Foldout("Test")] private Vector2Int _testTileAddPosition;

    [Button("Fuck you new tile")]
    public void AddTileAt()
    {
        AddTile(_testTileAddPosition);
    }

    [SerializeField, Foldout("Test")] private Vector2Int _testTileRemovePosition;

    [Button("Fuck you no tile")]
    public void RemoveTileAt()
    {
        RemoveTile(_testTileRemovePosition);
    }

    [SerializeField, Foldout("Test")] private int _testGridWidth;
    [SerializeField, Foldout("Test")] private int _testGridHeight;
    [SerializeField, Foldout("Test")] private Vector2Int _testGridHeightOrigin;

    [Button("Generate Grid with width and height")]
    public void GenerateGridButton()
    {
        GenerateGrid(_testGridWidth, _testGridHeight, _testGridHeightOrigin);
    }

    [Button("Update Grid Size")]
    void UpdateSizeAll()
    {
        foreach (var tile in _tiles)
        {
            tile.Value.transform.position = (Vector2)tile.Key * _tilesSize;
            tile.Value.transform.localScale = new Vector3(_tilesSize, _tilesSize, _tilesSize);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellowNice;
        Gizmos.DrawWireCube(new Vector2(0 + 0.5f, 0 + 0.5f), _limits);
    }
}
