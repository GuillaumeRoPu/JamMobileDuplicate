using UnityEngine;
using Frynity.Saves;

[System.Serializable]
public class SettingsData : ISaveData
{
    public float fxVolume;
    public float musicVolume;
}