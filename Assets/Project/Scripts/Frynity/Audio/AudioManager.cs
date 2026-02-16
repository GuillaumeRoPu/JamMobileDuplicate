using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public struct AudioParameter
{
    public AudioClip clip;
    public float startTime;
    public float endTime;
    [Range(0, 1)] public float volume;
}

public class AudioManager : MonoBehaviour
{
    public AudioParameter[] music;
    public AudioParameter[] fx;

    public AudioMixerGroup musicEffectMixer;
    public AudioMixerGroup fxEffectMixer;

    public static AudioManager Of;

    private void Awake()
    {
        if (Of != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de AudioManager dans la scène");
            return;
        }

        Of = this;
    }

    public AudioSource PlayClipAt(AudioParameter param, Vector3 pos, bool fx = false)
    {
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = pos;
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = param.clip;
        audioSource.time = param.startTime;
        audioSource.volume = param.volume;
        audioSource.outputAudioMixerGroup = fx ? fxEffectMixer : musicEffectMixer;
        audioSource.Play();
        if (fx)
            Destroy(tempGO, param.clip.length - param.startTime - param.endTime);
        else
            audioSource.loop = true;
        return audioSource;
    }

    public void PlayAudioFX(AudioParameter param)
    {
        GameObject tempGO = new GameObject("TempAudio");
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = param.clip;
        audioSource.time = param.startTime;
        audioSource.volume = param.volume;
        audioSource.outputAudioMixerGroup = fxEffectMixer;
        audioSource.Play();
        Destroy(tempGO, param.clip.length - param.startTime - param.endTime);
    }

    public void PlayDataAudio(SoundData data)
    {
        if (data.PlayType == PlayType.RANDOMLY)
        {
            PlayClipAt(data.Sounds[Random.Range(0, data.Sounds.Length)], Vector2.zero, !data.Loop);
        }
    }
}
