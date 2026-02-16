using Frynity.Saves.Binary;
using TMPro;
using UnityEngine;

public class LevelSelectorUI : MonoBehaviour
{
    [SerializeField] GameObject _lockObject;
    [SerializeField] GameObject _playButtonObject;
    [SerializeField] Transform _monsterContainer;
    [SerializeField] GameObject _monsterContent;
    [SerializeField] TextMeshProUGUI _sizeText;
    [SerializeField] TextMeshProUGUI _levelText;
    [SerializeField] LevelData[] _levels;

    int _index = 0;

    private void OnEnable()
    {
        ChangeLevel(0);
    }

    private void Start()
    {
        SaveManager.Of.LoadGame();
    }

    public void ChangeLevel(int indentation)
    {
        _index += indentation;
        if (_index < 0) _index = _levels.Length - 1;
        if (_index >= _levels.Length) _index = 0;

        _lockObject.SetActive(SaveManager.Of.Level.LevelUnlock < _index);
        _playButtonObject.SetActive(SaveManager.Of.Level.LevelUnlock >= _index);


        ShowLevelStat();

        _levelText.text = string.Format("Level {0}", _index + 1);
    }

    void ShowLevelStat()
    {
        int z = 0;
        int e = 0;
        int w = 0;
        foreach (var evnt in _levels[_index].Events)
        {
            if (evnt.EventType == EventType.Zombie)
            {
                z++;
            }

            if (evnt.EventType == EventType.EchoEnter)
            {
                e++;
            }

            if (evnt.EventType == EventType.Witch)
            {
                w++;
            }
        }


        for (int i = 0; i < _monsterContainer.childCount; i++)
        {
            Destroy(_monsterContainer.GetChild(i).gameObject);
        }

        if (z > 0)
        {
            GameObject container = Instantiate(_monsterContent, _monsterContainer);
            MonsterContent script = container.GetComponent<MonsterContent>();
            script.SetIcon(IconType.ZOMBIE, z);
        }

        if (e > 0)
        {
            GameObject container = Instantiate(_monsterContent, _monsterContainer);
            MonsterContent script = container.GetComponent<MonsterContent>();
            script.SetIcon(IconType.ECHO, e);
        }

        if (w > 0)
        {
            GameObject container = Instantiate(_monsterContent, _monsterContainer);
            MonsterContent script = container.GetComponent<MonsterContent>();
            script.SetIcon(IconType.WITCH, w);
        }

        int count = 0;
        foreach ( var room in _levels[_index].GridLevel.allRoomsCells)
        {
            foreach (var tile in room.items)
            {
                count++;
            }
        }
        _sizeText.text = string.Format("x{0} tiles", count);
    }

    public void PlayLevel()
    {
        GameManager.Of.SetCurrentLevel(_index);
        GameManager.Of.Turn.SetLevelData(_levels[_index]);
        GameManager.Of.Grid.SetLevelData(_levels[_index].GridLevel);

        GameManager.Of.stateMachine.ChangeState(GameManager.Of.playState);
    }
}