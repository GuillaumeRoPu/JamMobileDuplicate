using TMPro;
using UnityEngine;

public class IconTurnsLeft : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    public void SetNumber(int number)
    {
        string strNum = number.ToString();
        if (strNum.Length == 1)
        {
            strNum = 0 + strNum;
        }
        _text.text = strNum;
    }
}
