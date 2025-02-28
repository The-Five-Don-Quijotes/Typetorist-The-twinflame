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
        audioSource.PlayOneShot(selectedSound);
    }
}
