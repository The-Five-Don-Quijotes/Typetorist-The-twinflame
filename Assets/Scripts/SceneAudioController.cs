using UnityEngine;

public class SceneAudioController : MonoBehaviour
{
    [Header("=========== Scene Music ============")]
    [Tooltip("Background music to play when this scene loads.")]
    [SerializeField] private AudioClip sceneBackgroundMusic;

    [Header("=========== Scene Specific SFX ============")]
    [Tooltip("Specific sound effects used only in this scene.")]
    public AudioClip fireballSound1;
    public AudioClip fireballSound2;

    private void Start()
    {
        if (AudioManager.instance != null)
        {
            if (sceneBackgroundMusic != null)
            {
                AudioManager.instance.PlayMusic(sceneBackgroundMusic);
            }
        }
        else
        {
            Debug.LogWarning("SceneAudioController: AudioManager instance not found in scene.");
        }
    }

    // Call this method from boss/player scripts in this specific scene
    public void PlayFireball1()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(fireballSound1);
        }
    }

    public void PlayFireball2()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(fireballSound2);
        }
    }
}