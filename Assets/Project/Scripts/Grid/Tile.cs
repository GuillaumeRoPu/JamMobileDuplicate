using UnityEngine;

public class Tile : MonoBehaviour, IHoverable
{
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _offsetColor;

    [SerializeField] private SpriteRenderer _renderer;

    [SerializeField] private GameObject _highlight;

    private Vector2 _gridPosition;

    public void Initialize(bool isOffset, Vector2 gridPosition)
    {
        _renderer.color = isOffset ? _offsetColor : _baseColor;
        _gridPosition = gridPosition;
    }

    public void OnHover()
    {
        _highlight.SetActive(true);
        Debug.Log(_gridPosition);
    }

    public void StopHover()
    {
        _highlight.SetActive(false);
    }
}
