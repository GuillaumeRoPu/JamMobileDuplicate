using UnityEngine;

public class ColorAnimation : MonoBehaviour
{
    [SerializeField] Color _baseColor;
    [SerializeField] Color _endColor;

    [SerializeField] float _duration;
    [SerializeField] AnimationCurve _animCurve;
    float _timing;
    bool _reverse;

    void Update()
    {
        float ratio = _timing / _duration;
        if (_reverse)
        {
            ratio = 1 - ratio;
        }

        GetComponent<SpriteRenderer>().color = Color.Lerp(_baseColor, _endColor, _animCurve.Evaluate(ratio));

        if (_timing >= _duration)
        {
            _timing = 0;
            _reverse = !_reverse;
        }
        _timing += Time.deltaTime;
    }
}
