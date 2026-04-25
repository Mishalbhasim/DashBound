using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SoundClip
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.8f, 1.2f)] public float pitch = 1f;
    public bool loop = false;
    [HideInInspector] public AudioSource source;
}

public class AudioManager : Singleton<AudioManager>
{
    [Header("Sound Library")]
    [SerializeField] private SoundClip[] sounds;

    [Header("Volume Settings")]
    [Range(0, 1)] public float masterVolume = 1f;
    [Range(0, 1)] public float musicVolume = 0.7f;
    [Range(0, 1)] public float sfxVolume = 1f;

    private AudioSource musicSource;
    private Dictionary<string, SoundClip> soundDict;

    protected override void Awake()
    {
        base.Awake();
        BuildDictionary();
        CreateMusicSource();
    }

    private void BuildDictionary()
    {
        soundDict = new Dictionary<string, SoundClip>();
        foreach (var s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            soundDict[s.name] = s;
        }
    }

    private void CreateMusicSource()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
    }

    public void PlaySFX(string name)
    {
        if (!soundDict.TryGetValue(name, out SoundClip s))
        {
            Debug.LogWarning($"Sound '{name}' not found!");
            return;
        }
        s.source.volume = s.volume * sfxVolume * masterVolume;
        s.source.PlayOneShot(s.clip);
    }

    public void PlaySFXAtPosition(string name, Vector3 pos)
    {
        if (!soundDict.TryGetValue(name, out SoundClip s)) return;
        AudioSource.PlayClipAtPoint(s.clip, pos, s.volume * sfxVolume * masterVolume);
    }

    public void PlayMusic(AudioClip clip, bool forceRestart = false)
    {
        if (musicSource.clip == clip && !forceRestart) return;
        musicSource.clip = clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    public void SetMusicVolume(float v)
    {
        musicVolume = v;
        musicSource.volume = v * masterVolume;
    }

    public void SetSFXVolume(float v) => sfxVolume = v;

    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeOut(duration));
    }

    private System.Collections.IEnumerator FadeOut(float duration)
    {
        float startVol = musicSource.volume;
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0, t / duration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = startVol;
    }
}