using UnityEngine;

public class FinalSceneMusicChanger : MonoBehaviour
{
    [Header("Audio Configuration")]
    [Tooltip("The specific background music for this final cinematic scene.")]
    public AudioClip finalSceneMusic;

    private void Start()
    {
        StopAllStrayAudio();
        PlayFinalMusic();
    }

    private void StopAllStrayAudio()
    {
        // Find every AudioSource currently loaded in the scene/memory
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (AudioSource source in allAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    private void PlayFinalMusic()
    {
        // Utilize the existing AudioManager singleton to handle the playback and PlayerPrefs volume
        if (AudioManager.instance != null)
        {
            if (finalSceneMusic != null)
            {
                AudioManager.instance.PlayMusic(finalSceneMusic);
            }
            else
            {
                Debug.LogWarning("FinalSceneMusicChanger: No finalSceneMusic assigned in the Inspector.");
            }
        }
        else
        {
            Debug.LogError("FinalSceneMusicChanger: AudioManager.instance is null. Ensure AudioManager exists in the scene hierarchy or is carried over.");
        }
    }
}