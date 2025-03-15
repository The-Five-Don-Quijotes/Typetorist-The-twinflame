using System.Collections;
using UnityEngine;

public class DotController : MonoBehaviour
{
    private Transform player;
    public float followDuration = 1f; // How long the dot follows the player
    public float speed = 8f; // Movement speed
    public float spawnRadius = 5f; // How far the dots spawn from the player

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player != null)
        {
            // Spawn at a random position around the player
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            transform.position = player.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            StartCoroutine(FollowPlayer());
        }
    }

    private IEnumerator FollowPlayer()
    {
        float elapsedTime = 0f;

        while (elapsedTime < followDuration)
        {
            if (player == null) yield break; // Stop if player is missing

            // Move directly toward the player's current position
            transform.position = Vector3.Lerp(transform.position, player.position, speed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(FadeOutAndDestroy()); // After following, fade out and disappear
    }

    private IEnumerator FadeOutAndDestroy()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        float fadeTime = 0.01f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // Destroy after fading out
    }
}
