using Frynity.Saves;
using Frynity.Saves.Binary;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] AudioMixer _mixer;

    [SerializeField] Slider _fxSlider;
    [SerializeField] Slider _musicSlider;


    private void Start()
    {
        SaveManager.Of.LoadSettings();

        _mixer.SetFloat("FXVolume", SaveManager.Of.Settings.fxVolume);
        _mixer.SetFloat("MusicVolume", SaveManager.Of.Settings.musicVolume);

        _fxSlider.value = 1 - SaveManager.Of.Settings.fxVolume / (-80);
        _musicSlider.value = 1 - SaveManager.Of.Settings.musicVolume / (-80);
    }

    public void ChangeFXVolume(float volume)
    {
        _mixer.SetFloat("FXVolume", (1 - volume) * -80);

        SaveManager.Of.Settings.fxVolume = (1 - volume) * -80;
        SaveManager.Of.SaveSettings();
    }

    public void ChangeMusicVolume(float volume)
    {
        _mixer.SetFloat("MusicVolume", (1 - volume) * -80);

        SaveManager.Of.Settings.musicVolume = (1 - volume) * -80;
        SaveManager.Of.SaveSettings();
    }
}

[System.Serializable]
public class SettingSave : ISaveData
{
    public float fxVolume;
    public float musicVolume;
}
