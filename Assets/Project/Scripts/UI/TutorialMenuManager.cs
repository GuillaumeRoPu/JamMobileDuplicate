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
        _text.text = str;
        _image.sprite = image;
    }
}
