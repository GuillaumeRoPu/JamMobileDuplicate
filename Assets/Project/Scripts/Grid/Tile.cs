using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour, IHoverable
{
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _offsetColor;

    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Sprite _inGameSprite;
    [SerializeField] private Sprite _inEditorSprite;

    [SerializeField] private GameObject _highlight;

    private CellData _cellData = new CellData();

    private bool _isInARoom = false;
    private Vector2Int _mainRoomCell = new Vector2Int();
    private RoomData _roomData = new RoomData();

    public CellData CellData { get { return _cellData; } }

    private bool _isOffset;

    public void Initialize(bool isOffset, Vector2Int gridPosition)
    {
        if (Application.isPlaying)
        {
            _renderer.sprite = _inGameSprite;
            _renderer.sortingOrder = 1;
        }
        else
        {
            _renderer.sprite = _inEditorSprite;
            _renderer.sortingOrder = -1;
            _highlight.SetActive(true);
        }
        _isOffset = isOffset;
        SetColor();
        _cellData.GridPosition = gridPosition;
    }

    private void SetColor()
    {
        //_renderer.color = _isOffset ? _offsetColor : _baseColor;
        _renderer.color = _baseColor;
        var rend = _highlight.GetComponent<SpriteRenderer>();
        if (rend != null && !Application.isPlaying)
        {
            rend.color = new Color (0, 0, 0, 0);
            rend.sortingOrder = 100;
        }
        if (_cellData.IsSpawnPoint)
        {
            if (_cellData.spawnPointType.HasFlag(CellData.SpawnPointType.Player))
            {
                _renderer.color = Color.red;
                if (rend != null && !Application.isPlaying) 
                { 
                    rend.color = Color.red;
                    rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 0.5f);
                }
            }
            else
            {
                _renderer.color = Color.blue;
                if (rend != null && !Application.isPlaying)
                {
                    rend.color = Color.blue;
                    rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 0.5f);
                }
            }
        }
    }

    public void OnHover()
    {
        _highlight.SetActive(true);
        Debug.Log(_isInARoom);
        Debug.Log("Main Cell Position | X : " + _roomData.GetMainCell().x + ", Y : " + _roomData.GetMainCell().y);
    }

    public void StopHover()
    {
        _highlight.SetActive(false);
    }

    public void SetColor(Color color)
    {
        _renderer.color = color;
    }

    public Color GetColor()
    {
        return _renderer.color;
    }

    public void AnnihilateTile()
    {
        #if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        #else
        Destroy(gameObject);
        #endif

    }

    public void SetAsSpawnTile(CellData.SpawnPointType type)
    {
        _cellData.IsSpawnPoint = true;
        _cellData.spawnPointType = type;
        SetColor();
    }

    public void RemoveSpawnTile()
    {
        _cellData.IsSpawnPoint = false;
        _cellData.spawnPointType = CellData.SpawnPointType.None;
        SetColor();
    }

    public bool IsInARoom()
    {
        if (_isInARoom)
        {
            return true;
        }
        return false;
    }

    public void SetAsInARoom(Vector2Int mainRoomCellData, RoomData roomData)
    {
        if (!IsInARoom())
        {
            _isInARoom = true;
            _mainRoomCell = mainRoomCellData;
            CellData.HasRoom = true;
            CellData.MainRoomCell = _mainRoomCell;
            if (CellData.GridPosition == mainRoomCellData)
            {
                _roomData = roomData;
            }
        }
        else
        {
            Debug.Log("This Tile is not in a room");
        }
    }

    public void SetAsNotInARoom()
    {
        if (_isInARoom)
        {
            _isInARoom = false;
            _mainRoomCell = new Vector2Int();
            _roomData = new RoomData();
        }
        else
        {
            Debug.Log("This Tile is already not in a room");
        }
    }

    public RoomData GetRoomData()
    {
        if (_mainRoomCell == CellData.GridPosition)
        {
            return _roomData;
        }
        else
        {
            return GameManager.Of.Grid.GetTileAtPosition(GetMainRoomCell()).GetRoomData();
        }
    }

    public Vector2Int GetMainRoomCell()
    {
        if (!_isInARoom)
        {
            Debug.Log(CellData.GridPosition);
            Debug.LogError("This cell isn't a room");
            return new Vector2Int();
        }
        else
        {
            return _mainRoomCell;
        }
    }

    private void OnDestroy()
    {
        if (CellData.tileTrap != null)
        {
            CellData.tileTrap.onDestroy -= CellData.RemoveTileTrap;
        }
        if (CellData.doorTrap != null)
        {
            CellData.doorTrap.onDestroy -= CellData.RemoveDoorTrap;
        }
    }
}
