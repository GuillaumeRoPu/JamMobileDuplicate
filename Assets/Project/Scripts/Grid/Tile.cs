using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour, IHoverable
{
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _offsetColor;

    [SerializeField] private SpriteRenderer _renderer;

    [SerializeField] private GameObject _highlight;

    private CellData _cellData = new CellData();

    public bool _isOffset;

    public void Initialize(bool isOffset, Vector2Int gridPosition)
    {
        _isOffset = isOffset;
        _renderer.color = isOffset ? _offsetColor : _baseColor;
        if (_cellData._isSpawnPoint)
        {
            _renderer.color = Color.red;
        }
        _cellData._gridPosition = gridPosition;
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
        _renderer.color = _isOffset ? _offsetColor : _baseColor;
        if (_cellData._isSpawnPoint)
        {
            _renderer.color = Color.red;
        }
    }
}
