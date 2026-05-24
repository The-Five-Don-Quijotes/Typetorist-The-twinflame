using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("=========== Audio Source ============")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("=========== Global Audio Clips ============")]
    public AudioClip dieClip;
    public AudioClip damagedClip;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    // Dictionary to store the last time a specific AudioClip was played
    private Dictionary<int, float> sfxCooldowns = new Dictionary<int, float>();

    // Minimum time (in seconds) required before playing the exact same clip again
    private const float MIN_SFX_INTERVAL = 0.05f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadVolume();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        // Prevent restarting the track if it is already playing
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        int clipId = clip.GetInstanceID();

        if (sfxCooldowns.TryGetValue(clipId, out float lastPlayedTime))
        {
            if (Time.time - lastPlayedTime < MIN_SFX_INTERVAL)
            {
                return;
            }
        }

        sfxCooldowns[clipId] = Time.time;
        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(clip);
    }

    // Wrappers for global SFX
    public void PlayDamagedSFX() => PlaySFX(damagedClip);
    public void PlayDeathSFX() => PlaySFX(dieClip);

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    private void LoadVolume()
    {
        musicSource.volume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        sfxSource.volume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f);
    }

    public float GetMusicVolume()
    {
        if (musicSource != null)
        {
            return musicSource.volume;
        }
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
    }

    public float GetSFXVolume()
    {
        if (sfxSource != null)
        {
            return sfxSource.volume;
        }
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f);
    }
}