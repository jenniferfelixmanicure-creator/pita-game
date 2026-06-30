using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gerenciador de áudio global. Suporta músicas dinâmicas, SFX e fading.
/// Singleton persistente entre cenas.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitch = 1f;
        public bool loop = false;
        [HideInInspector] public AudioSource source;
    }

    [Header("Sons")]
    [SerializeField] private Sound[] sounds;
    [SerializeField] private Sound[] musicTracks;

    [Header("Configurações")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.6f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float fadeDuration = 1f;

    private AudioSource musicSource;
    private AudioSource musicSource2; // Para crossfade
    private bool usingSource1 = true;
    private Dictionary<string, Sound> soundDict = new Dictionary<string, Sound>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Criar fontes de áudio para cada som
        foreach (var s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume * sfxVolume * masterVolume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            soundDict[s.name] = s;
        }

        // Fontes de música para crossfade
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource2 = gameObject.AddComponent<AudioSource>();
        musicSource2.loop = true;

        LoadAudioSettings();
    }

    // --- SFX ---

    public void Play(string soundName)
    {
        if (!soundDict.TryGetValue(soundName, out Sound s)) return;
        s.source.volume = s.volume * sfxVolume * masterVolume;
        s.source.pitch = s.pitch + Random.Range(-0.05f, 0.05f);
        s.source.PlayOneShot(s.clip);
    }

    public void PlayAt(string soundName, Vector3 position)
    {
        if (!soundDict.TryGetValue(soundName, out Sound s)) return;
        AudioSource.PlayClipAtPoint(s.clip, position, s.volume * sfxVolume * masterVolume);
    }

    public void Stop(string soundName)
    {
        if (!soundDict.TryGetValue(soundName, out Sound s)) return;
        s.source.Stop();
    }

    // --- Música ---

    public void PlayMusic(string trackName)
    {
        var track = System.Array.Find(musicTracks, m => m.name == trackName);
        if (track == null) return;

        StartCoroutine(CrossfadeMusic(track.clip, track.volume * musicVolume * masterVolume));
    }

    public void StopMusic()
    {
        StartCoroutine(FadeOut(usingSource1 ? musicSource : musicSource2));
    }

    private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip, float targetVolume)
    {
        var current = usingSource1 ? musicSource : musicSource2;
        var next = usingSource1 ? musicSource2 : musicSource;
        usingSource1 = !usingSource1;

        next.clip = newClip;
        next.volume = 0f;
        next.Play();

        float elapsed = 0f;
        float startVol = current.volume;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            current.volume = Mathf.Lerp(startVol, 0f, t);
            next.volume = Mathf.Lerp(0f, targetVolume, t);
            yield return null;
        }
        current.Stop();
    }

    private System.Collections.IEnumerator FadeOut(AudioSource src)
    {
        float startVol = src.volume;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, elapsed / fadeDuration);
            yield return null;
        }
        src.Stop();
    }

    // --- Volume ---

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        RefreshVolumes();
        SaveAudioSettings();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        RefreshVolumes();
        SaveAudioSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        RefreshVolumes();
        SaveAudioSettings();
    }

    private void RefreshVolumes()
    {
        foreach (var s in sounds)
            if (s.source) s.source.volume = s.volume * sfxVolume * masterVolume;
        musicSource.volume = musicVolume * masterVolume;
        musicSource2.volume = musicVolume * masterVolume;
    }

    // --- Persistência ---

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MasterVol", masterVolume);
        PlayerPrefs.SetFloat("MusicVol", musicVolume);
        PlayerPrefs.SetFloat("SFXVol", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVol", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVol", 0.6f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVol", 1f);
    }

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
}
