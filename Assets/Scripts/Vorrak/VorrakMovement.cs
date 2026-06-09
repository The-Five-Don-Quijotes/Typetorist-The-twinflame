using System.Collections;
using UnityEngine;

public class VorrakMovement : MonoBehaviour
{
    public Transform player;
    private Animator animator;

    public GameObject melee;

    [Header("Attack Settings")]
    public float hitboxDuration = 0.2f;

    [Header("Teleport Positions")]
    private Vector2[] teleportPositions = new Vector2[]
    {
        // Left boundary positions (X = -16)
        new Vector2(-16, 13), new Vector2(-16, 10), new Vector2(-16, 7), new Vector2(-16, 4),
        new Vector2(-16, 1), new Vector2(-16, -2), new Vector2(-16, -5), new Vector2(-16, -8),
        new Vector2(-16, -11), new Vector2(-16, -13),

        // Right boundary positions (X = 16)
        new Vector2(16, 13), new Vector2(16, 10), new Vector2(16, 7), new Vector2(16, 4),
        new Vector2(16, 1), new Vector2(16, -2), new Vector2(16, -5), new Vector2(16, -8),
        new Vector2(16, -11), new Vector2(16, -13)
    };

    private bool isMoving = false;
    public float moveSpeed = 3f;

    [Header("Audio Settings")]
    public AudioClip teleSound;

    private bool isCombatActive = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null || !isCombatActive) return;
        LookAtPlayer();
    }

    public void BeginMovementPhase()
    {
        isCombatActive = true;
    }

    public void StopMovementPhase()
    {
        isCombatActive = false;
        StopAllCoroutines();
        isMoving = false;
    }

    public void ActivateMeleeHitbox()
    {
        StartCoroutine(EnableHitbox(melee));
    }

    private IEnumerator EnableHitbox(GameObject hitbox)
    {
        hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxDuration);
        hitbox.SetActive(false);
    }

    private void LookAtPlayer()
    {
        if (player == null) return;
        if (player.position.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    public void TeleportToRandomPosition()
    {
        if (isMoving || !isCombatActive) return;

        if (AudioManager.instance != null && teleSound != null)
        {
            AudioManager.instance.PlaySFX(teleSound);
        }
        int randomIndex = Random.Range(0, teleportPositions.Length);
        transform.position = teleportPositions[randomIndex];
        LookAtPlayer();
    }

    // Teleports the boss to the optimal corner to ensure a full-screen laser sweep
    public void TeleportForLaserSweep()
    {
        if (player == null || isMoving || !isCombatActive) return;

        if (AudioManager.instance != null && teleSound != null)
        {
            AudioManager.instance.PlaySFX(teleSound);
        }

        // Opposite X side
        float targetX = (player.position.x > 0) ? -16f : 16f;

        // Opposite Y half: If player is at the bottom, spawn at the top (and vice versa)
        float targetY = (player.position.y > 0) ? -11f : 7f;

        transform.position = new Vector2(targetX, targetY);
        LookAtPlayer();
    }

    public void MoveToFurthestY()
    {
        if (!isCombatActive) return;
        Vector2 targetPosition = GetFurthestYPosition();
        StartCoroutine(MoveTowardsTarget(targetPosition));
    }

    // Accurately calculates the furthest Y point along the current X axis lane
    private Vector2 GetFurthestYPosition()
    {
        float currentX = transform.position.x;
        float currentY = transform.position.y;
        float furthestY = currentY;
        float maxDistance = 0f;

        foreach (Vector2 pos in teleportPositions)
        {
            if (Mathf.Approximately(pos.x, currentX))
            {
                float distance = Mathf.Abs(pos.y - currentY);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthestY = pos.y;
                }
            }
        }
        return new Vector2(currentX, furthestY);
    }

    private IEnumerator MoveTowardsTarget(Vector2 targetPosition)
    {
        isMoving = true;
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    public IEnumerator ChasePlayerForMelee(float maxDuration, float meleeRange, System.Action onReachTarget)
    {
        if (player == null || isMoving || !isCombatActive) yield break;

        isMoving = true;
        float elapsedTime = 0f;

        while (elapsedTime < maxDuration)
        {
            if (player == null || !isCombatActive) break;

            if (Vector2.Distance(transform.position, player.position) <= meleeRange)
            {
                break;
            }

            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        isMoving = false;

        if (isCombatActive)
        {
            onReachTarget?.Invoke();
        }
    }

    public void TeleportToPlayerYLane()
    {
        if (player == null || isMoving || !isCombatActive) return;

        if (AudioManager.instance != null && teleSound != null)
        {
            AudioManager.instance.PlaySFX(teleSound);
        }

        float targetX = (player.position.x > 0) ? -16f : 16f;
        transform.position = new Vector2(targetX, player.position.y);

        LookAtPlayer();
    }
}