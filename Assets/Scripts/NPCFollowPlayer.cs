using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCFollowPlayer : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Assign the Player transform here.")]
    public Transform playerTarget;

    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    [Tooltip("Distance maintained from the player to avoid overlapping.")]
    public float stoppingDistance = 1.5f;
    [Tooltip("Distance at which the NPC will start running to catch up.")]
    public float startFollowDistance = 2.5f;

    private Animator animator;
    private Rigidbody2D rb2D;
    private bool isFollowing = true;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();

        // Ensure Rigidbody settings do not conflict with kinematic cutscene state
        if (rb2D != null)
        {
            rb2D.bodyType = RigidbodyType2D.Kinematic;
            rb2D.linearVelocity = Vector2.zero;
        }

        // Auto-locate player if not assigned via Inspector
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        if (playerTarget == null || !isFollowing)
        {
            ResetAnimatorToIdle();
            return;
        }

        HandleAutonomousMovement();
    }

    private void HandleAutonomousMovement()
    {
        Vector3 targetPosition = playerTarget.position;
        float currentDistance = Vector3.Distance(transform.position, targetPosition);
        Vector2 moveDirection = Vector2.zero;

        // Move only if the player moves beyond the follow threshold
        if (currentDistance > stoppingDistance)
        {
            moveDirection = ((Vector2)(targetPosition - transform.position)).normalized;

            // Translate position smoothly using frame independent delta time
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }

        // Core Animator Parameter Synchronizations
        if (moveDirection != Vector2.zero)
        {
            animator.SetFloat("MoveX", moveDirection.x);
            animator.SetFloat("MoveY", moveDirection.y);
            animator.SetFloat("Speed", 1f); // Triggers walking animation state
        }
        else
        {
            animator.SetFloat("Speed", 0f); // Triggers idle animation state
        }
    }

    private void ResetAnimatorToIdle()
    {
        animator.SetFloat("Speed", 0f);
    }

    public void SetFollowingState(bool state)
    {
        isFollowing = state;
        if (!state)
        {
            ResetAnimatorToIdle();
        }
    }
}