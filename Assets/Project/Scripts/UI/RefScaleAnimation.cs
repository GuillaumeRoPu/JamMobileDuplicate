using UnityEngine;

public class RefScaleAnimation : MonoBehaviour
{
    [SerializeField] Transform _ref;
    [SerializeField] float _duration;

    Vector3 _initScale;
    Vector3 _initPosition;

    float _current;
    bool _play;

    private void Update()
    {
        if (_play)
        {
            float ratio = _current / _duration;

            transform.localScale = Vector3.Lerp(_initScale, _ref.localScale, ratio);
            transform.position = Vector3.Lerp(_initPosition, _ref.position, ratio);

            if (_current >= _duration)
            {
                _play = false;
            }

            _current += Time.deltaTime;
        }
    }

    public void PlayAnimation()
    {
        _play = true;
        _current = 0;
        _initScale = transform.localScale;
        _initPosition = transform.position;
    }
}
