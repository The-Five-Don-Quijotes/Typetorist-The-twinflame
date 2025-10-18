using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the movement of a 2D boss character based on selectable patterns.
/// Attach this script to your Boss GameObject.
/// </summary>
public class UyiaMovement : MonoBehaviour
{
    // Enum to define the different movement patterns you drew.
    // You can select one of these in the Unity Inspector.
    public enum MovementPattern { Cross, Diamond, LungeTowardsPlayer }

    [Header("Movement Settings")]
    [Tooltip("Choose the movement pattern for the boss.")]
    public MovementPattern pattern = MovementPattern.Cross;

    [Tooltip("The speed at which the boss moves.")]
    public float speed = 15f;

    [Tooltip("The player's transform. Only needed for the 'LungeTowardsPlayer' pattern.")]
    public Transform playerTransform;

    [Tooltip("The distance from the center the boss will move for Cross and Diamond patterns.")]
    public float patrolDistance = 7f;

    [Header("Timing Settings")]
    [Tooltip("The short pause before the boss executes a move. Use this time to trigger a warning animation or effect.")]
    public float telegraphTime = 0.5f;

    [Tooltip("The pause duration when the boss reaches a destination point.")]
    public float pauseAtDestinationTime = 1f;

    [Header("Component Settings")]
    [Tooltip("Disable Animator root motion to allow this script to control movement.")]
    public bool disableRootMotion = true;

    // Internal variables
    private Vector3 startPosition;
    private Vector3[] patrolPoints;
    private int currentPatrolIndex = 0;
    private Coroutine movementCoroutine;
    private Rigidbody2D rb2D;
    private Animator animator;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    void Start()
    {
        // Store the initial position of the boss.
        startPosition = transform.position;

        // --- Get component references ---
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // --- Rigidbody Setup ---
        if (rb2D == null)
        {
            Debug.LogWarning("Rigidbody2D component not found. Adding a kinematic Rigidbody2D.", this);
            rb2D = gameObject.AddComponent<Rigidbody2D>();
        }
        rb2D.bodyType = RigidbodyType2D.Kinematic;

        // --- FIX: Disable Animator Root Motion ---
        // This is the key change. If an Animator exists, this tells it to stop
        // controlling the GameObject's position, allowing this script to take over.
        if (animator != null && disableRootMotion)
        {
            animator.applyRootMotion = false;
            Debug.Log("Animator root motion has been disabled to allow script-based movement.");
        }

        // Set up the specific points for the selected pattern.
        SetupPatrolPoints();

        // Start the main movement logic.
        movementCoroutine = StartCoroutine(MovementLoop());
    }

    /// <summary>
    /// Sets up the patrol points based on the selected pattern.
    /// </summary>
    void SetupPatrolPoints()
    {
        // Using Vector2 for 2D direction calculations
        Vector2 startPos2D = startPosition;

        switch (pattern)
        {
            case MovementPattern.Cross:
                // Defines 4 points in a '+' shape around the start position.
                patrolPoints = new Vector3[]
                {
                    startPos2D + Vector2.up * patrolDistance,
                    startPos2D + Vector2.right * patrolDistance,
                    startPos2D + Vector2.down * patrolDistance,
                    startPos2D + Vector2.left * patrolDistance
                };
                break;

            case MovementPattern.Diamond:
                // Also defines 4 points, but in a diamond shape.
                patrolPoints = new Vector3[]
                {
                    startPos2D + new Vector2(0, patrolDistance),       // Top
                    startPos2D + new Vector2(patrolDistance, 0),       // Right
                    startPos2D + new Vector2(0, -patrolDistance),      // Bottom
                    startPos2D + new Vector2(-patrolDistance, 0)       // Left
                };
                break;

            case MovementPattern.LungeTowardsPlayer:
                // The lunge pattern doesn't use predefined points.
                if (playerTransform == null)
                {
                    Debug.LogError("Player Transform is not assigned! The 'LungeTowardsPlayer' pattern requires it.", this);
                }
                break;
        }
    }

    /// <summary>
    /// The main coroutine that loops through the movement logic.
    /// </summary>
    private IEnumerator MovementLoop()
    {
        // The loop runs forever, making the boss move continuously.
        while (true)
        {
            // --- TELEGRAPH PHASE ---
            yield return new WaitForSeconds(telegraphTime);

            // --- MOVEMENT PHASE ---
            Vector3 targetPosition = GetNextTargetPosition();

            // This loop moves the boss towards the target position over several frames.
            while (Vector2.Distance(rb2D.position, targetPosition) > 0.01f)
            {
                // --- IMPROVEMENT: Use rb2D.MovePosition for smoother, physics-based movement ---
                Vector2 newPosition = Vector2.MoveTowards(rb2D.position, targetPosition, speed * Time.fixedDeltaTime);
                rb2D.MovePosition(newPosition);
                // Wait for the next physics update to ensure smooth movement.
                yield return new WaitForFixedUpdate();
            }
            // Ensure the boss is exactly at the target position.
            rb2D.MovePosition(targetPosition);


            // --- PAUSE PHASE ---
            yield return new WaitForSeconds(pauseAtDestinationTime);

            // For the Lunge pattern, we need a return trip.
            if (pattern == MovementPattern.LungeTowardsPlayer)
            {
                yield return new WaitForSeconds(telegraphTime);

                while (Vector2.Distance(rb2D.position, startPosition) > 0.01f)
                {
                    Vector2 newPosition = Vector2.MoveTowards(rb2D.position, startPosition, speed * Time.fixedDeltaTime);
                    rb2D.MovePosition(newPosition);
                    yield return new WaitForFixedUpdate();
                }
                rb2D.MovePosition(startPosition);

                yield return new WaitForSeconds(pauseAtDestinationTime);
            }
        }
    }

    /// <summary>
    /// Determines the next target position based on the current movement pattern.
    /// </summary>
    /// <returns>The Vector3 position of the next target.</returns>
    private Vector3 GetNextTargetPosition()
    {
        Vector3 target;
        if (pattern == MovementPattern.LungeTowardsPlayer)
        {
            if (playerTransform == null) return startPosition;
            target = playerTransform.position;
        }
        else
        {
            target = patrolPoints[currentPatrolIndex];
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        return target;
    }
}

