using TMPro;
using UnityEngine;

public class IconPlayerCurrentTurns : MonoBehaviour
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

    public void UpdateText()
    {
        string strNum = GameManager.Of.Turn.Turn.ToString();
        if (strNum.Length == 1)
        {
            strNum = 0 + strNum;
        }
        _text.text = strNum;
    }

    private void Start()
    {
        GameManager.Of.Turn.onAddTurn += UpdateText;
    }

    private void OnDestroy()
    {
        GameManager.Of.Turn.onAddTurn -= UpdateText;
    }
}
