
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SoundClip
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.8f, 1.2f)] public float pitch = 1f;
    [Range(0f, 0.2f)] public float pitchVariance = 0f; 
    public bool loop = false;
    [HideInInspector] public AudioSource source;
}

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;        
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [Header("Sound Library")]
    [SerializeField] private SoundClip[] sounds;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Music Sources (for crossfade)")]
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private bool usingSourceA = true;

    private Dictionary<string, SoundClip> soundDict;
    private Coroutine crossfadeRoutine;

    
    protected override void Awake()
    {
        base.Awake();
        BuildDictionary();
        CreateMusicSources();
        LoadVolumeSettings();
    }

    
    private void BuildDictionary()
    {
        soundDict = new Dictionary<string, SoundClip>();

        if (sounds == null) return;

        foreach (var s in sounds)
        {
            if (s == null || string.IsNullOrEmpty(s.name)) continue;
            if (s.clip == null)
            {
                Debug.LogWarning($"[AudioManager] '{s.name}' has no clip assigned");
                continue;
            }
            if (soundDict.ContainsKey(s.name))
            {
                Debug.LogWarning($"[AudioManager] Duplicate sound: '{s.name}'");
                continue;
            }

            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = false;

            // Route through SFX mixer group if mixer exists
            if (audioMixer != null)
            {
                AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("SFX");
                if (groups.Length > 0)
                    s.source.outputAudioMixerGroup = groups[0];
            }

            soundDict[s.name] = s;
        }

        Debug.Log($"[AudioManager] {soundDict.Count} sounds loaded");
    }

    private void CreateMusicSources()
    {
        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceA.loop = true;
        musicSourceA.playOnAwake = false;

        musicSourceB = gameObject.AddComponent<AudioSource>();
        musicSourceB.loop = true;
        musicSourceB.playOnAwake = false;

        // Route through Music mixer group
        if (audioMixer != null)
        {
            AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("Music");
            if (groups.Length > 0)
            {
                musicSourceA.outputAudioMixerGroup = groups[0];
                musicSourceB.outputAudioMixerGroup = groups[0];
            }
        }
    }

    //SFX
    public void PlaySFX(string name)
    {
        if (string.IsNullOrEmpty(name) || soundDict == null) return;

        if (!soundDict.TryGetValue(name, out SoundClip s))
        {
            Debug.LogWarning($"[AudioManager] SFX '{name}' not found");
            return;
        }

        if (s.source == null || s.clip == null) return;

        // Apply pitch variance for natural feel
        s.source.pitch = s.pitch + UnityEngine.Random.Range(-s.pitchVariance, s.pitchVariance);
        s.source.volume = s.volume * sfxVolume * masterVolume;
        s.source.PlayOneShot(s.clip);
    }

    public void PlaySFXAtPosition(string name, Vector3 pos)
    {
        if (string.IsNullOrEmpty(name) || soundDict == null) return;
        if (!soundDict.TryGetValue(name, out SoundClip s)) return;
        if (s.clip == null) return;
        AudioSource.PlayClipAtPoint(s.clip, pos, s.volume * sfxVolume * masterVolume);
    }

    //MUSIC

    //Play music — crossfades if a track is already playing
    public void PlayMusic(AudioClip clip, bool forceRestart = false)
    {
        if (clip == null) return;

        AudioSource current = usingSourceA ? musicSourceA : musicSourceB;
        AudioSource next = usingSourceA ? musicSourceB : musicSourceA;

        // Already playing same clip
        if (current.clip == clip && current.isPlaying && !forceRestart) return;

        if (current.isPlaying)
        {
            // Crossfade to new track
            if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
            crossfadeRoutine = StartCoroutine(CrossFade(current, next, clip, 0.8f));
        }
        else
        {
            // Nothing playing — just start
            current.clip = clip;
            current.volume = musicVolume * masterVolume;
            current.Play();
        }
    }

    private IEnumerator CrossFade(AudioSource from, AudioSource to, AudioClip newClip, float duration)
    {
        // Start new track silently
        to.clip = newClip;
        to.volume = 0f;
        to.Play();

        float startVol = from.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float ratio = t / duration;
            from.volume = Mathf.Lerp(startVol, 0f, ratio);
            to.volume = Mathf.Lerp(0f, musicVolume * masterVolume, ratio);
            yield return null;
        }

        from.Stop();
        from.volume = musicVolume * masterVolume;
        usingSourceA = !usingSourceA;
    }

    public void StopMusic()
    {
        musicSourceA.Stop();
        musicSourceB.Stop();
    }

    public void FadeOutMusic(float duration)
    {
        AudioSource active = usingSourceA ? musicSourceA : musicSourceB;
        StartCoroutine(FadeOut(active, duration));
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVol = source.volume;
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            source.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }
        source.Stop();
        source.volume = startVol;
    }

    //VOLUME CONTROL

    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        ApplyMixerVolume(masterVolumeParam, masterVolume);
        SaveVolumeSettings();
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        ApplyMixerVolume(musicVolumeParam, musicVolume);

        // Also update active source directly
        AudioSource active = usingSourceA ? musicSourceA : musicSourceB;
        active.volume = musicVolume * masterVolume;
        SaveVolumeSettings();
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        ApplyMixerVolume(sfxVolumeParam, sfxVolume);
        SaveVolumeSettings();
    }

    //Convert 0-1 linear to decibels and apply to mixer
    private void ApplyMixerVolume(string paramName, float linearValue)
    {
        if (audioMixer == null) return;
        // Convert linear (0-1) to decibels (-80 to 0)
        float db = linearValue > 0.0001f
            ? Mathf.Log10(linearValue) * 20f
            : -80f;
        audioMixer.SetFloat(paramName, db);
    }

    private void SaveVolumeSettings()
    {
        var data = SaveSystem.Load();
        data.masterVolume = masterVolume;
        data.musicVolume = musicVolume;
        data.sfxVolume = sfxVolume;
        SaveSystem.Save(data);
    }

    private void LoadVolumeSettings()
    {
        var data = SaveSystem.Load();
        masterVolume = data.masterVolume;
        musicVolume = data.musicVolume;
        sfxVolume = data.sfxVolume;

        ApplyMixerVolume(masterVolumeParam, masterVolume);
        ApplyMixerVolume(musicVolumeParam, musicVolume);
        ApplyMixerVolume(sfxVolumeParam, sfxVolume);
    }
}