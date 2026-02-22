using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrapToUse
{
    public bool Net;
    public bool Log;
    public bool Blocker;
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Granny/Level Data")]
public class LevelData : ScriptableObject
{
    [field: SerializeField] public GridLevelData GridLevel { get; private set; }
    [field: SerializeField, MinValue(1)] public int TurnMax { get; private set; }
    [field: SerializeField] public TurnEvent[] Events { get; private set; }
    [field: SerializeField] public TrapToUse TrapToUse { get; private set; }

    [field: SerializeField] public bool HasTutorial { get; private set; }

    [field: SerializeField, ShowIf(nameof(HasTutorial), false, true)] public Sprite TutoImage { get; private set; }

    [field: SerializeField, ShowIf(nameof(HasTutorial), false, true), TextArea(5, 10)] public string TutoTextEnglish { get; private set; }
    [field: SerializeField, ShowIf(nameof(HasTutorial), false, true), TextArea(5, 10)] public string TutoTextFrench { get; private set; }

    Dictionary<int, List<Vector2Int>> _spawnTiles;
    public List<Vector2Int> GetNextSpawn(int turn)
    {
        return _spawnTiles[turn];
    }

    public void Init()
    {
        _spawnTiles = new Dictionary<int, List<Vector2Int>>();
        foreach (var evnt in Events)
        {
            if (evnt.EventType != EventType.Zombie && evnt.EventType != EventType.Witch) continue;

            if (_spawnTiles.ContainsKey(evnt.Turn))
            {
                _spawnTiles[evnt.Turn].Add(GameManager.Of.Grid.GridLevelData.keys[evnt.SpawnCellIndex]);
            }
            else
            {
                _spawnTiles.Add(evnt.Turn, new List<Vector2Int>() {
                GameManager.Of.Grid.GridLevelData.keys[evnt.SpawnCellIndex] });
            }
        }
    }
}