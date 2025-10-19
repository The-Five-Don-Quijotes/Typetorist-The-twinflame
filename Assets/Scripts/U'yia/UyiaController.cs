using System.Collections;
using System.Collections.Generic; // Required for List
using UnityEngine;

/// <summary>
/// A unified, advanced controller for the "Uyia" boss.
/// It handles multiple movement patterns and can sequence them dynamically.
/// This should be the ONLY movement script on the boss GameObject.
/// </summary>
public class UyiaBossController : MonoBehaviour
{
    // Enum to define the boss's overall behavior strategy.
    public enum MovementBehavior
    {
        FixedPattern,      // The boss repeats a single pattern forever.
        DynamicSequence    // The boss cycles through a sequence of patterns.
    }

    // Enum to define all available movement patterns in the Inspector.
    public enum MovementPattern
    {
        Cross,             // Moves in a '+' shape automatically.
        Diamond,           // Moves in a diamond shape automatically.
        CustomPatrol,      // Moves between points you set manually in the scene.
        ArcJumps,          // Jumps in arcs over a central point.
        LungeTowardsPlayer, // A direct attack towards the player's position.
        MoonLeap           // Leaps high up, pauses, crashes down, and creates a shockwave.
    }

    [Header("AI Behavior")]
    [Tooltip("Choose if the boss uses one fixed pattern or cycles through a sequence.")]
    public MovementBehavior behavior = MovementBehavior.DynamicSequence;

    [Tooltip("The single pattern to use if behavior is 'FixedPattern'.")]
    public MovementPattern fixedPattern = MovementPattern.Cross;

    [Tooltip("The sequence of patterns to follow in 'DynamicSequence' mode.")]
    public List<MovementPattern> dynamicSequence = new List<MovementPattern>();

    [Tooltip("How many moves to perform in one pattern before switching to the next.")]
    public int movesPerPattern = 4;


    [Header("General Settings")]
    [Tooltip("The maximum speed at which the boss moves.")]
    public float moveSpeed = 15f;

    [Tooltip("How quickly the boss accelerates and decelerates. Lower values are faster.")]
    public float smoothTime = 0.25f;

    [Tooltip("The time the boss waits before starting a move (good for telegraphing attacks).")]
    public float telegraphTime = 0.5f;

    [Tooltip("The time the boss pauses after completing a move.")]
    public float waitTime = 1.0f;


    [Header("Patrol Settings")]
    [Tooltip("The distance from the center the boss will move for Cross and Diamond patterns.")]
    public float patrolDistance = 7f;

    [Tooltip("An array of points for the boss to move between (only for 'CustomPatrol' pattern).")]
    public Transform[] patrolPoints;


    [Header("Arc Jump Settings")]
    [Tooltip("The center of the area for arc jumps.")]
    public Transform arenaCenter;
    [Tooltip("How far from the center the boss will jump.")]
    public float arcRadius = 10f;
    [Tooltip("The maximum height of the arc jump.")]
    public float arcHeight = 5f;
    [Tooltip("How long it takes to complete one arc jump.")]
    public float arcDuration = 1.5f;


    [Header("Lunge Settings")]
    [Tooltip("A reference to the player's transform (required for 'LungeTowardsPlayer').")]
    public Transform playerTransform;


    [Header("Moon Leap Settings")]
    [Tooltip("The max height the boss will reach during the Moon Leap.")]
    public float moonLeapHeight = 20f;
    [Tooltip("How long it takes to reach the 'moon'.")]
    public float leapUpDuration = 1.0f;
    [Tooltip("How long the boss stays at the 'moon' apex.")]
    public float moonPauseDuration = 1.0f;
    [Tooltip("How long it takes to crash back down.")]
    public float crashDownDuration = 0.75f;
    [Tooltip("The Prefab for the shockwave effect to spawn on impact.")]
    public GameObject shockwavePrefab;
    [Tooltip("The point relative to the boss where the shockwave should spawn (e.g., at its base).")]
    public Vector3 shockwaveOffset = new Vector3(0, -1f, 0); // Adjust this if your boss's pivot isn't at the bottom

    // Internal state variables
    private Rigidbody2D rb2D;
    private Vector3 startPosition;
    private Vector3[] generatedPatrolPoints;
    private int currentPatrolIndex = 0;
    private MovementPattern currentPattern;
    private int dynamicSequenceIndex = 0;
    private int movesCompleted = 0;
    private Vector2 smoothDampVelocity; // Used for LinearMovement
    private Vector3 currentLeapVelocity = Vector3.zero; // Used for MoonLeap's SmoothDamp

    void Start()
    {
        startPosition = transform.position;

        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            rb2D = gameObject.AddComponent<Rigidbody2D>();
        }
        rb2D.bodyType = RigidbodyType2D.Kinematic; // Kinematic for precise control

