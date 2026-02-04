using UnityEngine;
using System.Collections.Generic;
using Frynity.Gen;



#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UIElements;

public class GridDrawer : MonoBehaviour
{
    private enum LevelCreationSteps 
    { 
        Config = 0,
        Tiles = 1,
        SpawnPoints = 2,
        Room = 4,
        Save = 8,
    }

    [SerializeField] private LevelCreationSteps _levelCreationSteps;

    [Space(5)]
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Config)] private Tile _tilesPrefab;
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Config)] private Transform _tilesParent;
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Config)] private Transform _roomsParent;

    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Config)] private float _tilesSize;

    [SerializeField, MaxValue(0), ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Config)] private int _limitsMinX;
    [SerializeField, MinValue(0), ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Config)] private int _limitsMaxX;
    [SerializeField, MaxValue(0), ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Config)] private int _limitsMinY;
    [SerializeField, MinValue(0), ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Config)] private int _limitsMaxY;

    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Tiles)] private int _drawGridWidth;
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Tiles)] private int _drawGridHeight;
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Tiles)] private Vector2Int _drawGridOrigin;
                          
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Tiles)] private int _eraseGridWidth;
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Tiles)] private int _eraseGridHeight;
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Tiles)] private Vector2Int _eraseGridOrigin;
                          
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Tiles), Tooltip("The button 'Generate Grid from Data' can create Tiles using the grid's shape")] private GridLevelData _gridLevelData;

    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.SpawnPoints)] private Vector2Int _spawnPointPosition;
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.SpawnPoints)] private CellData.SpawnPointType _spawnPointTypes;

    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Room)] private GameObject _roomPrefab;
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Room)] private Vector2Int _roomPosition;

    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Save), Tooltip("The name given to your entier Grid Layout")] private string _gridLevelName;

    private static Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();
    private static Dictionary<Vector2Int, CellData.SpawnPointType> _spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();
    private static List<RoomData> _allRoomDatas = new List<RoomData>();

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

    public void SetAsSpawnPoint(Vector2Int position, CellData.SpawnPointType type)
    {
        if (_tiles.ContainsKey(position))
        {
            if (type.HasFlag(CellData.SpawnPointType.Player) && type != CellData.SpawnPointType.Player)
            {
                Debug.LogError("You can't make a spawn point spawn the player and other entities on the same tile");
                return;
            }
            if (type == CellData.SpawnPointType.Player)
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
            if (type == CellData.SpawnPointType.None)
            {
                _tiles[position].RemoveSpawnTile();
                Color newcol = new Color(Mathf.Lerp(0, 1, (float)position.x + (float)Mathf.Abs(_limitsMinX) / ((float)Mathf.Abs(_limitsMinX) + (float)_limitsMaxX)), 0.5f, Mathf.Lerp(0, 1, (float)position.y + (float)Mathf.Abs(_limitsMinY) / ((float)Mathf.Abs(_limitsMinY) + (float)_limitsMaxY)));
                _tiles[position].SetColor(newcol);
            }
            else
            {
                if (_tiles[position].CellData.spawnPointType != CellData.SpawnPointType.None)
                {
                    _tiles[position].RemoveSpawnTile(); // if the tile is a spawn point it makes the tile not a spawn point
                }
                _spawnPointsTiles[position] = type; // adds the tile to the dictionnary of all the spawn points. If it already exists, changes it
                _tiles[position].SetAsSpawnTile(type); // makes the tile a spawn point
            }
        }
        else
        {
            Debug.LogWarning("Doesn't Exist bro :sob:");
        }
    }

    private void PlaceRoom(GameObject room, Vector2Int position)
    {
        var roomCells = room.GetComponent<RoomCells>();
        if (roomCells != null)
        {
            List<Vector2Int> allTilesInTheRoom = new List<Vector2Int>();
            Vector2Int mainCellPosition = new Vector2Int();
            foreach(var cell in roomCells.GetCells)
            {
                if (_tiles.ContainsKey(cell.localPos + position))
                {
                    if (!_tiles[cell.localPos + position].IsInARoom())
                    {
                        allTilesInTheRoom.Add(cell.localPos + position);
                        if (cell.localPos == new Vector2(0, 0))
                        {
                            mainCellPosition = cell.localPos + position;
                        }
                    }
                    else
                    {
                        Debug.LogError("One of the tiles already has a room");
                        return;
                    }
                }
                else
                {
                    Debug.LogError("Your room doesn't fit in the grid");
                    return;
                }
            }
            RoomData roomData = new RoomData();
            roomData.SetMainCell(mainCellPosition);
            roomData.SetAllCellsInRoom(allTilesInTheRoom);
            _allRoomDatas.Add(roomData);
            foreach (var tile in allTilesInTheRoom)
            {
                _tiles[tile].SetAsInARoom(mainCellPosition, roomData);
            }
            GameObject newRoom = Instantiate(room, (Vector2)mainCellPosition, Quaternion.identity, _roomsParent);
        }
        else
        {
            Debug.LogError("You didn't select a valid Room");
        }
    }

    private void RemoveAllTiles()
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

        RoomCells[] cells = _roomsParent.GetComponentsInChildren<RoomCells>();
        foreach (var cell in cells)
        {
            DestroyImmediate(cell.gameObject);
        }

        _spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();
        _allRoomDatas = new List<RoomData>();
    }

    [Button("Generate Tiles (Green)")]
    private void GenerateTilesButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.Tiles)
        {
            GenerateGrid(_drawGridWidth, _drawGridHeight, _drawGridOrigin);
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Generate Grid From Data (Blue)")]
    private void GenerateGridDataButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.Tiles)
        {
            if (_gridLevelData != null)
            {
                GenerateGrid(_gridLevelData.allTilesPosition);
                if (_gridLevelData.spawnPointsTiles != null)
                {
                    foreach (var spawn in _gridLevelData.spawnPointsTiles)
                    {
                        SetAsSpawnPoint(spawn.Key, spawn.Value);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Erase Tiles (Red)")]
    private void EraseGridButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.Tiles)
        {
            EraseGrid(_eraseGridWidth, _eraseGridHeight, _eraseGridOrigin);
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Make Tile A Spawn Point (Orange)")]
    private void SetAsSpawnPointButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.Tiles)
        {
            SetAsSpawnPoint(_spawnPointPosition, _spawnPointTypes);
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Update Grid Size")]
    private void UpdateSizeAll()
    {
        if (_levelCreationSteps == LevelCreationSteps.Tiles)
        {
            foreach (var tile in _tiles)
            {
                tile.Value.transform.position = new Vector2(tile.Key.x * _tilesSize + 0.5f * _tilesSize, tile.Key.y * _tilesSize + 0.5f * _tilesSize);
                tile.Value.transform.localScale = new Vector3(_tilesSize, _tilesSize, _tilesSize);
            }
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Erase All Tiles")]
    private void RemoveAllTilesButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.Tiles)
        {
            RemoveAllTiles();
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Place Room")]
    private void PlaceRoomButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.Room)
        {
            PlaceRoom(_roomPrefab, _roomPosition);
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Save The Grid layout")]
    private void CommitValue()
    {
#if UNITY_EDITOR
        if (_levelCreationSteps == LevelCreationSteps.Save)
        {
            var data = GridLevelDataBase.GetOrCreate(_gridLevelName);

            Undo.RecordObject(data, "Commit Tool Value");

            //WRITE DATA YOU WANT TO SAVE

            List<Vector2Int> valueToSave = new List<Vector2Int>();
            Vector2Int _limitsMin = new Vector2Int();
            Vector2Int _limitsMax = new Vector2Int();
            foreach (var tile in _tiles.Keys)
            {
                valueToSave.Add(tile);
                if (_limitsMin.x > tile.x)
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
            foreach (var value in data.spawnPointsTiles)
            {
                data.keys.Add(value.Key);
                data.values.Add(value.Value);
            }
            data.allRooms = _allRoomDatas;
            foreach(var value in data.allRooms)
            {
                data.allRoomsPosition.Add(value.GetMainCell());
            }
            data.limitsMin = _limitsMin;
            data.limitsMax = _limitsMax;
            Debug.Log("Min X : " + data.limitsMin.x + " | Min Y : " + data.limitsMin.y + " | Max X : " + data.limitsMax.x + " | Max Y : " + data.limitsMin.y);

            //END OF WRITING DATA EZ

            GridLevelDataBase.Save(data);

            RemoveAllTiles();
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
#endif
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellowNice;
        Gizmos.DrawWireCube(new Vector2((0.5f + (float)(_limitsMaxX + _limitsMinX) / 2) * _tilesSize, (0.5f + (float)(_limitsMaxY + _limitsMinY) / 2) * _tilesSize), new Vector2((float)(Mathf.Abs(_limitsMaxX) + Mathf.Abs(_limitsMinX) + 1) * _tilesSize, (float)(Mathf.Abs(_limitsMaxY) + Mathf.Abs(_limitsMinY) + 1) * _tilesSize));
        if  (_levelCreationSteps == LevelCreationSteps.Tiles)
        {
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
        }
        if (_levelCreationSteps == LevelCreationSteps.SpawnPoints)
        {
            Gizmos.color = Color.orangeRed;
            Gizmos.DrawWireCube(new Vector2((_spawnPointPosition.x + 0.5f) * _tilesSize, (_spawnPointPosition.y + 0.5f) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
        }
        if (_levelCreationSteps == LevelCreationSteps.Room)
        {
            if (_roomPrefab != null) 
            {
                var roomCells = _roomPrefab.GetComponent<RoomCells>();
                if (roomCells != null)
                {
                    foreach (var cell in roomCells.GetCells) 
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawWireCube(new Vector2((float)cell.localPos.x + (float)_roomPosition.x + 0.5f, (float)cell.localPos.y + (float)_roomPosition.y + 0.5f), new Vector3(_tilesSize, _tilesSize, _tilesSize));
                    }
                }
            }
        }
    }
}
