using Frynity.Gen;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private bool _generateGridOnPlay;
    [SerializeField] private GridLevelData _gridLevelData;

    [SerializeField] private Tile _tilesPrefab;
    [SerializeField] private Transform _tilesParent;
    [SerializeField] private Transform _roomsParent;

    [SerializeField] private float _tilesSize;

    [SerializeField] private Trap _barricade;

    private Vector2Int _limitsMin;
    private Vector2Int _limitsMax;

    [SerializeField] private Transform _camera;

    [SerializeField] private bool _coolColors;
    [SerializeField] private bool _coolColorsAnimated;

    //ALL THE DATA FOR THE GRID IS HERE

    private static Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();

    private List<RoomData> _allRoomDatas = new List<RoomData>();
    private Dictionary<Vector2Int, GameObject> _roomDeletionTracker = new Dictionary<Vector2Int, GameObject>();

    public static Dictionary<Vector2Int, Tile> Tiles => _tiles;

    public GridLevelData GridLevelData => _gridLevelData;
    public void SetLevelData(GridLevelData levelData) {  _gridLevelData = levelData; }
    public float TileSize => _tilesSize;


    //END OF ALL THE DATA

    public static Face GetFace(Vector2Int face)
    {
        if (face == Vector2Int.right)
            return Face.Right;
        if (face == Vector2Int.left)
            return Face.Left;
        if (face == Vector2Int.up)
            return Face.Up;
        if (face == Vector2Int.down)
            return Face.Down;
        else
            return Face.None;
    }


    //Fills the whole grid with tiles depending on width and height, as well as the origin

    public Vector2Int WorldPositionToTilePosition(Vector2 position)
    {
        return Vector2Int.FloorToInt(position / _tilesSize);
    }

    void GenerateGrid(List<Vector2Int> tiles)
    {
        foreach (var tile in tiles)
        {
            AddTile(tile);
        }
    }

    void PlaceAllRooms(List<RoomData> data)
    {
        foreach(var room in data)
        {
            PlaceRoom(room.GetRoomPrefab(), room.GetMainCell());
        }
        UpdateSizeAll();
    }

    void PlaceAllDoors(Dictionary<Vector2Int, Face> allDoors)
    {
        foreach(var door in allDoors)
        {
            SetDoor(door.Key, door.Value);
        }
    }

    public void EntityInARoom(Vector2Int tile, bool enter = true)
    {
        if (!Tiles[tile].CellData.HasRoom) return;
        foreach (SpriteRenderer sprite in GetTileAtPosition(tile).GetRoomData().GetWalls)
        {
            if (sprite == null) continue;
            sprite.color = new Color(1, 1, 1, enter ? 0.5f : 1);
        }
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
        if (_gridLevelData != null && _generateGridOnPlay)
        {
            //Init();
        }
    }

    public void Init()
    {
        if (_gridLevelData != null)
        {
            _limitsMax = _gridLevelData.limitsMax;
            _limitsMin = _gridLevelData.limitsMin;
            GenerateGrid(_gridLevelData.allTilesPosition);
            PlaceAllRooms(_gridLevelData.allRooms);
            PlaceAllDoors(_gridLevelData.allDoors);
            PlaceAllMaterials(_gridLevelData.allMaterials);
            PlaceAllBarricades(_gridLevelData.allBarricades);
        }
    }

    public void EraseAll()
    {

        foreach (var tile in _tiles)
        {
            tile.Value.RemoveMaterial();
            tile.Value.CellData.ActivateTileTrap(null);
            tile.Value.CellData.ActivateDoorTrap(null);
        }

        for (int i = 0; i < _tilesParent.childCount; i++)
        {
            Destroy(_tilesParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < _roomsParent.childCount; i++)
        {
            Destroy(_roomsParent.GetChild(i).gameObject);
        }

        _tiles.Clear();
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

    private void PlaceRoom(GameObject room, Vector2Int roomPosition)
    {
        var roomCells = room.GetComponent<RoomCells>();
        if (roomCells != null)
        {
            List<Vector2Int> allTilesInTheRoom = new List<Vector2Int>();
            Vector2Int mainCellPosition = new Vector2Int();
            foreach (var cell in roomCells.GetCells)
            {
                if (_tiles.ContainsKey(cell.localPos + roomPosition))
                {

                    if (!_tiles[cell.localPos + roomPosition].IsInARoom())
                    {
                        allTilesInTheRoom.Add(cell.localPos + roomPosition);
                        if (cell.localPos == new Vector2(0, 0))
                        {
                            mainCellPosition = cell.localPos + roomPosition;
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
            roomData.SetRoomPrefab(room);
            _allRoomDatas.Add(roomData);
            foreach (var tile in allTilesInTheRoom)
            {
                _tiles[tile].SetAsInARoom(mainCellPosition, roomData);
            }
            GameObject newRoom = Instantiate(room, (Vector2)mainCellPosition, Quaternion.identity, _roomsParent);
            _roomDeletionTracker[mainCellPosition] = newRoom;
        }
        else
        {
            Debug.LogError("You didn't select a valid Room");
        }
    }

    private void SetDoor(Vector2Int position, Face face)
    {
        if (_tiles.ContainsKey(position))
        {
            _tiles[position].CellData.SetDoor(face);
            //switch (face)
            //{
            //    case Face.Up:
            //        Debug.Log("Add a door to the top of Cell | X : " + position.x + ", Y : " + position.y);
            //        break;
            //    case Face.Down:
            //        Debug.Log("Add a door to the bottom of Cell | X : " + position.x + ", Y : " + position.y);
            //        break;
            //    case Face.Left:
            //        Debug.Log("Add a door to the left of Cell | X : " + position.x + ", Y : " + position.y);
            //        break;
            //    case Face.Right:
            //        Debug.Log("Add a door to the right of Cell | X : " + position.x + ", Y : " + position.y);
            //        break;
            //    default:
            //        break;
            //}
        }
    }

    private void PlaceAllMaterials(HashSet<Vector2Int> allMaterials)
    {
        foreach (var pos in allMaterials)
        {
            if (_tiles.ContainsKey(pos)) 
            {
                _tiles[pos].GiveTileAMaterial();
            }
        }
    }

    private void PlaceAllBarricades(HashSet<Vector2Int> allBarricades)
    {
        foreach (var pos in allBarricades)
        {
            if (_tiles.ContainsKey(pos))
            {
                var offset = new Vector2(0, 0);
                switch (GridManager.Tiles[pos].CellData.DoorPosition)
                {
                    case Face.Up:
                        offset = new Vector2(0.5f, 1);
                        break;
                    case Face.Down:
                        offset = new Vector2(0.5f, 0);
                        break;
                    case Face.Left:
                        offset = new Vector2(0, 0.5f);
                        break;
                    case Face.Right:
                        offset = new Vector2(1, 0.5f);
                        break;
                    default:
                        break;
                }
                var trap = Instantiate(_barricade, (Vector2)pos + offset, Quaternion.identity);
                trap.TrapPlaced(pos, GridManager.Tiles[pos].CellData.DoorPosition);
                GridManager.Tiles[pos].CellData.SetTrap(trap);
            }
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
            Gizmos.color = Color.yellowNice;
            Gizmos.DrawWireCube(new Vector2((0.5f + (float)(_gridLevelData.limitsMax.x + _gridLevelData.limitsMin.x) / 2) * _tilesSize, (0.5f + (float)(_gridLevelData.limitsMax.y + _gridLevelData.limitsMin.y) / 2) * _tilesSize), new Vector2((float)(Mathf.Abs(_gridLevelData.limitsMax.x) + Mathf.Abs(_gridLevelData.limitsMin.x) + 1) * _tilesSize, (float)(Mathf.Abs(_gridLevelData.limitsMax.y) + Mathf.Abs(_gridLevelData.limitsMin.y) + 1) * _tilesSize));
            foreach (var tile in _gridLevelData.allTilesPosition)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(new Vector2((0.5f + tile.x) * _tilesSize, (0.5f + tile.y) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
                Gizmos.DrawWireSphere(new Vector2((0.5f + tile.x) * _tilesSize, (0.5f + tile.y) * _tilesSize), 0.25f);
            }
            foreach (var tile in _gridLevelData.spawnPointsTiles)
            {
                if (_gridLevelData.spawnPointsTiles[tile.Key].HasFlag(CellData.SpawnPointType.Player))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(new Vector2((0.5f + tile.Key.x) * _tilesSize, (0.5f + tile.Key.y) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
                }
                else if (_gridLevelData.spawnPointsTiles[tile.Key] != CellData.SpawnPointType.None)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(new Vector2((0.5f + tile.Key.x) * _tilesSize, (0.5f + tile.Key.y) * _tilesSize), new Vector3(_tilesSize, _tilesSize, _tilesSize));
                }
            }
            foreach (var door in _gridLevelData.allDoors)
            {
                Gizmos.color = Color.black;
                Vector2 pos = door.Key;
                switch (door.Value)
                {
                    case Face.Up:
                        pos = new Vector2(pos.x + 0.5f, pos.y + 0.9f);
                        break;
                    case Face.Down:
                        pos = new Vector2(pos.x + 0.5f, pos.y + 0.1f);
                        break;
                    case Face.Left:
                        pos = new Vector2(pos.x + 0.1f, pos.y + 0.5f);
                        break;
                    case Face.Right:
                        pos = new Vector2(pos.x + 0.9f, pos.y + 0.5f);
                        break;
                    default:
                        break;
                }
                Gizmos.DrawWireSphere(pos, 0.25f);
            }
        }
    }

    [SerializeField] int _spawnIndex;
    private void OnDrawGizmos()
    {
        if (_gridLevelData == null) return;

        _spawnIndex = Mathf.Clamp(_spawnIndex, 0, _gridLevelData.keys.Count - 1);

        if (_gridLevelData.values[_spawnIndex] == CellData.SpawnPointType.Player)
            _spawnIndex++;

        _spawnIndex = Mathf.Min(_spawnIndex, _gridLevelData.keys.Count - 1);

        Vector2Int pos = _gridLevelData.keys[_spawnIndex];

        Gizmos.DrawCube((Vector2)pos * TileSize + Vector2.one/2, Vector3.one * TileSize);
    }
}