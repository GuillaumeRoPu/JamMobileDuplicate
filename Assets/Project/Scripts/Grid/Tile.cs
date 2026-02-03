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

    public CellData CellData { get { return _cellData; } }

    private bool _isOffset;

    public void Initialize(bool isOffset, Vector2Int gridPosition)
    {
        if (Application.isPlaying)
        {
            _renderer.sprite = _inGameSprite;
        }
        else
        {
            _renderer.sprite = _inEditorSprite;
        }
        _isOffset = isOffset;
        SetColor();
        _cellData.gridPosition = gridPosition;
    }

    private void SetColor()
    {
        //_renderer.color = _isOffset ? _offsetColor : _baseColor;
        _renderer.color = _baseColor;
        if (_cellData.isSpawnPoint)
        {
            if (_cellData.spawnPointType.HasFlag(CellData.SpawnPointType.Player))
            {
                _renderer.color = Color.red;
            }
            else
            {
                _renderer.color = Color.blue;
            }
        }
    }

    public void OnHover()
    {
        _highlight.SetActive(true);
        Debug.Log(_cellData.gridPosition);
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
        if (!EditorApplication.isPlaying)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetAsSpawnTile(CellData.SpawnPointType type)
    {
        _cellData.isSpawnPoint = true;
        _cellData.spawnPointType = type;
        SetColor();
    }

    public void RemoveSpawnTile()
    {
        _cellData.isSpawnPoint = false;
        _cellData.spawnPointType = CellData.SpawnPointType.None;
        SetColor();
    }
}
