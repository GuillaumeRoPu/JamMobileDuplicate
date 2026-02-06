using UnityEngine;

[CreateAssetMenu(fileName = "AssetList", menuName = "Room Creator/Asset List")]
public class AssetList : ScriptableObject
{
    [field: SerializeField] public Sprite[] Tiles { get; private set;  }
    [field: SerializeField] public Sprite[] Grounds { get; private set;  }
    [field: SerializeField] public Sprite[] Walls { get; private set; }
    [field: SerializeField] public Sprite[] Props { get; private set; }
}