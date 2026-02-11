using UnityEngine;
using UnityEngine.Events;

public class ZoomEffect : MonoBehaviour
{
    Vector3 _initPivot;
    float _currentZoom;
    float _maxZoom;
    bool _zoom;

    [SerializeField] UnityEvent _zoomFinish;

    void Update()
    {
        if (_zoom)
        {
            float ratio = _currentZoom / 1;

            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * _maxZoom, ratio);
            GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(_initPivot, new Vector3(0, _initPivot.y, _initPivot.z), ratio);

            _currentZoom += Time.deltaTime;

            if (ratio >= 1)
            {
                _zoom = false;
                _zoomFinish?.Invoke();
            }
        }
    }

    public void ZoomOn(float zoom)
    {
        _zoom = true;
        _maxZoom = zoom;
        _initPivot = GetComponent<RectTransform>().anchoredPosition;
    }
}
