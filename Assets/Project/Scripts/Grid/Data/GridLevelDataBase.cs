#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class GridLevelDataBase
{
    private const string AssetPath = "Assets/Project/Scripts/Grid/Data/GridLevelData.asset";

    public static GridLevelData GetOrCreate(string name = AssetPath)
    {
        var dir = Path.GetDirectoryName(AssetPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        string newAssetPath = dir + "/" + name + ".asset";
        var data = ScriptableObject.CreateInstance<GridLevelData>();
        AssetDatabase.CreateAsset(data, newAssetPath);
        AssetDatabase.SaveAssets();
        return data;
    }

    public static void Save(GridLevelData data)
    {
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }
}
#endif

