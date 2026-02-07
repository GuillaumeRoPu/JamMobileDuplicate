using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "FryAudio/Sound Data")]
public class SoundData : ScriptableObject
{
    [field: SerializeField] public AudioParameter[] Sounds { get; private set; }
    [field: SerializeField] public bool Loop { get; private set; }
    [field: SerializeField] public PlayType PlayType { get; private set; }
}

public enum PlayType
{
    RANDOMLY, QUEUE, SELECTED
}