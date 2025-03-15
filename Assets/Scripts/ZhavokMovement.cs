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
    public float movementPauseDuration = 1.5f; // New: Pause between moves
    public float stoppingDistance = 0.5f; // New: How close is "close enough" when moving

    private Vector2 currentTarget;
    private bool isMoving = false;
    private bool canTeleport = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;
        currentTarget = transform.position; // Start by standing still
    }

    void Update()
    {
        if (player == null) return;
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

    IEnumerator TeleportToPlayer()
    {
        canTeleport = false;
        animator.SetTrigger("isUsingSkill");

        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }

    public void Teleport()
    {
        transform.position = player.position + (Vector3)Random.insideUnitCircle * 2f; // Slight offset
        Debug.Log("Boss teleported to player!");
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
