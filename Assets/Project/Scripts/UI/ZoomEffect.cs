using UnityEngine;
using UnityEngine.Events;

public class ZoomEffect : MonoBehaviour
{
    float _currentZoom;
    float _maxZoom;
    bool _zoom;

    [SerializeField] float _duration;

    [SerializeField] UnityEvent _zoomFinish;

    void Update()
    {
        if (_zoom)
        {
            float ratio = _currentZoom / _duration;

            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * _maxZoom, ratio);

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
        _currentZoom = 0;
    }
}
