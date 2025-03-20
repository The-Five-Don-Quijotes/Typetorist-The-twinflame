using System.Collections;
using UnityEngine;

public class VorrakMovement : MonoBehaviour
{
    private Transform player;
    private Animator animator;

    public GameObject melee;

    [Header("Attack Settings")]
    public float cooldown = 0.1f;
    public float hitboxDuration = 0.2f;

    [Header("Teleport Positions")]
    private Vector2[] teleportPositions = new Vector2[]
    {
        new Vector2(-16, 7), new Vector2(-16, 4), new Vector2(-16, 1), new Vector2(-16, -2),
        new Vector2(-16, -5), new Vector2(-16, -8), new Vector2(-16, -11),
        new Vector2(16, 7), new Vector2(16, 4), new Vector2(16, 1), new Vector2(16, -2),
        new Vector2(16, -5), new Vector2(16, -8), new Vector2(16, -11)
    };

    private bool isMoving = false;
    public float moveSpeed = 3f; // Speed of movement when moving down

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;
        LookAtPlayer();
    }

    public void ActivateMeleeHitbox()
    {
        StartCoroutine(EnableHitbox(melee));
    }

    IEnumerator EnableHitbox(GameObject hitbox)
    {
        hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxDuration);
        hitbox.SetActive(false);
    }

    private void LookAtPlayer()
    {
        if(player == null) return;
        if (player.position.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    // Teleport to a random position
    public void TeleportToRandomPosition()
    {
        if (isMoving) return;

        int randomIndex = Random.Range(0, teleportPositions.Length);
        transform.position = teleportPositions[randomIndex];

        // Ensure the boss faces the player correctly after teleporting
        LookAtPlayer();
    }

    // Move towards the furthest Y position while keeping the same X
    public void MoveToFurthestY()
    {
        Vector2 targetPosition = GetFurthestYPosition();
        StartCoroutine(MoveTowardsTarget(targetPosition));
    }

    private Vector2 GetFurthestYPosition()
    {
        float currentX = transform.position.x;
        float minY = float.MaxValue;

        // Find the lowest Y position for the same X coordinate
        foreach (Vector2 pos in teleportPositions)
        {
            if (Mathf.Approximately(pos.x, currentX) && pos.y < minY)
            {
                minY = pos.y;
            }
        }

        return new Vector2(currentX, minY);
    }

    IEnumerator MoveTowardsTarget(Vector2 targetPosition)
    {
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            // Move the boss
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            isMoving = true;

            yield return null; // Wait until next frame
        }

        // Ensure final position is exact
        transform.position = targetPosition;
        isMoving = false;
    }

    public void MoveNearPlayerWithDuration(float duration)
    {
        if (player == null || isMoving) return;

        isMoving = true; // Prevent overlapping movement

        // Get a random direction within a 0.3 unit radius
        Vector2 randomDirection = Random.insideUnitCircle.normalized * 0.3f;
        Vector2 targetPosition = (Vector2)player.position + randomDirection;

        StartCoroutine(MoveAndStop(targetPosition, duration));
    }

    IEnumerator MoveAndStop(Vector2 targetPosition, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isMoving = false; // Allow movement again
    }
}
