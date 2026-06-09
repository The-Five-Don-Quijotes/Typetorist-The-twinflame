using System.Collections;
using UnityEngine;

public class ZhavokMovement : MonoBehaviour
{
    private Transform player;
    private Animator animator;

    [Header("Movement Settings")]
    public float teleportDistanceThreshold = 10f; // Distance at which boss teleports
    public float closeRangeDistance = 3f; // Distance where boss picks a random position
    public float moveSpeed = 5f;
    public float teleportCooldown = 5f;
    public float movementPauseDuration = 1.5f; // Pause between moves
    public float stoppingDistance = 0.5f; // How close is "close enough" when moving

    [Header("Teleport Safety Zone")]
    [Tooltip("Minimum distance from the player the boss will appear after teleporting.")]
    public float minTeleportRadius = 3f;
    [Tooltip("Maximum distance from the player the boss will appear after teleporting.")]
    public float maxTeleportRadius = 5f;

    [Header("Audio Settings")]
    public AudioClip teleportSound;

    private Vector2 currentTarget;
    private bool isMoving = false;
    private bool canTeleport = true;
    private bool isCombatActive = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;
        currentTarget = transform.position; // Start by standing still
    }

    void Update()
    {
        if (player == null || !isCombatActive) return;
        LookAtPlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > teleportDistanceThreshold && canTeleport)
        {
            StartCoroutine(TeleportToPlayer());
        }
        else if (distanceToPlayer > closeRangeDistance && !isMoving)
        {
            StartCoroutine(MoveToRandomPositionNearPlayer());
        }

        MoveTowardsTarget();
    }

    public void BeginMovementPhase()
    {
        isCombatActive = true;
    }

    public void StopMovementPhase()
    {
        isCombatActive = false;
        StopAllCoroutines();
    }

    IEnumerator TeleportToPlayer()
    {
        canTeleport = false;

        // Play the telegraph sound immediately to warn the player
        if (AudioManager.instance != null && teleportSound != null)
        {
            AudioManager.instance.PlaySFX(teleportSound);
        }

        // Trigger the animation. The actual position change should be handled by an Animation Event calling Teleport()
        animator.SetTrigger("isUsingSkill");

        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }

    public void Teleport()
    {
        if (player == null) return;

        // Calculate a safe distance using normalized direction multiplied by a clamped random range
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minTeleportRadius, maxTeleportRadius);

        transform.position = (Vector2)player.position + (randomDirection * randomDistance);

        Debug.Log($"Boss teleported near player at distance: {randomDistance}");
    }

    IEnumerator MoveToRandomPositionNearPlayer()
    {
        isMoving = true;

        Vector2 randomOffset = Random.insideUnitCircle * closeRangeDistance;
        currentTarget = (Vector2)player.position + randomOffset;

        yield return new WaitForSeconds(movementPauseDuration); // Pause between moves
        isMoving = false;
    }

    private void MoveTowardsTarget()
    {
        if (Vector2.Distance(transform.position, currentTarget) > stoppingDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        }
    }

    private void LookAtPlayer()
    {
        if (player.position.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
}