using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorAnimation : MonoBehaviour
{
    [SerializeField] Color _baseColor;
    [SerializeField] Color _endColor;

    [SerializeField] float _duration;
    [SerializeField] AnimationCurve _animCurve;
    [SerializeField] List<Image> _allButtons;
    [SerializeField] List<ParticleSystem> _allParticles;
    [SerializeField] private bool _darken = false;
    float _timing;
    bool _reverse;

    [SerializeField] bool _controlPlay;
    bool _play;

    private void Start()
    {
        _play = !_controlPlay;
        foreach (var particle in _allParticles) 
        { 
            particle.Stop();
        }
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

            Color baseColor = new Color(0, 0, 0, 0);
            Color endColor = new Color(0, 0, 0, 0.5f);

            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().color = Color.Lerp(_baseColor, _endColor, _animCurve.Evaluate(ratio));

            if (GetComponent<Image>())
                GetComponent<Image>().color = Color.Lerp(_baseColor, _endColor, _animCurve.Evaluate(ratio));

            if (GetComponentInChildren<TextMeshProUGUI>())
                GetComponentInChildren<TextMeshProUGUI>().color = Color.Lerp(_baseColor, _endColor, _animCurve.Evaluate(ratio));

            if (_darken)
            {
                if (GetComponent<SpriteRenderer>())
                    GetComponent<SpriteRenderer>().color = Color.Lerp(baseColor, endColor, _animCurve.Evaluate(ratio));

                if (GetComponent<Image>())
                    GetComponent<Image>().color = Color.Lerp(baseColor, endColor, _animCurve.Evaluate(ratio));

                if (GetComponentInChildren<TextMeshProUGUI>())
                    GetComponentInChildren<TextMeshProUGUI>().color = Color.Lerp(baseColor, endColor, _animCurve.Evaluate(ratio));
            }

            foreach(var button in _allButtons)
            {
                button.color = Color.Lerp(_baseColor, _endColor, _animCurve.Evaluate(ratio));
            }

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
        _reverse = false;
        foreach (var particle in _allParticles)
        {
            particle.Play();
        }
    }

}
