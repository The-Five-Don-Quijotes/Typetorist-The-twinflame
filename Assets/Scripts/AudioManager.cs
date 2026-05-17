using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("=========== Audio Source ============")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("=========== Audio Clip ============")]
    public AudioClip backgroundmusic;
    public AudioClip audioClip2;
    public AudioClip audioClip3;
    public AudioClip audioClip4;
    public AudioClip die;
    public AudioClip damaged;
    public AudioClip fireballSound1;
    public AudioClip fireballSound2;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

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

        musicSource.clip = backgroundmusic;
        musicSource.Play();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1.0f); 
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        // Save the setting so it persists after closing the game.
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        SFXSource.volume = volume;
        // Save the setting.
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    private void LoadVolume()
    {
        // GetFloat will return the default value (1.0f) if the key doesn't exist yet.
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f);

        musicSource.volume = musicVolume;
        SFXSource.volume = sfxVolume;
    }

    public float GetMusicVolume()
    {
        return musicSource.volume;
    }

    public float GetSFXVolume()
    {
        return SFXSource.volume;
    }
}
