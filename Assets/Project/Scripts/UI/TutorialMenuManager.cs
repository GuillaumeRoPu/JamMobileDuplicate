using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _menu;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _text;

    public void ActivateMenu(Sprite image, string str)
    {
        _menu.SetActive(true);
        if (str != null)
        {
            _text.text = str;
        }
        if (_image != null) 
        {
            _image.sprite = image;
        }
        Time.timeScale = 0;
    }

    public void DeactivateMenu() 
    {
        Time.timeScale = 1;
        _menu.SetActive(false);
    }

    private void Start()
    {
        DeactivateMenu();
    }
}
