using UnityEngine;

public class Fireball : MonoBehaviour
{
    public AudioClip fireballSound1;
    public AudioClip fireballSound2;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Pick a random sound and play it
        AudioClip selectedSound = Random.value < 0.5f ? fireballSound1 : fireballSound2;
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(selectedSound);
        }
        else
        {
            Debug.LogWarning("AudioManager is missing! Fireball sound could not play.");
        }
    }
}