        // Start the main behavior coroutine.
        StartCoroutine(BehaviorManager());
    }

    /// <summary>
    /// Manages the boss's overall behavior, switching patterns as needed.
    /// </summary>
    private IEnumerator BehaviorManager()
    {
        UpdateCurrentPattern();

        while (true)
        {
            switch (currentPattern)
            {
                case MovementPattern.ArcJumps:
                    yield return StartCoroutine(ArcJumpRoutine());
                    break;
                case MovementPattern.MoonLeap:
                    yield return StartCoroutine(MoonLeapRoutine());
                    break;
                default: // Handles Cross, Diamond, CustomPatrol, LungeTowardsPlayer
                    yield return StartCoroutine(LinearMovementRoutine());
                    break;
            }
            movesCompleted++;

            // After a move is done, check if we need to switch patterns.
            if (behavior == MovementBehavior.DynamicSequence && movesCompleted >= movesPerPattern)
            {
                movesCompleted = 0;
                dynamicSequenceIndex = (dynamicSequenceIndex + 1) % dynamicSequence.Count;
                UpdateCurrentPattern();
            }
        }
    }

    /// <summary>
    /// Sets the currentPattern variable and generates any necessary points.
    /// </summary>
    private void UpdateCurrentPattern()
    {
        if (behavior == MovementBehavior.FixedPattern)
        {
            currentPattern = fixedPattern;
        }
        else
        {
            if (dynamicSequence.Count > 0)
            {
                currentPattern = dynamicSequence[dynamicSequenceIndex];
            }
            else
            {
                Debug.LogError("DynamicSequence is selected but the sequence is empty! Defaulting to Cross.", this);
                currentPattern = MovementPattern.Cross; // Fallback to prevent infinite loop
            }
        }

        // Generate the points for procedural patterns like Cross and Diamond.
        SetupGeneratedPoints();
        // Reset smooth damp velocity for next movement
        smoothDampVelocity = Vector2.zero;
        currentLeapVelocity = Vector3.zero;
    }

    /// <summary>
    /// A single move for all linear patterns (Cross, Diamond, Patrol, Lunge).
    /// </summary>
    private IEnumerator LinearMovementRoutine()
    {
        yield return new WaitForSeconds(telegraphTime);

        Vector3 targetPosition = GetNextTargetPosition();

        // Move towards the target using SmoothDamp for acceleration/deceleration.
        while (Vector2.Distance(rb2D.position, targetPosition) > 0.1f)
        {
            Vector2 newPosition = Vector2.SmoothDamp(rb2D.position, targetPosition, ref smoothDampVelocity, smoothTime, moveSpeed);
            rb2D.MovePosition(newPosition);
            yield return new WaitForFixedUpdate();
        }
        rb2D.MovePosition(targetPosition); // Snap to final position.

        yield return new WaitForSeconds(waitTime);

        // If lunging, the boss must return to its starting position.
        if (currentPattern == MovementPattern.LungeTowardsPlayer)
        {
            yield return new WaitForSeconds(telegraphTime);
            while (Vector2.Distance(rb2D.position, startPosition) > 0.1f)
            {
                Vector2 newPosition = Vector2.SmoothDamp(rb2D.position, startPosition, ref smoothDampVelocity, smoothTime, moveSpeed);
                rb2D.MovePosition(newPosition);
                yield return new WaitForFixedUpdate();
            }
            rb2D.MovePosition(startPosition);
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// A single move for the Arc Jump pattern.
    /// </summary>
    private IEnumerator ArcJumpRoutine()
    {
        if (arenaCenter == null)
        {
            Debug.LogError("Arena Center is not assigned for ArcJumps pattern!", this);
            yield break;
        }

        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0);
        Vector3 startJumpPos = arenaCenter.position + direction * arcRadius;
        Vector3 endJumpPos = arenaCenter.position - direction * arcRadius;

        transform.position = startJumpPos; // Instantly move to jump start point
        yield return new WaitForSeconds(telegraphTime);

        float elapsedTime = 0f;
        while (elapsedTime < arcDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / arcDuration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
            Vector3 currentPos = Vector3.Lerp(startJumpPos, endJumpPos, progress);
            currentPos.y += yOffset;
            transform.position = currentPos;
            yield return null;
        }
        transform.position = endJumpPos;
        yield return new WaitForSeconds(waitTime);
    }

    /// <summary>
    /// A single move for the Moon Leap pattern: jump high, pause, crash down, shockwave.
    /// </summary>
    private IEnumerator MoonLeapRoutine()
    {
        Vector3 initialGroundPos = transform.position; // Store current ground position
        Vector3 apexPosition = initialGroundPos + Vector3.up * moonLeapHeight;
        Vector3 impactPosition = playerTransform != null ? playerTransform.position : initialGroundPos; // Crash near player or original spot

        // --- Telegraph ---
        yield return new WaitForSeconds(telegraphTime);

        // --- Leap Up ---
        float timer = 0f;
        currentLeapVelocity = Vector3.zero; // Reset velocity for SmoothDamp
        while (timer < leapUpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / leapUpDuration;
            // Using Vector3.SmoothDamp for the upward motion for a nice ease-in/ease-out
            transform.position = Vector3.SmoothDamp(transform.position, apexPosition, ref currentLeapVelocity, leapUpDuration, moveSpeed);
            yield return null;
        }
        transform.position = apexPosition; // Ensure it reaches the exact apex

        // --- Pause at Apex ("On the Moon") ---
        yield return new WaitForSeconds(moonPauseDuration);

        // --- Crash Down ---
        timer = 0f;
        currentLeapVelocity = Vector3.zero; // Reset velocity for SmoothDamp
        Vector3 currentBossPos = transform.position; // Starting point for the crash down
        while (timer < crashDownDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / crashDownDuration;
            // Smoothly damp from current high position to impact position
            transform.position = Vector3.SmoothDamp(transform.position, impactPosition, ref currentLeapVelocity, crashDownDuration, moveSpeed);
            yield return null;
        }
        transform.position = impactPosition; // Ensure it reaches the exact impact point

        // --- Shockwave on Impact ---
        if (shockwavePrefab != null)
        {
            Vector3 shockwaveSpawnPos = transform.position + shockwaveOffset;
            Instantiate(shockwavePrefab, shockwaveSpawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Shockwave Prefab is not assigned for MoonLeap pattern!", this);
        }

        // --- Return to initial ground position (optional, depending on desired gameplay) ---
        // For now, let's assume the boss stays at the impactPosition until the next move.
        // If you want it to return, uncomment and adjust the following:
        // yield return new WaitForSeconds(telegraphTime); // Small pause before returning
        // while (Vector2.Distance(rb2D.position, initialGroundPos) > 0.1f)
        // {
        //     Vector2 newPosition = Vector2.SmoothDamp(rb2D.position, initialGroundPos, ref smoothDampVelocity, smoothTime, moveSpeed);
        //     rb2D.MovePosition(newPosition);
        //     yield return new WaitForFixedUpdate();
        // }
        // rb2D.MovePosition(initialGroundPos);

        yield return new WaitForSeconds(waitTime); // Pause after the full attack
    }


    /// <summary>
    /// Generates the patrol points for the Cross and Diamond patterns.
    /// </summary>
    private void SetupGeneratedPoints()
    {
        switch (currentPattern)
        {
            case MovementPattern.Cross:
                generatedPatrolPoints = new Vector3[]
                {
                    startPosition + Vector3.up * patrolDistance,
                    startPosition + Vector3.right * patrolDistance,
                    startPosition + Vector3.down * patrolDistance,
                    startPosition + Vector3.left * patrolDistance
                };
                break;
            case MovementPattern.Diamond:
                generatedPatrolPoints = new Vector3[]
               {
                    startPosition + new Vector3(0, patrolDistance),
                    startPosition + new Vector3(patrolDistance, 0),
                    startPosition + new Vector3(0, -patrolDistance),
                    startPosition + new Vector3(-patrolDistance, 0)
               };
                break;
                // No points generated for CustomPatrol, ArcJumps, LungeTowardsPlayer, MoonLeap
        }
        currentPatrolIndex = 0; // Reset index when points are regenerated
    }

    /// <summary>
    /// Gets the next target position based on the current movement pattern.
    /// </summary>
    private Vector3 GetNextTargetPosition()
    {
        Vector3 target;
        switch (currentPattern)
        {
            case MovementPattern.LungeTowardsPlayer:
                if (playerTransform == null)
                {
                    Debug.LogError("Player Transform is not assigned for Lunge pattern! Returning to start.", this);
                    return startPosition;
                }
                target = playerTransform.position;
                break;
            case MovementPattern.CustomPatrol:
                if (patrolPoints == null || patrolPoints.Length == 0)
                {
                    Debug.LogError("Patrol Points are not assigned for CustomPatrol pattern! Returning to start.", this);
                    return startPosition;
                }
                target = patrolPoints[currentPatrolIndex].position;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                break;
            case MovementPattern.Cross:
            case MovementPattern.Diamond:
            default: // Default case also handles patterns where no target is *needed* by this function (like ArcJump/MoonLeap) but prevents error if called
                if (generatedPatrolPoints != null && generatedPatrolPoints.Length > 0)
                {
                    target = generatedPatrolPoints[currentPatrolIndex];
                    currentPatrolIndex = (currentPatrolIndex + 1) % generatedPatrolPoints.Length;
                }
                else
                {
                    // This case should ideally not be hit if SetupGeneratedPoints is called correctly
                    Debug.LogWarning("No generated patrol points available for Cross/Diamond pattern. Returning to start.", this);
                    target = startPosition;
                }
                break;
        }
        return target;
    }
}