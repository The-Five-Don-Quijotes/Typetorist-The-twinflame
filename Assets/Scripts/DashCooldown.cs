using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// Manages the visual cooldown effect for a UI ability icon.
[RequireComponent(typeof(Image))]
public class DashCooldown : MonoBehaviour
{
    private Image abilityImage;
    private Coroutine activeCooldownCoroutine;

    void Awake()
    {
        // Get the Image component attached to this GameObject.
        abilityImage = GetComponent<Image>();

        // Ensure the ability is ready when the game starts.
        abilityImage.fillAmount = 1f;
    }

    /// Starts the visual cooldown effect for a specified duration.
    /// If a cooldown is already in progress, it will be reset and started again.
    public void StartCooldown(float cooldownDuration)
    {
        // If there's an existing cooldown running, stop it first.
        if (activeCooldownCoroutine != null)
        {
            StopCoroutine(activeCooldownCoroutine);
        }

        // Start the new cooldown coroutine.
        activeCooldownCoroutine = StartCoroutine(CooldownRoutine(cooldownDuration));
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        // Prevent division by zero if duration is 0 or less.
        if (duration <= 0f)
        {
            abilityImage.fillAmount = 1f;
            yield break; // Exit the coroutine.
        }

        float elapsedTime = 0f;
        abilityImage.fillAmount = 0f; // Start with the icon empty.

        // Loop until the elapsed time reaches the cooldown duration.
        while (elapsedTime < duration)
        {
            // Calculate the fill amount (from 0 up to 1).
            abilityImage.fillAmount = elapsedTime / duration;

            // Increment the elapsed time by the time since the last frame.
            elapsedTime += Time.deltaTime;

            // Wait until the next frame before continuing the loop.
            yield return null;
        }

        // Ensure the fill amount is exactly 1 when the cooldown finishes.
        abilityImage.fillAmount = 1f;
        activeCooldownCoroutine = null; // Clear the coroutine reference.
    }
}