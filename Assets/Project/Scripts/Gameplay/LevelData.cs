using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Granny/Level Data")]
public class LevelData : ScriptableObject
{
    [field: SerializeField] public int TurnMax { get; private set; }
    [field: SerializeField] public TurnEvent[] Events { get; private set; }
}
