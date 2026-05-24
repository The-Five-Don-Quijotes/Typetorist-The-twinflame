using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A unified, advanced controller for the "Uyia" boss.
/// It handles multiple movement patterns and can sequence them dynamically.
/// It can ALSO shoot bullets while performing other movements and summon other bosses.
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
        MoonLeap,          // Leaps high up, pauses, crashes down, and creates a shockwave.
        SummonMinion       // Uyia summons another boss to perform an attack.
    }

    // Enum for different bullet firing patterns.
    public enum BulletFirePattern
    {
        Plus,              // Shoots in 4 directions (+ shape: Up, Down, Left, Right)
        Cross,             // Shoots in 4 diagonal directions (X shape: Up-Left, Up-Right, Down-Left, Down-Right)
        Star               // Shoots in 8 directions (* shape: Plus + Cross combined)
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
    public Vector3 shockwaveOffset = new Vector3(0, -1f, 0);


    [Header("Bullet Attack Settings")]
    [Tooltip("The Prefab of the bullet to be fired.")]
    public GameObject bulletPrefab;
    [Tooltip("The speed at which the bullets will travel.")]
    public float bulletSpeed = 10f;
    [Tooltip("How often the boss fires a burst of bullets.")]
    public float fireRate = 1.0f;
    [Tooltip("The pattern in which bullets are fired (Plus, Cross, Star).")]
    public BulletFirePattern bulletFirePattern = BulletFirePattern.Star;


    [Header("Summon Minion Settings")]
    [Tooltip("The Vorrak boss prefab to summon.")]
    public GameObject vorrakPrefab;
    [Tooltip("The Baeloris boss prefab to summon.")]
    public GameObject baelorisPrefab;
    [Tooltip("Where summoned minions will appear relative to Uyia.")]
    public Vector3 summonOffset = new Vector3(0, 2f, 0); // Above Uyia
    [Tooltip("How long the summoned minion stays active before being destroyed.")]
    public float minionActiveTime = 5.0f; // E.g., Vorrak performs one attack then disappears
    [Tooltip("The tag of the GameObject to destroy if a boss is summoned. E.g., 'Player'.")]
    public string playerTagForMinions = "Player"; // Vorrak needs player reference, Baeloris needs player reference


    [Header("Audio Settings")]
    [Tooltip("Sound played when firing a burst of bullets.")]
    public AudioClip shootSound;
    [Tooltip("Sound played when summoning a minion.")]
    public AudioClip summonSound;
    [Tooltip("Sound played when leaping up for Moon Leap.")]
    public AudioClip moonLeapJumpSound;
    [Tooltip("Sound played when crashing down for Moon Leap.")]
    public AudioClip moonLeapImpactSound;


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

    private Coroutine activeShootingCoroutine;

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
            // Stop any existing shooting coroutine before starting a new move.
            if (activeShootingCoroutine != null)
            {
                StopCoroutine(activeShootingCoroutine);
            }
            // Check if the *new* pattern allows shooting.
            if (CanShootDuringPattern(currentPattern))
            {
                activeShootingCoroutine = StartCoroutine(ShootingRoutine());
            }

            switch (currentPattern)
            {
                case MovementPattern.ArcJumps:
                    yield return StartCoroutine(ArcJumpRoutine());
                    break;
                case MovementPattern.MoonLeap:
                    yield return StartCoroutine(MoonLeapRoutine());
                    break;
                case MovementPattern.SummonMinion:
                    yield return StartCoroutine(SummonMinionRoutine());
                    break;
                default: // Handles Cross, Diamond, CustomPatrol, LungeTowardsPlayer
                    yield return StartCoroutine(LinearMovementRoutine());
                    break;
            }
            movesCompleted++;

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

        SetupGeneratedPoints();
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

    #region Jump Routines
    private IEnumerator ArcJumpRoutine()
    {
        if (arenaCenter == null)
        {
            Debug.LogError("Arena Center is not assigned for ArcJumps pattern!", this);
            yield break;
        }

        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(randomAngle), Mathf.Cos(randomAngle), 0);
        Vector3 startJumpPos = arenaCenter.position + direction * arcRadius;
        Vector3 endJumpPos = arenaCenter.position - direction * arcRadius;

        transform.position = startJumpPos;
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

    private IEnumerator MoonLeapRoutine()
    {
        Vector3 initialGroundPos = transform.position;
        Vector3 apexPosition = initialGroundPos + Vector3.up * moonLeapHeight;
        Vector3 impactPosition = playerTransform != null ? playerTransform.position : initialGroundPos;

        yield return new WaitForSeconds(telegraphTime);

        // Play jump sound
        if (AudioManager.instance != null && moonLeapJumpSound != null)
        {
            AudioManager.instance.PlaySFX(moonLeapJumpSound);
        }

        float timer = 0f;
        currentLeapVelocity = Vector3.zero;
        while (timer < leapUpDuration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.SmoothDamp(transform.position, apexPosition, ref currentLeapVelocity, leapUpDuration, moveSpeed);
            yield return null;
        }
        transform.position = apexPosition;

        yield return new WaitForSeconds(moonPauseDuration);

        timer = 0f;
        currentLeapVelocity = Vector3.zero;
        while (timer < crashDownDuration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.SmoothDamp(transform.position, impactPosition, ref currentLeapVelocity, crashDownDuration, moveSpeed);
            yield return null;
        }
        transform.position = impactPosition;

        // Play impact sound
        if (AudioManager.instance != null && moonLeapImpactSound != null)
        {
            AudioManager.instance.PlaySFX(moonLeapImpactSound);
        }

        if (shockwavePrefab != null)
        {
            Vector3 shockwaveSpawnPos = transform.position + shockwaveOffset;
            Instantiate(shockwavePrefab, shockwaveSpawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Shockwave Prefab is not assigned for MoonLeap pattern!", this);
        }

        yield return new WaitForSeconds(waitTime);
    }
    #endregion


    /// <summary>
    /// Coroutine for continuous shooting.
    /// </summary>
    private IEnumerator ShootingRoutine()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab is not assigned! Cannot shoot.", this);
            yield break;
        }

        yield return new WaitForSeconds(fireRate / 2);

        while (true)
        {
            // Trigger shoot sound once per pattern burst
            if (AudioManager.instance != null && shootSound != null)
            {
                AudioManager.instance.PlaySFX(shootSound);
            }

            FireBulletsInPattern(bulletFirePattern);
            yield return new WaitForSeconds(fireRate);
        }
    }

    /// <summary>
    /// Helper function to determine if the boss should be shooting during a given pattern.
    /// </summary>
    private bool CanShootDuringPattern(MovementPattern pattern)
    {
        switch (pattern)
        {
            case MovementPattern.Cross:
            case MovementPattern.Diamond:
            case MovementPattern.CustomPatrol:
            case MovementPattern.LungeTowardsPlayer:
            case MovementPattern.ArcJumps:
                return true;
            case MovementPattern.MoonLeap:
            case MovementPattern.SummonMinion:
            default:
                return false;
        }
    }

    /// <summary>
    /// Fires a single bullet in a given direction.
    /// </summary>
    private void FireBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction.normalized * bulletSpeed;
        }
        else
        {
            Debug.LogWarning("Bullet Prefab is missing a Rigidbody2D component!", bullet);
        }
    }

    /// <summary>
    /// Fires bullets in the specified pattern (Plus, Cross, Star).
    /// </summary>
    private void FireBulletsInPattern(BulletFirePattern pattern)
    {
        if (pattern == BulletFirePattern.Plus || pattern == BulletFirePattern.Star)
        {
            FireBullet(Vector2.up);
            FireBullet(Vector2.down);
            FireBullet(Vector2.left);
            FireBullet(Vector2.right);
        }

        if (pattern == BulletFirePattern.Cross || pattern == BulletFirePattern.Star)
        {
            FireBullet(new Vector2(1, 1).normalized);
            FireBullet(new Vector2(-1, 1).normalized);
            FireBullet(new Vector2(1, -1).normalized);
            FireBullet(new Vector2(-1, -1).normalized);
        }
    }

    /// <summary>
    /// Summon Minion Routine
    /// </summary>
    /// <returns></returns>
    private IEnumerator SummonMinionRoutine()
    {
        yield return new WaitForSeconds(telegraphTime); // Uyia pauses to "cast" summon

        // Play summon sound
        if (AudioManager.instance != null && summonSound != null)
        {
            AudioManager.instance.PlaySFX(summonSound);
        }

        GameObject summonedBoss = null;
        VorrakMovement vorrakScript = null;
        BaelorisMovement baelorisScript = null;

        // Randomly choose which boss to summon
        int bossChoice = Random.Range(0, 2); // 0 for Vorrak, 1 for Baeloris

        Vector3 spawnPos = transform.position + summonOffset;

        if (bossChoice == 0 && vorrakPrefab != null)
        {
            Debug.Log("Uyia summoning Vorrak!");
            summonedBoss = Instantiate(vorrakPrefab, spawnPos, Quaternion.identity);
            vorrakScript = summonedBoss.GetComponent<VorrakMovement>();
            if (vorrakScript != null)
            {
                // Vorrak needs to know where the player is
                vorrakScript.player = playerTransform;
            }
            else
            {
                Debug.LogError("Summoned Vorrak Prefab is missing VorrakMovement script!", summonedBoss);
                Destroy(summonedBoss); // Clean up if script is missing
                yield break;
            }
        }
        else if (bossChoice == 1 && baelorisPrefab != null)
        {
            Debug.Log("Uyia summoning Baeloris!");
            summonedBoss = Instantiate(baelorisPrefab, spawnPos, Quaternion.identity);
            baelorisScript = summonedBoss.GetComponent<BaelorisMovement>();
            if (baelorisScript != null)
            {
                // Baeloris needs to know where the player is
                baelorisScript.player = playerTransform;
            }
            else
            {
                Debug.LogError("Summoned Baeloris Prefab is missing BaelorisMovement script!", summonedBoss);
                Destroy(summonedBoss); // Clean up if script is missing
                yield break;
            }
        }
        else
        {
            Debug.LogWarning("Uyia tried to summon a minion but its prefab was not assigned or choice was invalid!", this);
            yield break;
        }

        // --- Trigger the summoned boss's attack ---
        if (vorrakScript != null)
        {
            Debug.Log("Vorrak activated melee attack!");
            vorrakScript.MoveNearPlayerWithDuration(vorrakScript.cooldown + vorrakScript.hitboxDuration + 0.5f); // Move, then attack
            yield return new WaitForSeconds(vorrakScript.cooldown); // Give him time to move before attack
            vorrakScript.ActivateMeleeHitbox(); // Activate the melee hitbox
        }
        else if (baelorisScript != null)
        {
            Debug.Log("Baeloris activated movement attack!");
            baelorisScript.PickNewTargetPosition(); // Make it pick a target immediately
            baelorisScript.enabled = true; // Make sure its script is active
        }

        // Wait for the minion to be active
        yield return new WaitForSeconds(minionActiveTime);

        // --- Destroy the summoned boss ---
        if (summonedBoss != null)
        {
            Debug.Log($"Destroying summoned minion: {summonedBoss.name}");
            Destroy(summonedBoss);
        }

        yield return new WaitForSeconds(waitTime); // Uyia pauses after the summon
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
        }
        currentPatrolIndex = 0;
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
                    Debug.LogError("Player Transform is not assigned for Lunge pattern!", this);
                    return startPosition;
                }
                target = playerTransform.position;
                break;
            case MovementPattern.CustomPatrol:
                if (patrolPoints == null || patrolPoints.Length == 0)
                {
                    Debug.LogError("Patrol Points are not assigned for CustomPatrol pattern!", this);
                    return startPosition;
                }
                target = patrolPoints[currentPatrolIndex].position;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                break;
            case MovementPattern.Cross:
            case MovementPattern.Diamond:
            default:
                if (generatedPatrolPoints != null && generatedPatrolPoints.Length > 0)
                {
                    target = generatedPatrolPoints[currentPatrolIndex];
                    currentPatrolIndex = (currentPatrolIndex + 1) % generatedPatrolPoints.Length;
                }
                else
                {
                    Debug.LogWarning("No generated patrol points available. Returning to start.", this);
                    target = startPosition;
                }
                break;
        }
        return target;
    }
}