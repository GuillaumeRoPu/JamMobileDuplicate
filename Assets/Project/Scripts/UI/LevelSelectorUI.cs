using Frynity.Gen;
using Frynity.Utils;
using TMPro;
using UnityEngine;

public class LevelSelectorUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _levelText;
    [SerializeField] LevelData[] _levels;

    GameObject _preview;

    int _index = 0;

    private void OnEnable()
    {
        ChangeLevel(0);
    }

    public void ChangeLevel(int indentation)
    {
        _index += indentation;
        if (_index < 0) _index = _levels.Length - 1;
        if (_index >= _levels.Length) _index = 0;

        Destroy(_preview);
        ShowLevelGrid();

        _levelText.text = string.Format("Level {0}", _index + 1);
    }

    void ShowLevelGrid()
    {
        GameObject level = new GameObject("Level");
        for (int i = 0; i < _levels[_index].GridLevel.allRoomsPosition.Count; i++)
        {
            GameObject room = Instantiate(_levels[_index].GridLevel.allRoomsPrefab[i],(Vector2)_levels[_index].GridLevel.allRoomsPosition[i] * GameManager.Of.Grid.TileSize, 
                Quaternion.identity);

            foreach (var sprite in room.GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.sortingLayerName = "GUI";
                sprite.sortingOrder = 2;
            }

            room.transform.SetParent(level.transform);
        }
        level.transform.localScale = Vector3.one * 0.3f;
        _preview = level;
    }

    public void PlayLevel()
    {
        Destroy(_preview);
        GameManager.Of.Turn.SetLevelData(_levels[_index]);
        GameManager.Of.Grid.SetLevelData(_levels[_index].GridLevel);

        GameManager.Of.stateMachine.ChangeState(GameManager.Of.playState);
    }
}