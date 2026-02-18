using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberOfTurnsAdded : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Image _image;
    private bool _isInGame = false;
    private int _currentNumber = 0;
    private int _oldNumber = -2;

    private void UpdateTextAndImage()
    {
        UpdateCurrentNumber();
        if (_currentNumber <= 0)
        {
            _currentNumber = 1;
        }
        if (_currentNumber != _oldNumber)
        {
            _oldNumber = -2;
        }
        string strNum = _currentNumber.ToString();
        _text.text = "+" + strNum;
        if (_currentNumber <= 0)
        {
            ActivateImageAndText(false);
        }
        else if (_isInGame && _currentNumber != _oldNumber)
        {
            ActivateImageAndText(true);
        }
        else if (_isInGame)
        {
            ActivateImageAndText(true);
            _text.text = "+" + 1;
        }
    }

    private void ActivateImageAndText(bool activate)
    {
        if (!activate)
        {
            _image.gameObject.SetActive(false);
            _text.gameObject.SetActive(false);
        }
        else
        {
            _image.gameObject.SetActive(true);
            _text.gameObject.SetActive(true);
        }
    }

    private void ActivateTrue()
    {
        _isInGame = true;
        Debug.Log("Active");
    }

    private void ActivateFalse()
    {
        ActivateImageAndText(false);
        _oldNumber = _currentNumber;
        _isInGame = false;
        Debug.Log("Inactive");
    }

    private void UpdateCurrentNumber()
    {
        _currentNumber = GameManager.Of.Player.Path.Count - 1;
    }

    private void Start()
    {
        GameManager.Of.Turn.onNewTurn += ActivateTrue;
        GameManager.Of.Turn.onNextTurn += ActivateFalse;
    }

    private void OnDestroy()
    {
        GameManager.Of.Turn.onNewTurn -= ActivateTrue;
        GameManager.Of.Turn.onNextTurn -= ActivateFalse;
    }

    private void Update()
    {
        UpdateTextAndImage();
    }
}
