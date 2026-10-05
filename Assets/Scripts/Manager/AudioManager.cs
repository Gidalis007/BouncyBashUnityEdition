using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] audioClips;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private Dictionary<string, AudioClip> audioDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioDictionary();
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

    public void PlaySFX(string audioName)
    {
        if (!TryGetClip(audioName, out AudioClip clip))
            return;

        sfxSource.PlayOneShot(clip);
    }


    public void PlayMusic(string audioName, bool loop = true)
    {
        if (!TryGetClip(audioName, out AudioClip clip))
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
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