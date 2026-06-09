using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class GhostHand : MonoBehaviour
{
    [Header("Hand Settings")]
    public float flySpeed = 8f;
    public float pullSpeed = 12f;

    [Tooltip("The minimum distance from the boss where the player will be released.")]
    public float dropOffDistance = 2.5f;

    [Tooltip("How long the player remains invincible after being dropped off.")]
    public float postGrabInvincibilityTime = 1.5f;

    private Transform targetPlayer;
    private Transform bossTransform;
    private PlayerMovement playerMovement;
    private bool isPulling = false;

    public void Initialize(Transform player, Transform boss)
    {
        targetPlayer = player;
        bossTransform = boss;

        // Calculate initial rotation towards the player
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        if (targetPlayer == null || bossTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        if (!isPulling)
        {
            // Phase 1: Fly towards the player
            transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, flySpeed * Time.deltaTime);
        }
        else
        {
            // Phase 2: Calculate a drop-off point that is NOT exactly inside the boss
            Vector2 directionToBoss = (bossTransform.position - transform.position).normalized;
            Vector2 pullDestination = (Vector2)bossTransform.position - (directionToBoss * dropOffDistance);

            // Pull the player
            transform.position = Vector2.MoveTowards(transform.position, pullDestination, pullSpeed * Time.deltaTime);
            targetPlayer.position = transform.position; // Force player to follow the hand

            // Release the player when reaching the drop-off point
            if (Vector2.Distance(transform.position, pullDestination) <= 0.1f)
            {
                ReleasePlayer();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPulling && collision.CompareTag("Player"))
        {
            GrabPlayer(collision.gameObject);
        }
    }

    private void GrabPlayer(GameObject playerObj)
    {
        isPulling = true;

        // Disable movement and grant temporary invincibility during the forced pull
        playerMovement = playerObj.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            playerMovement.isInvincible = true;
        }
    }

    private void ReleasePlayer()
    {
        // Restore player control and trigger the internal invincibility timer
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            playerMovement.SetTemporaryInvincibility(postGrabInvincibilityTime);
        }

        // Trigger the visual blinking effect
        if (targetPlayer != null)
        {
            PlayerStats stats = targetPlayer.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TriggerExternalInvincibility(postGrabInvincibilityTime);
            }
        }

        Destroy(gameObject);
    }
}