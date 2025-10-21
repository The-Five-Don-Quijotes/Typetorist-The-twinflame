using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShockwaveController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The tag of the GameObject that this shockwave should detect as a 'player'.")]
    public string playerTag = "Player";

    [Tooltip("Should the shockwave only hit the player once?")]
    public bool hitOnce = true;

    private bool hasHitPlayer = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure the collider is set to Trigger for non-physical collisions
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Ensure there's a Rigidbody2D for collision detection, if not already present.
        // Kinematic is usually best for effects that don't need physics.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if this shockwave has already hit the player and if it's set to hit only once.
        if (hitOnce && hasHitPlayer)
        {
            return; // Already hit, so do nothing.
        }

        // Check if the other GameObject has the specified playerTag.
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"Shockwave hit the player: {other.gameObject.name}!", other.gameObject);

            // If we only want to hit once, set the flag.
            if (hitOnce)
            {
                hasHitPlayer = true;
            }

            // --- In a full game, you would add actual damage logic here: ---
            // PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(damageAmount);
            // }
        }
    }

    // --- Optional: Uncomment for continuous damage while overlapping ---
    // private void OnTriggerStay2D(Collider2D other)
    // {
    //     if (other.CompareTag(playerTag))
    //     {
    //         Debug.Log("Player is still in shockwave area!");
    //     }
    // }

    public void destroyShockWave()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
