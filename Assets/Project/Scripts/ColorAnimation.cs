using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorAnimation : MonoBehaviour
{
    [SerializeField] Color _baseColor;
    [SerializeField] Color _endColor;

    [SerializeField] float _duration;
    [SerializeField] AnimationCurve _animCurve;
    float _timing;
    bool _reverse;

    [SerializeField] bool _controlPlay;
    bool _play;

    private void Start()
    {
        _play = !_controlPlay;
    }

    void Update()
    {
        if (_play)
        {
            float ratio = _timing / _duration;
            if (_reverse)
            {
                ratio = 1 - ratio;
            }

            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().color = Color.Lerp(_baseColor, _endColor, _animCurve.Evaluate(ratio));

            if (GetComponent<Image>())
                GetComponent<Image>().color = Color.Lerp(_baseColor, _endColor, _animCurve.Evaluate(ratio));

            if (GetComponentInChildren<TextMeshProUGUI>())
                GetComponentInChildren<TextMeshProUGUI>().color = Color.Lerp(_baseColor, _endColor, _animCurve.Evaluate(ratio));

            if (_timing >= _duration)
            {
                if (_controlPlay) _play = false;
                _timing = 0;
                _reverse = !_reverse;
            }

            _timing += Time.deltaTime;
        }
    }

    public void PlayAnimation()
    {
        _play = true;
        _timing = 0;
    }
}
