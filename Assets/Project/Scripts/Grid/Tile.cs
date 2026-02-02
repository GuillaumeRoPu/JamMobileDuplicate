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
        _cellData._gridPosition = gridPosition;
    }

    private void SetColor()
    {
        //_renderer.color = _isOffset ? _offsetColor : _baseColor;
        _renderer.color = _baseColor;
        if (_cellData._isSpawnPoint)
        {
            _renderer.color = Color.red;
        }
    }

    public void OnHover()
    {
        _highlight.SetActive(true);
        Debug.Log(_cellData._gridPosition);
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

    public void SetAsSpawnTile(bool isSpawn)
    {
        _cellData._isSpawnPoint = isSpawn;
        SetColor();
    }
}
