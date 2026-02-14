using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum IconType
{
    ZOMBIE, ECHO, WITCH
}

public class MonsterContent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _countTxt;
    [SerializeField] Image _iconImg;

    [SerializeField] Sprite _zombieIcon;
    [SerializeField] Sprite _echoIcon;
    [SerializeField] Sprite _witchIcon;


    public void SetIcon(IconType type, int count)
    {
        switch (type)
        {
            case IconType.ZOMBIE: _iconImg.sprite = _zombieIcon; break;
            case IconType.ECHO: _iconImg.sprite = _echoIcon; break;
            case IconType.WITCH: _iconImg.sprite = _witchIcon; break;
        }

        _countTxt.text = string.Format("x{0}", count);
    }
}
