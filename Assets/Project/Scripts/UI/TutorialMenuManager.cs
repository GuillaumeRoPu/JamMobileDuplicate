using Frynity.Saves.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _menu;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] Image _languageButtonImage;

    [SerializeField] Sprite _englishSprite;
    [SerializeField] Sprite _frenchSprite;

    private string _frenchText;
    private string _englishText;

    int _languageIndex;

    public void ActivateMenu(Sprite image, string engl, string fr)
    {
        _languageIndex = SaveManager.Of.Settings.languageIndex;
        _menu.SetActive(true);
        if (engl != null && fr != null)
        {
            _englishText = engl;
            _frenchText = fr;

            if (_languageIndex == 0)
            {
                _text.text = engl;
            }
            else
            {
                _text.text = fr;
            }
        }
        if (_image != null) 
        {
            _image.sprite = image;
        }

        _languageButtonImage.sprite = _languageIndex == 0 ? _englishSprite : _frenchSprite;

        Time.timeScale = 0;
    }

    public void DeactivateMenu() 
    {
        Time.timeScale = 1;
        _menu.SetActive(false);
    }

    public void ChangeLanguage()
    {
        _languageIndex = _languageIndex == 0 ? 1 : 0;
        if (_languageIndex == 0 )
        {
            _text.text = _englishText;
        }
        else
        {
            _text.text = _frenchText;
        }

        _languageButtonImage.sprite = _languageIndex == 0 ? _englishSprite : _frenchSprite;

        SaveManager.Of.Settings.languageIndex = _languageIndex;
        SaveManager.Of.SaveSettings();
    }

    private void Start()
    {
        DeactivateMenu();
    }
}
