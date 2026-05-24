using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShockwaveController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The tag of the GameObject that this shockwave should detect as a 'player'.")]
    public string playerTag = "Player";

    [Tooltip("Should the shockwave only hit the player once?")]
    public bool hitOnce = true;

    [Tooltip("The amount of damage the shockwave deals to the player.")]
    public float damageAmount = 10f;

    private bool hasHitPlayer = false;
    private PlayerStats playerStats;

    void Start()
    {
        // Ensure the collider is set to Trigger for non-physical collisions
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Ensure there's a Rigidbody2D for collision detection
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Cache the PlayerStats reference from the GameManager
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
        {
            playerStats = gameManager.GetComponent<PlayerStats>();
        }

        // Fallback: Find by type if the GameObject is named differently
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats script could not be found on the GameManager.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if this shockwave has already hit the player and if it's set to hit only once.
        if (hitOnce && hasHitPlayer)
        {
            return;
        }

        // Check if the collided GameObject has the specified playerTag.
        if (other.CompareTag(playerTag))
        {
            if (playerStats != null)
            {
                // Apply damage via the GameManager's PlayerStats component
                playerStats.DealDamage(1);
                Debug.Log($"Shockwave dealt {damageAmount} damage to the player via GameManager.", gameObject);

                if (hitOnce)
                {
                    hasHitPlayer = true;
                }
            }
            else
            {
                Debug.LogWarning("Player was hit, but the PlayerStats reference is missing.");
            }
        }
    }

    public void destroyShockWave()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}