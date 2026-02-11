using TMPro;
using UnityEngine;

public class LevelSelectorUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _levelText;
    [SerializeField] LevelData[] _levels;

    int _index = 0;

    public void ChangeLevel(int indentation)
    {
        _index += indentation;
        if (_index < 0) _index = _levels.Length - 1;
        if (_index >= _levels.Length) _index = 0;

        _levelText.text = string.Format("Level {0}", _index + 1);
    }
}