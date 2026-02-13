using UnityEngine;
using System.Collections.Generic;
using Frynity.Gen;




#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UIElements;

#if UNITY_EDITOR

public class GridDrawer : MonoBehaviour
{
    private enum LevelCreationSteps 
    { 
        Config = 0,
        Tiles = 1,
        SpawnPoints = 2,
        Room = 4,
        Materials = 8,
        Save = 16,
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
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Room)] private Vector2Int _eraseRoomPosition;

    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Materials)] private Vector2Int _materialPosition;
    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Materials)] private Vector2Int _eraseMaterialPosition;

    [SerializeField, ShowIf(nameof(_levelCreationSteps), true, LevelCreationSteps.Save), Tooltip("The name given to your entier Grid Layout")] private string _gridLevelName;

    private static Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();
    private static Dictionary<Vector2Int, CellData.SpawnPointType> _spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();
    private static List<RoomData> _allRoomDatas = new List<RoomData>();
    private static Dictionary<Vector2Int, GameObject> _roomDeletionTracker = new Dictionary<Vector2Int, GameObject>();
    private static Dictionary<Vector2Int, Face> _allDoors = new Dictionary<Vector2Int, Face>();
    private static HashSet<Vector2Int> _allMaterials = new HashSet<Vector2Int>();

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
            EraseRoom(position, true);
            SetAsSpawnPoint(position, CellData.SpawnPointType.None);
            _tiles[position].AnnihilateTile();
            _tiles.Remove(position);
            Debug.Log("Is inside ");
        }
        else
        {
            if (Application.isPlaying)
            {
                Debug.Log("There Isn't A Tile Here : X - " + position.x + " | Y - " + position.y);
            }
        }
    }

    public void RemoveTileBis(Vector2Int position)
    {
        if (_tiles.ContainsKey(position))
        {
            EraseRoom(position);
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
                        Color newcol = new Color(Mathf.Lerp(0, 1, (float)(spawn.Key.x + Mathf.Abs(_limitsMinX)) / (float)(Mathf.Abs(_limitsMinX) + _limitsMaxX)), 0.5f, Mathf.Lerp(0, 1, (float)(spawn.Key.y + Mathf.Abs(_limitsMinY)) / (float)(Mathf.Abs(_limitsMinY) + _limitsMaxY)));
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
                Color newcol = new Color(Mathf.Lerp(0, 1, (float)(position.x + Mathf.Abs(_limitsMinX)) / (float)(Mathf.Abs(_limitsMinX) + _limitsMaxX)), 0.5f, Mathf.Lerp(0, 1, (float)(position.y + Mathf.Abs(_limitsMinY)) / (float)(Mathf.Abs(_limitsMinY) + _limitsMaxY)));
                _tiles[position].SetColor(newcol);
                _spawnPointsTiles.Remove(position);
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

    private (Vector2Int, Face) GetOppositeDoor(Vector2Int position, Face doorFace)
    {
        if (doorFace == Face.None)
        {
            return (position, doorFace);
        }
        else if (doorFace == Face.Left)
        {
            return (new Vector2Int(position.x - 1, position.y), Face.Right);
        }
        else if (doorFace == Face.Right)
        {
            return (new Vector2Int(position.x + 1, position.y), Face.Left);
        }
        else if (doorFace == Face.Down)
        {
            return (new Vector2Int(position.x, position.y - 1), Face.Up);
        }
        else if (doorFace == Face.Up)
        {
            return (new Vector2Int(position.x, position.y + 1), Face.Down);
        }
        else
        {
            return (position, doorFace);
        }
    }

    private Face GetOppositeFace(Face face)
    {
        if (face == Face.Left)
        {
            return Face.Right;
        }
        else if (face == Face.Right)
        {
            return Face.Left;
        }
        else if (face == Face.Down)
        {
            return Face.Up;
        }
        else if (face == Face.Up)
        {
            return Face.Down;
        }
        else
        {
            return Face.None;
        }
    }

    private Face GetFaceDoor(FaceDoor doorFace)
    {
        if (doorFace == FaceDoor.Left)
        {
            return Face.Left;
        }
        else if (doorFace == FaceDoor.Right)
        {
            return Face.Right;
        }
        else if (doorFace == FaceDoor.Bottom)
        {
            return Face.Down;
        }
        else if (doorFace == FaceDoor.Top)
        {
            return Face.Up;
        }
        else
        {
            return Face.None;
        }
    }

    private void PlaceRoom(GameObject room, Vector2Int position)
    {
        var roomCells = room.GetComponent<RoomCells>();
        if (roomCells != null)
        {
            List<Vector2Int> allTilesInTheRoom = new List<Vector2Int>();
            Dictionary<Vector2Int, Face> allDoors = new Dictionary<Vector2Int, Face>();
            Dictionary<Vector2Int, Face> temporaryAllDoors = new Dictionary<Vector2Int, Face>();
            Vector2Int mainCellPosition = new Vector2Int();
            foreach(var cell in roomCells.GetCells)
            {
                var cellWorldPos = cell.localPos + position;
                if (_tiles.ContainsKey(cellWorldPos))
                {
                    if (!_tiles[cellWorldPos].IsInARoom())
                    {
                        allTilesInTheRoom.Add(cellWorldPos);

                        if (cell.localPos == new Vector2(0, 0))
                        {
                            mainCellPosition = cellWorldPos;
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
            //Checks if each tile has a valid door to place
            foreach (var cell in roomCells.GetCells)
            {
                var cellWorldPos = cell.localPos + position;
                Face doorFace = Face.None;
                for (int i = 0; i < roomCells.GetActiveDoors.Count; i++)
                {
                    if (roomCells.GetDoors[i].cellId == cell.id && roomCells.GetActiveDoors[i])
                    {
                        doorFace = GetFaceDoor(roomCells.GetDoors[i].face);
                    }
                }
                if (doorFace != Face.None)
                {
                    if (_tiles.ContainsKey(new Vector2Int(cellWorldPos.x + 1, cellWorldPos.y)))
                    {
                        var tileCellData = _tiles[new Vector2Int(cellWorldPos.x + 1, cellWorldPos.y)].CellData;
                        if (tileCellData.HasADoor() && tileCellData.DoorPosition == Face.Left && tileCellData.DoorPosition != GetOppositeFace(doorFace))
                        {
                            Debug.LogError("One of the tiles has more than 1 door");
                            Debug.Log("This is the problem Right");
                            return;
                        }
                    }
                    if (_tiles.ContainsKey(new Vector2Int(cellWorldPos.x - 1, cellWorldPos.y)))
                    {
                        var tileCellData = _tiles[new Vector2Int(cellWorldPos.x - 1, cellWorldPos.y)].CellData;
                        if (tileCellData.HasADoor() && tileCellData.DoorPosition == Face.Right && tileCellData.DoorPosition != GetOppositeFace(doorFace))
                        {
                            Debug.LogError("One of the tiles has more than 1 door");
                            Debug.Log("This is the problem Left");
                            return;
                        }
                    }
                    if (_tiles.ContainsKey(new Vector2Int(cellWorldPos.x, cellWorldPos.y + 1)))
                    {
                        var tileCellData = _tiles[new Vector2Int(cellWorldPos.x, cellWorldPos.y + 1)].CellData;
                        if (tileCellData.HasADoor() && tileCellData.DoorPosition == Face.Down && tileCellData.DoorPosition != GetOppositeFace(doorFace))
                        {
                            Debug.LogError("One of the tiles has more than 1 door");
                            Debug.Log("This is the problem Up");
                            return;
                        }
                    }
                    if (_tiles.ContainsKey(new Vector2Int(cellWorldPos.x, cellWorldPos.y - 1)))
                    {
                        var tileCellData = _tiles[new Vector2Int(cellWorldPos.x, cellWorldPos.y - 1)].CellData;
                        if (tileCellData.HasADoor() && tileCellData.DoorPosition == Face.Up && tileCellData.DoorPosition != GetOppositeFace(doorFace))
                        {
                            Debug.LogError("One of the tiles has more than 1 door");
                            Debug.Log("This is the problem Down");
                            return;
                        }
                    }
                    var oppositeDoor = GetOppositeDoor(cellWorldPos, doorFace);
                    if (_tiles.ContainsKey(oppositeDoor.Item1))
                    {
                        if (!_tiles[oppositeDoor.Item1].CellData.HasADoor() || _tiles[oppositeDoor.Item1].CellData.DoorPosition == GetOppositeFace(doorFace))
                        {
                            temporaryAllDoors[oppositeDoor.Item1] = oppositeDoor.Item2;
                        }
                        else
                        {
                            Debug.LogError("One of the tiles has more than 1 door");
                            return;
                        }
                    }
                    allDoors.Add(cellWorldPos, doorFace);
                    temporaryAllDoors[cellWorldPos] = doorFace;
                }
            }

            //Then sets all the doors at the tiles that need to get a new door
            foreach(var door in temporaryAllDoors)
            {
                SetDoor(door.Key, door.Value);
                _allDoors[door.Key] = door.Value;
            }

            RoomData roomData = new RoomData();
            roomData.SetMainCell(mainCellPosition);
            roomData.SetAllCellsInRoom(allTilesInTheRoom);
            roomData.SetRoomPrefab(room);
            roomData.SetAllDoorsinRoom(allDoors);
            _allRoomDatas.Add(roomData);
            foreach (var tile in allTilesInTheRoom)
            {
                _tiles[tile].SetAsInARoom(mainCellPosition, roomData);
            }
            GameObject newRoom = PrefabUtility.InstantiatePrefab(room, _roomsParent) as GameObject;
            newRoom.transform.position = (Vector2)mainCellPosition;
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
            if (!_tiles[position].CellData.HasADoor() && face != Face.None)
            {
                _tiles[position].CellData.SetDoor(face);
                switch (face)
                {
                    case Face.Up:
                        Debug.Log("Add a door to the top of Cell | X : " + position.x + ", Y : " + position.y);
                        break;
                    case Face.Down:
                        Debug.Log("Add a door to the bottom of Cell | X : " + position.x + ", Y : " + position.y);
                        break;
                    case Face.Left:
                        Debug.Log("Add a door to the left of Cell | X : " + position.x + ", Y : " + position.y);
                        break;
                    case Face.Right:
                        Debug.Log("Add a door to the right of Cell | X : " + position.x + ", Y : " + position.y);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Debug.Log("The Tile you are trying to put a door on already has a door");
            }
        }
        else
        {
            Debug.Log("Can't set a door if there isn't a tile");
        }
    }

    private void PlaceRoom(RoomData room)
    {
        if (room != null)
        {
            List<Vector2Int> allTilesInTheRoom = new List<Vector2Int>();
            Dictionary<Vector2Int, Face> allDoors = new Dictionary<Vector2Int, Face>();
            Vector2Int mainCellPosition = new Vector2Int();
            foreach (var cell in room.GetAllCellsInRoom())
            {
                if (_tiles.ContainsKey(cell))
                {
                    if (!_tiles[cell].IsInARoom())
                    {
                        allTilesInTheRoom.Add(cell);
                        if (cell == new Vector2(0, 0))
                        {
                            mainCellPosition = cell;
                        }
                        //if (roomCells.)
                        //{
                        //
                        //}
                        //allDoors.Add(cell, );
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
            RoomData roomData = room;
            _allRoomDatas.Add(roomData);
            foreach (var tile in allTilesInTheRoom)
            {
                _tiles[tile].SetAsInARoom(mainCellPosition, roomData);
            }
            GameObject newRoom = PrefabUtility.InstantiatePrefab(room.GetRoomPrefab(), _roomsParent) as GameObject;
            newRoom.transform.position = (Vector2)mainCellPosition;
            _roomDeletionTracker[mainCellPosition] = newRoom;
        }
        else
        {
            Debug.LogError("You didn't select a valid Room");
        }
    }

    private void EraseRoom(Vector2Int position, bool specialDelete = false)
    {
        if (_tiles.ContainsKey(position))
        {
            if (_tiles[position].IsInARoom())
            {
                Vector2Int mainCell = _tiles[position].GetMainRoomCell();
                RoomData roomData = _tiles[mainCell].GetRoomData();
                List<Vector2Int> allTilesInRoom = roomData.GetAllCellsInRoom();
                foreach (var tile in allTilesInRoom)
                {
                    _tiles[tile].SetAsNotInARoom();
                    if (_allDoors.ContainsKey(tile) && roomData.GetAllDoorsinRoom().ContainsKey(tile))
                    {
                        var opposite = GetOppositeDoor(tile, _allDoors[tile]);
                        if (_tiles.ContainsKey(opposite.Item1))
                        {
                            if (_tiles[opposite.Item1].IsInARoom())
                            {
                                if (!_tiles[_tiles[opposite.Item1].GetMainRoomCell()].GetRoomData().GetAllDoorsinRoom().ContainsKey(opposite.Item1))
                                {
                                    _allDoors.Remove(opposite.Item1);
                                    _tiles[opposite.Item1].CellData.SetDoor(Face.None);
                                    _allDoors.Remove(tile);
                                    _tiles[tile].CellData.SetDoor(Face.None);
                                }
                            }
                            else
                            {
                                _allDoors.Remove(opposite.Item1);
                                _tiles[opposite.Item1].CellData.SetDoor(Face.None);
                                _allDoors.Remove(tile);
                                _tiles[tile].CellData.SetDoor(Face.None);
                            }
                        }
                        else
                        {
                            _allDoors.Remove(tile);
                            _tiles[tile].CellData.SetDoor(Face.None);
                        }
                    }
                    Debug.Log("Removed X : " + tile.x + ", Y : " + tile.y + " from the room");
                }
                DestroyImmediate(_roomDeletionTracker[mainCell]);
                _roomDeletionTracker.Remove(mainCell);
                _allRoomDatas.Remove(roomData);
            }
            else if (specialDelete == true)
            {
                Debug.LogWarning("This tile isn't tied to any room");
                return;
            }
        }
        else
        {
            Debug.LogWarning("There isn't a tile here");
            return;
        }
    }

    private void PlaceMaterial(Vector2Int position)
    {
        if (_tiles.ContainsKey(position)) 
        { 
            _allMaterials.Add(position);
        }
        else
        {
            Debug.LogWarning("There isn't a tile here");
        }
    }

    private void EraseMaterial(Vector2Int position)
    {
        if (_allMaterials.Contains(position))
        {
            _allMaterials.Remove(position);
        }
        else
        {
            Debug.LogWarning("There isn't a material here");
        }
    }

    private void EraseAllRooms()
    {
        List<RoomData> allRooms = new List<RoomData>(_allRoomDatas);
        foreach(var roomData in allRooms)
        {
            EraseRoom(roomData.GetMainCell());
        }

        RoomCells[] cells = _roomsParent.GetComponentsInChildren<RoomCells>();
        foreach (var cell in cells)
        {
            DestroyImmediate(cell.gameObject);
        }

        _allRoomDatas = new List<RoomData>();
        _roomDeletionTracker.Clear();
        _allDoors.Clear();
    }

    private void RemoveAllTiles()
    {
        EraseAllRooms();

        List<Vector2Int> list = new List<Vector2Int>();
        foreach (var tile in _tiles.Keys)
        {
            list.Add(tile);
        }
        foreach (var tile in list)
        {
            RemoveTileBis(tile); // I had to create a new RemoveTile method cuz the first one also deleted the data from the dictionnary of spawn points when you saved for some unknown reason ????
        }
        _tiles.Clear();

        Tile[] tiles = _tilesParent.GetComponentsInChildren<Tile>();
        foreach (var tile in tiles)
        {
            tile.AnnihilateTile();
        }

        _spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>();
        _allMaterials = new HashSet<Vector2Int>();
    }

    [Button("Generate Tiles (Green)"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Tiles)]
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

    [Button("Generate Grid From Data (Blue)"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Tiles)]
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
                if (_gridLevelData.allRooms != null)
                {
                    foreach (var room in _gridLevelData.allRooms)
                    {
                        PlaceRoom(room.GetRoomPrefab(), room.GetMainCell());
                    }
                }
                if (_gridLevelData.allMaterials != null)
                {
                    foreach (var mat in _gridLevelData.allMaterials)
                    {
                        PlaceMaterial(mat);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Erase Tiles (Red)"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Tiles)]
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

    [Button("Update Grid Size"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Tiles)]
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

    [Button("Erase All Tiles"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Tiles)]
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

    [Button("Make Tile A Spawn Point (Orange)"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.SpawnPoints)]
    private void SetAsSpawnPointButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.SpawnPoints)
        {
            SetAsSpawnPoint(_spawnPointPosition, _spawnPointTypes);
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Place Room"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Room)]
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

    [Button("Erase Room"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Room)]
    private void EraseRoomButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.Room)
        {
            EraseRoom(_eraseRoomPosition);
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Place Material"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Materials)]
    private void PlaceMaterialButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.Materials)
        {
            PlaceMaterial(_materialPosition);
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Erase Material"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Materials)]
    private void EraseMaterialButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.Materials)
        {
            EraseMaterial(_eraseMaterialPosition);
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Erase All Room"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Room)]
    private void EraseAllRoomsButton()
    {
        if (_levelCreationSteps == LevelCreationSteps.Room)
        {
            EraseAllRooms();
        }
        else
        {
            Debug.LogWarning("Change Creation Step");
        }
    }

    [Button("Save The Grid layout"), ButtonShowIf(nameof(_levelCreationSteps), LevelCreationSteps.Save)]
    private void CommitValue()
    {
#if UNITY_EDITOR
        if (_levelCreationSteps == LevelCreationSteps.Save)
        {
            var data = GridLevelDataBase.GetOrCreate(_gridLevelName);
            Debug.Log("First Count : " + data.spawnPointsTiles.Count);

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
            data.spawnPointsTiles = new Dictionary<Vector2Int, CellData.SpawnPointType>(_spawnPointsTiles);
            foreach (var value in data.spawnPointsTiles)
            {
                data.keys.Add(value.Key);
                data.values.Add(value.Value);
            }
            data.allRooms = new List<RoomData>(_allRoomDatas);
            foreach(var value in data.allRooms)
            {
                data.allRoomsPosition.Add(value.GetMainCell());
            }
            data.allDoors = new Dictionary<Vector2Int, Face>(_allDoors);
            foreach (var value in data.allDoors)
            {
                data.keysD.Add(value.Key);
                data.valuesD.Add(value.Value);
            }
            data.allMaterials = new HashSet<Vector2Int>(_allMaterials);

            data.limitsMin = _limitsMin;
            data.limitsMax = _limitsMax;


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
            Gizmos.DrawWireSphere(new Vector2((_spawnPointPosition.x + 0.5f) * _tilesSize, (_spawnPointPosition.y + 0.5f) * _tilesSize), 0.25f);
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
                        Gizmos.color = Color.darkGreen;
                        Gizmos.DrawWireCube(new Vector2((float)cell.localPos.x + (float)_roomPosition.x + 0.5f, (float)cell.localPos.y + (float)_roomPosition.y + 0.5f), new Vector3(_tilesSize, _tilesSize, _tilesSize));
                        Gizmos.DrawWireSphere(new Vector2((float)cell.localPos.x + (float)_roomPosition.x + 0.5f, (float)cell.localPos.y + (float)_roomPosition.y + 0.5f), 0.25f);
                    }
                }
            }
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector2((float)_eraseRoomPosition.x + 0.5f, (float)_eraseRoomPosition.y + 0.5f), new Vector3(_tilesSize, _tilesSize, _tilesSize));
            Gizmos.DrawLine(new Vector2((float)_eraseRoomPosition.x, (float)_eraseRoomPosition.y), new Vector2((float)_eraseRoomPosition.x + 1, (float)_eraseRoomPosition.y + 1));
            Gizmos.DrawLine(new Vector2((float)_eraseRoomPosition.x + 1, (float)_eraseRoomPosition.y), new Vector2((float)_eraseRoomPosition.x, (float)_eraseRoomPosition.y + 1));
        }

        if (_levelCreationSteps == LevelCreationSteps.Materials)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector2((float)_materialPosition.x + 0.5f, (float)_materialPosition.y + 0.5f), new Vector3(_tilesSize, _tilesSize, _tilesSize));
            Gizmos.DrawWireSphere(new Vector2((float)_materialPosition.x + 0.5f, (float)_materialPosition.y+ 0.5f), 0.25f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector2((float)_eraseMaterialPosition.x + 0.5f, (float)_eraseMaterialPosition.y + 0.5f), new Vector3(_tilesSize, _tilesSize, _tilesSize));
            Gizmos.DrawLine(new Vector2((float)_eraseMaterialPosition.x, (float)_eraseMaterialPosition.y), new Vector2((float)_eraseMaterialPosition.x + 1, (float)_eraseMaterialPosition.y + 1));
            Gizmos.DrawLine(new Vector2((float)_eraseMaterialPosition.x + 1, (float)_eraseMaterialPosition.y), new Vector2((float)_eraseMaterialPosition.x, (float)_eraseMaterialPosition.y + 1));
        }

        foreach (var door in _allDoors)
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

        foreach(var pos in _allMaterials)
        {
            Gizmos.color = Color.brown;
            Gizmos.DrawWireSphere((Vector2)pos + new Vector2(0.5f, 0.5f), 0.25f);
        }
    }
}

#endif