using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips (auto-loaded from Resources/Sound if empty)")]
    [SerializeField] private AudioClip[] audioClips;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Hierarchy (0-1)")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.35f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.85f;
    [Range(0f, 1f)][SerializeField] private float engineVolume = 0.45f;
    [Range(0f, 1f)][SerializeField] private float uiVolume = 0.5f;

    private AudioSource _engineSource;
    private readonly List<AudioSource> _spatialPool = new List<AudioSource>();
    private int _spatialIndex;
    private const int SpatialPoolSize = 4;

    private Dictionary<string, AudioClip> audioDictionary;
    private Coroutine _musicFadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioClips == null || audioClips.Length == 0)
        {
            audioClips = Resources.LoadAll<AudioClip>("Sound");
        }

        InitializeAudioDictionary();
        InitializeSources();

        if (musicSource.clip == null && !musicSource.isPlaying)
        {
            PlayMusic("menu_music");
        }
    }

    private void InitializeAudioDictionary()
    {
        audioDictionary = new Dictionary<string, AudioClip>();

        foreach (AudioClip clip in audioClips)
        {
            if (clip == null)
                continue;

            if (audioDictionary.ContainsKey(clip.name))
            {
                Debug.LogWarning(
                    $"AudioManager: Duplicate audio clip name '{clip.name}'."
                );

                continue;
            }

            audioDictionary.Add(clip.name, clip);
        }
    }

    private void InitializeSources()
    {
        if (musicSource != null)
        {
            musicSource.volume = masterVolume * musicVolume;
            musicSource.spatialBlend = 0f;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = 1f;
            sfxSource.spatialBlend = 0f;
            sfxSource.pitch = 1f;
        }

        _engineSource = gameObject.AddComponent<AudioSource>();
        _engineSource.loop = true;
        _engineSource.playOnAwake = false;
        _engineSource.spatialBlend = 0f;
        _engineSource.volume = 0f;

        for (int i = 0; i < SpatialPoolSize; i++)
        {
            GameObject spatialObject = new GameObject($"SpatialSFX_{i}");
            spatialObject.transform.SetParent(transform);
            AudioSource spatial = spatialObject.AddComponent<AudioSource>();
            spatial.playOnAwake = false;
            spatial.spatialBlend = 1f;
            spatial.minDistance = 5f;
            spatial.maxDistance = 60f;
            spatial.volume = 1f;
            _spatialPool.Add(spatial);
        }
    }

    private bool GetClipProfile(string audioName, out float volume, out float pitchMin, out float pitchMax)
    {
        volume = 0.8f;
        pitchMin = 1f;
        pitchMax = 1f;

        switch (audioName)
        {
            case "bump_hit": volume = 0.85f; pitchMin = 0.9f; pitchMax = 1.1f; return true;
            case "wall_bounce": volume = 0.7f; pitchMin = 0.9f; pitchMax = 1.15f; return true;
            case "dash_whoosh": volume = 0.8f; pitchMin = 0.95f; pitchMax = 1.05f; return true;
            case "jump": volume = 0.75f; pitchMin = 0.95f; pitchMax = 1.05f; return true;
            case "kill": volume = 1f; pitchMin = 0.95f; pitchMax = 1.05f; return true;
            case "respawn": volume = 0.8f; pitchMin = 0.95f; pitchMax = 1.05f; return true;
            case "reward_painter": volume = 0.9f; return true;
            case "timeout_buzzer": volume = 1f; return true;
            case "timer_tick": volume = 0.55f; return true;
            case "countdown_blip": volume = 0.85f; return true;
            case "ui_click": volume = 0.5f; pitchMin = 0.95f; pitchMax = 1.05f; return true;
            case "ui_over": volume = 0.35f; pitchMin = 0.95f; pitchMax = 1.05f; return true;
            default: return false;
        }
    }

    private bool GetFallbackClip(string audioName, out string fallbackName, out float volume, out float pitchMin, out float pitchMax)
    {
        switch (audioName)
        {
            case "land": fallbackName = "bump_hit"; volume = 0.6f; pitchMin = 0.6f; pitchMax = 0.7f; return true;
            case "death_fall": fallbackName = "dash_whoosh"; volume = 0.7f; pitchMin = 0.55f; pitchMax = 0.65f; return true;
            case "capture": fallbackName = "reward_painter"; volume = 0.85f; pitchMin = 1f; pitchMax = 1f; return true;
            case "countdown_go": fallbackName = "countdown_blip"; volume = 1f; pitchMin = 1.28f; pitchMax = 1.34f; return true;
            default: fallbackName = null; volume = 0f; pitchMin = 1f; pitchMax = 1f; return false;
        }
    }

    public void PlaySFX(string audioName)
    {
        if (sfxSource == null)
            return;

        if (!TryGetClip(audioName, out AudioClip clip))
            return;

        GetClipProfile(audioName, out float volume, out float pitchMin, out float pitchMax);

        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * volume);
        sfxSource.pitch = 1f;
    }

    public void PlayUI(string audioName)
    {
        if (sfxSource == null)
            return;

        if (!TryGetClip(audioName, out AudioClip clip))
            return;

        GetClipProfile(audioName, out float volume, out float pitchMin, out float pitchMax);

        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, masterVolume * uiVolume * volume);
        sfxSource.pitch = 1f;
    }

    public void PlaySFXAt(string audioName, Vector3 position, float volumeScale = 1f)
    {
        if (_spatialPool.Count == 0)
            return;

        if (!TryGetClip(audioName, out AudioClip clip))
            return;

        GetClipProfile(audioName, out float volume, out float pitchMin, out float pitchMax);

        AudioSource spatial = _spatialPool[_spatialIndex];
        _spatialIndex = (_spatialIndex + 1) % _spatialPool.Count;

        spatial.transform.position = position;
        spatial.pitch = Random.Range(pitchMin, pitchMax);
        spatial.PlayOneShot(clip, Mathf.Clamp01(masterVolume * sfxVolume * volume * volumeScale));
        spatial.pitch = 1f;
    }

    public void PlayLand(Vector3 position)
    {
        PlayFallbackAt("land", position, 1f);
    }

    public void PlayDeathFall(Vector3 position)
    {
        PlayFallbackAt("death_fall", position, 1f);
    }

    public void PlayCapture(Vector3 position)
    {
        PlayFallbackAt("capture", position, 1f);
    }

    public void PlayCountdownGo()
    {
        PlayFallback("countdown_go", 1f);
    }

    public void PlayWin()
    {
        if (sfxSource == null)
            return;

        if (!TryGetClip("reward_painter", out AudioClip reward))
            return;

        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(reward, masterVolume * sfxVolume * 1f);

        if (TryGetClip("kill", out AudioClip kill))
        {
            StartCoroutine(PlayDelayedOneShot(kill, masterVolume * sfxVolume * 0.9f, 0.12f));
        }
    }

    private void PlayFallback(string audioName, float volumeScale)
    {
        if (sfxSource == null)
            return;

        if (!GetFallbackClip(audioName, out string fallbackName, out float volume, out float pitchMin, out float pitchMax))
            return;

        if (!TryGetClip(fallbackName, out AudioClip clip))
            return;

        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(masterVolume * sfxVolume * volume * volumeScale));
        sfxSource.pitch = 1f;
    }

    private void PlayFallbackAt(string audioName, Vector3 position, float volumeScale)
    {
        if (_spatialPool.Count == 0)
            return;

        if (!GetFallbackClip(audioName, out string fallbackName, out float volume, out float pitchMin, out float pitchMax))
            return;

        if (!TryGetClip(fallbackName, out AudioClip clip))
            return;

        AudioSource spatial = _spatialPool[_spatialIndex];
        _spatialIndex = (_spatialIndex + 1) % _spatialPool.Count;

        spatial.transform.position = position;
        spatial.pitch = Random.Range(pitchMin, pitchMax);
        spatial.PlayOneShot(clip, Mathf.Clamp01(masterVolume * sfxVolume * volume * volumeScale));
        spatial.pitch = 1f;
    }

    private IEnumerator PlayDelayedOneShot(AudioClip clip, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayMusic(string audioName, bool loop = true, float fadeDuration = 0.75f)
    {
        if (musicSource == null)
            return;

        if (!TryGetClip(audioName, out AudioClip clip))
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        if (_musicFadeRoutine != null)
        {
            StopCoroutine(_musicFadeRoutine);
        }

        _musicFadeRoutine = StartCoroutine(CrossfadeMusic(clip, loop, fadeDuration));
    }

    private IEnumerator CrossfadeMusic(AudioClip clip, bool loop, float fadeDuration)
    {
        float targetVolume = masterVolume * musicVolume;
        float halfFade = Mathf.Max(fadeDuration * 0.5f, 0.01f);

        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;

            for (float t = 0f; t < halfFade; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / halfFade);
                yield return null;
            }
        }

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();

        for (float t = 0f; t < halfFade; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, targetVolume, t / halfFade);
            yield return null;
        }

        musicSource.volume = targetVolume;
        _musicFadeRoutine = null;
    }

    public void StopMusic()
    {
        if (_musicFadeRoutine != null)
        {
            StopCoroutine(_musicFadeRoutine);
            _musicFadeRoutine = null;
        }

        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    public void StopAllSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }

        foreach (AudioSource spatial in _spatialPool)
        {
            if (spatial != null)
            {
                spatial.Stop();
            }
        }
    }

    public void EngineStart()
    {
        if (_engineSource == null)
            return;

        if (!TryGetClip("engine_loop", out AudioClip clip))
            return;

        if (_engineSource.isPlaying && _engineSource.clip == clip)
            return;

        _engineSource.clip = clip;
        _engineSource.loop = true;
        _engineSource.volume = 0f;
        _engineSource.Play();
    }

    public void EngineStop()
    {
        if (_engineSource != null)
        {
            _engineSource.Stop();
            _engineSource.volume = 0f;
        }
    }

    public void SetEngine(float speed01, float throttle01)
    {
        if (_engineSource == null || !_engineSource.isPlaying)
            return;

        speed01 = Mathf.Clamp01(speed01);
        throttle01 = Mathf.Clamp01(throttle01);

        _engineSource.pitch = Mathf.Lerp(0.8f, 1.7f, speed01);
        _engineSource.volume = masterVolume * engineVolume * (0.35f + 0.65f * throttle01);
    }


    private bool TryGetClip(string audioName, out AudioClip clip)
    {
        if (string.IsNullOrWhiteSpace(audioName))
        {
            Debug.LogWarning("AudioManager: Audio name is null or empty.");
            clip = null;
            return false;
        }

        if (!audioDictionary.TryGetValue(audioName, out clip))
        {
            Debug.LogWarning(
                $"AudioManager: No audio clip found with name '{audioName}'."
            );

            return false;
        }

        return true;
    }
}
