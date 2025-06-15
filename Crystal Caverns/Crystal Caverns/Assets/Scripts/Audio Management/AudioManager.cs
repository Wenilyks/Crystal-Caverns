using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Music")]
    [SerializeField] public List<Sound> musicTracks;
    [SerializeField] private float musicFadeDuration = 1f;

    [Header("Sound Effects")]
    [SerializeField] public List<Sound> soundEffects;
    [SerializeField] private int sfxPoolSize = 10;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    // Audio source pools
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    private List<AudioSource> activeSfxSources = new List<AudioSource>();

    // Current music tracking
    private AudioSource currentMusicSource;
    private AudioSource fadingMusicSource;
    private string currentMusicName;

    // Volume control
    public static event Action<float> OnMasterVolumeChanged;
    public static event Action<float> OnMusicVolumeChanged;
    public static event Action<float> OnSfxVolumeChanged;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudio();
        LoadVolumeSettings();
    }

    private void InitializeAudio()
    {
        // Initialize music tracks
        foreach (Sound music in musicTracks)
        {
            music.source = gameObject.AddComponent<AudioSource>();
            music.source.clip = music.clip;
            music.source.volume = music.volume;
            music.source.pitch = music.pitch;
            music.source.loop = music.loop;
            music.source.playOnAwake = music.playOnAwake;
            music.source.outputAudioMixerGroup = musicMixerGroup;

            if (music.playOnAwake)
            {
                PlayMusic(music.name);
            }
        }

        // Initialize SFX pool
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
            sfxSource.playOnAwake = false;
            sfxPool.Enqueue(sfxSource);
        }
    }

    public void PlayMusic(string name)
    {
        Sound music = musicTracks.Find(sound => sound.name == name);
        if (music == null)
        {
            Debug.LogWarning($"Music track '{name}' not found!");
            return;
        }

        if (currentMusicName == name && currentMusicSource != null && currentMusicSource.isPlaying)
        {
            return; // Already playing this track
        }

        StartCoroutine(PlayMusicWithFade(music));
    }

    private IEnumerator PlayMusicWithFade(Sound newMusic)
    {
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            fadingMusicSource = currentMusicSource;
            yield return StartCoroutine(FadeAudioSource(fadingMusicSource, 0f, musicFadeDuration));
            fadingMusicSource.Stop();
            fadingMusicSource = null;
        }

        currentMusicSource = newMusic.source;
        currentMusicName = newMusic.name;

        currentMusicSource.volume = 0f;
        currentMusicSource.Play();
        yield return StartCoroutine(FadeAudioSource(currentMusicSource, newMusic.volume * musicVolume, musicFadeDuration));
    }

    public void StopMusic()
    {
        if (currentMusicSource != null)
        {
            StartCoroutine(StopMusicWithFade());
        }
    }

    private IEnumerator StopMusicWithFade()
    {
        yield return StartCoroutine(FadeAudioSource(currentMusicSource, 0f, musicFadeDuration));
        currentMusicSource.Stop();
        currentMusicSource = null;
        currentMusicName = null;
    }

    public void PauseMusic()
    {
        if (currentMusicSource != null)
        {
            currentMusicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (currentMusicSource != null)
        {
            currentMusicSource.UnPause();
        }
    }

    public bool IsMusicPlaying()
    {
        return currentMusicSource != null && currentMusicSource.isPlaying;
    }

    public string GetCurrentMusicName()
    {
        return currentMusicName;
    }

    public void PlaySFX(string name)
    {
        Sound sfx = soundEffects.Find(sound => sound.name == name);
        if (sfx == null)
        {
            Debug.LogWarning($"Sound effect '{name}' not found!");
            return;
        }
        Debug.Log("Playing sfx");

        PlaySFX(sfx.clip, sfx.volume, sfx.pitch);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource sfxSource = GetPooledSFXSource();
        if (sfxSource == null) return;

        sfxSource.clip = clip;
        sfxSource.volume = volume * sfxVolume;
        sfxSource.pitch = pitch;
        sfxSource.Play();
        Debug.Log("SFX played");

        activeSfxSources.Add(sfxSource);
        StartCoroutine(ReturnSFXToPool(sfxSource, clip.length / pitch));
    }

    public void PlaySFXAtPosition(string name, Vector3 position)
    {
        Sound sfx = soundEffects.Find(sound => sound.name == name);
        if (sfx == null)
        {
            Debug.LogWarning($"Sound effect '{name}' not found!");
            return;
        }

        AudioSource.PlayClipAtPoint(sfx.clip, position, sfx.volume * sfxVolume);
    }

    public void StopAllSFX()
    {
        foreach (AudioSource source in activeSfxSources)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    private AudioSource GetPooledSFXSource()
    {
        if (sfxPool.Count > 0)
        {
            return sfxPool.Dequeue();
        }

        // If pool is empty, try to find an inactive source
        foreach (AudioSource source in activeSfxSources)
        {
            if (!source.isPlaying)
            {
                activeSfxSources.Remove(source);
                return source;
            }
        }

        Debug.LogWarning("SFX pool exhausted! Consider increasing pool size or optimizing SFX usage.");
        return null;
    }

    private IEnumerator ReturnSFXToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeSfxSources.Contains(source))
        {
            activeSfxSources.Remove(source);
            sfxPool.Enqueue(source);
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        OnMasterVolumeChanged?.Invoke(masterVolume);
        SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume();
        OnMusicVolumeChanged?.Invoke(musicVolume);
        SaveVolumeSettings();
        Debug.Log($"volume: {volume}");
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        OnSfxVolumeChanged?.Invoke(sfxVolume);
        SaveVolumeSettings();
    }

    private void UpdateAllVolumes()
    {
        UpdateMusicVolume();
        // SFX volume is applied per-sound when played
    }

    private void UpdateMusicVolume()
    {
        if (currentMusicSource != null)
        {
            Sound currentMusic = musicTracks.Find(sound => sound.source == currentMusicSource);
            if (currentMusic != null)
            {
                currentMusicSource.volume = currentMusic.volume * musicVolume * masterVolume;
            }
        }
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);

        UpdateAllVolumes();
    }

    public void ResetVolumeSettings()
    {
        SetMasterVolume(1f);
        SetMusicVolume(1f);
        SetSfxVolume(1f);
    }

    private IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            if (source == null) yield break;

            source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        if (source != null)
        {
            source.volume = targetVolume;
        }
    }

    public void PlayRandomSFX(string[] sfxNames)
    {
        if (sfxNames.Length == 0) return;

        string randomName = sfxNames[UnityEngine.Random.Range(0, sfxNames.Length)];
        PlaySFX(randomName);
    }

    public bool IsSFXPlaying(string name)
    {
        foreach (AudioSource source in activeSfxSources)
        {
            if (source.isPlaying && source.clip != null && source.clip.name == name)
            {
                return true;
            }
        }
        return false;
    }
}