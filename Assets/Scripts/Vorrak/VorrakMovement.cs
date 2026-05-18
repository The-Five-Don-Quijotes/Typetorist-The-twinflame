using System.Collections;
using UnityEngine;

public class VorrakMovement : MonoBehaviour
{
    public Transform player;
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
    public float moveSpeed = 3f;

    // --- NEW: Combat state ---
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

    // --- NEW: Execution Control ---
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

        int randomIndex = Random.Range(0, teleportPositions.Length);
        transform.position = teleportPositions[randomIndex];
        LookAtPlayer();
    }

    public void MoveToFurthestY()
    {
        if (!isCombatActive) return;
        Vector2 targetPosition = GetFurthestYPosition();
        StartCoroutine(MoveTowardsTarget(targetPosition));
    }

    private Vector2 GetFurthestYPosition()
    {
        float currentX = transform.position.x;
        float minY = float.MaxValue;

        foreach (Vector2 pos in teleportPositions)
        {
            if (Mathf.Approximately(pos.x, currentX) && pos.y < minY)
            {
                minY = pos.y;
            }
        }
        return new Vector2(currentX, minY);
    }

    private IEnumerator MoveTowardsTarget(Vector2 targetPosition)
    {
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            isMoving = true;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    public void MoveNearPlayerWithDuration(float duration)
    {
        if (player == null || isMoving || !isCombatActive) return;

        isMoving = true;
        Vector2 randomDirection = Random.insideUnitCircle.normalized * 0.3f;
        Vector2 targetPosition = (Vector2)player.position + randomDirection;

        StartCoroutine(MoveAndStop(targetPosition, duration));
    }

    private IEnumerator MoveAndStop(Vector2 targetPosition, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isMoving = false;
    }
}