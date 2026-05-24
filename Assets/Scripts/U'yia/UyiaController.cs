using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A unified, advanced controller for the "Uyia" boss.
/// It handles multiple movement patterns, dynamic sequences, and health-based phases.
/// </summary>
public class UyiaBossController : MonoBehaviour
{
    public enum MovementBehavior
    {
        FixedPattern,
        DynamicSequence,
        HealthPhasedSequence
    }

    public enum MovementPattern
    {
        Cross,
        Diamond,
        CustomPatrol,
        ArcJumps,
        LungeTowardsPlayer,
        MoonLeap,
        SummonMinion
    }

    public enum BulletFirePattern
    {
        Plus,
        Cross,
        Star
    }

    [Header("AI Behavior")]
    [Tooltip("Choose if the boss uses one fixed pattern, cycles through a sequence, or uses health phases.")]
    public MovementBehavior behavior = MovementBehavior.HealthPhasedSequence;

    [Tooltip("The single pattern to use if behavior is 'FixedPattern'.")]
    public MovementPattern fixedPattern = MovementPattern.Cross;

    [Tooltip("The sequence of patterns to follow in 'DynamicSequence' mode.")]
    public List<MovementPattern> dynamicSequence = new List<MovementPattern>();

    [Tooltip("How many moves to perform in one pattern before switching to the next.")]
    public int movesPerPattern = 4;


    [Header("Health Phase Settings")]
    [Tooltip("Sequence used when health is above 70%.")]
    public List<MovementPattern> phase1Sequence = new List<MovementPattern>();

    [Tooltip("Sequence used when health is between 70% and 30%.")]
    public List<MovementPattern> phase2Sequence = new List<MovementPattern>();

    [Tooltip("Sequence used when health is below 30%.")]
    public List<MovementPattern> phase3Sequence = new List<MovementPattern>();


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
    [Tooltip("The Zhavok boss prefab to summon.")]
    public GameObject zhavokPrefab;
    [Tooltip("Where summoned minions will appear relative to Uyia.")]
    public Vector3 summonOffset = new Vector3(0, 2f, 0);
    [Tooltip("How long the summoned minion stays active before being destroyed.")]
    public float minionActiveTime = 5.0f;
    [Tooltip("The tag of the GameObject to destroy if a boss is summoned. E.g., 'Player'.")]
    public string playerTagForMinions = "Player";


    [Header("Audio Settings")]
    [Tooltip("Sound played when firing a burst of bullets.")]
    public AudioClip shootSound;
    [Tooltip("Sound played when summoning a minion.")]
    public AudioClip summonSound;
    [Tooltip("Sound played when leaping up for Moon Leap.")]
    public AudioClip moonLeapJumpSound;
    [Tooltip("Sound played when crashing down for Moon Leap.")]
    public AudioClip moonLeapImpactSound;


    private Rigidbody2D rb2D;
    private EnemyReceiveDamage healthComponent;

    private Vector3 startPosition;
    private Vector3[] generatedPatrolPoints;
    private int currentPatrolIndex = 0;
    private MovementPattern currentPattern;

    private int currentPhase = 1;
    private int dynamicSequenceIndex = 0;
    private int movesCompleted = 0;

    private Vector2 smoothDampVelocity;
    private Vector3 currentLeapVelocity = Vector3.zero;
    private Coroutine activeShootingCoroutine;

    void Start()
    {
        startPosition = transform.position;

        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            rb2D = gameObject.AddComponent<Rigidbody2D>();
        }
        rb2D.bodyType = RigidbodyType2D.Kinematic;

        healthComponent = GetComponent<EnemyReceiveDamage>();

        if (behavior == MovementBehavior.HealthPhasedSequence)
        {
            currentPhase = 1;
            dynamicSequence = phase1Sequence;
        }

        StartCoroutine(BehaviorManager());
    }

    private void CheckHealthPhases()
    {
        if (healthComponent == null || behavior != MovementBehavior.HealthPhasedSequence) return;

        float healthPercentage = healthComponent.health / healthComponent.maxHealth;
        int targetPhase = 1;

        if (healthPercentage <= 0.3f)
        {
            targetPhase = 3;
        }
        else if (healthPercentage <= 0.7f)
        {
            targetPhase = 2;
        }

        if (targetPhase != currentPhase)
        {
            currentPhase = targetPhase;
            dynamicSequenceIndex = 0;
            movesCompleted = 0;

            if (currentPhase == 1) dynamicSequence = phase1Sequence;
            else if (currentPhase == 2) dynamicSequence = phase2Sequence;
            else if (currentPhase == 3) dynamicSequence = phase3Sequence;
        }
    }

    private IEnumerator BehaviorManager()
    {
        while (true)
        {
            CheckHealthPhases();
            UpdateCurrentPattern();

            if (activeShootingCoroutine != null)
            {
                StopCoroutine(activeShootingCoroutine);
            }

            if (CanShootDuringPattern(currentPattern))
            {
                activeShootingCoroutine = StartCoroutine(ShootingRoutine());
            }

            switch (currentPattern)
            {
                case MovementPattern.ArcJumps:
                    yield return StartCoroutine(ArcJumpRoutine());
                    movesCompleted++;
                    break;
                case MovementPattern.MoonLeap:
                    yield return StartCoroutine(MoonLeapRoutine());
                    movesCompleted++;
                    break;
                case MovementPattern.SummonMinion:
                    StartCoroutine(SummonMinionRoutine());

                    yield return StartCoroutine(LinearMovementRoutine());

                    // Force the sequence to advance immediately to prevent multiple consecutive summons
                    movesCompleted = movesPerPattern;
                    break;
                default:
                    yield return StartCoroutine(LinearMovementRoutine());
                    movesCompleted++;
                    break;
            }

            if ((behavior == MovementBehavior.DynamicSequence || behavior == MovementBehavior.HealthPhasedSequence) && movesCompleted >= movesPerPattern)
            {
                movesCompleted = 0;
                if (dynamicSequence != null && dynamicSequence.Count > 0)
                {
                    dynamicSequenceIndex = (dynamicSequenceIndex + 1) % dynamicSequence.Count;
                }
            }
        }
    }

    private void UpdateCurrentPattern()
    {
        if (behavior == MovementBehavior.FixedPattern)
        {
            currentPattern = fixedPattern;
        }
        else
        {
            if (dynamicSequence != null && dynamicSequence.Count > 0)
            {
                currentPattern = dynamicSequence[dynamicSequenceIndex];
            }
            else
            {
                currentPattern = MovementPattern.Cross;
            }
        }

        SetupGeneratedPoints();
        smoothDampVelocity = Vector2.zero;
        currentLeapVelocity = Vector3.zero;
    }

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
        rb2D.MovePosition(targetPosition);

        yield return new WaitForSeconds(waitTime);

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
        if (arenaCenter == null) yield break;

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

        if (AudioManager.instance != null && moonLeapImpactSound != null)
        {
            AudioManager.instance.PlaySFX(moonLeapImpactSound);
        }

        if (shockwavePrefab != null)
        {
            Vector3 shockwaveSpawnPos = transform.position + shockwaveOffset;
            Instantiate(shockwavePrefab, shockwaveSpawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(waitTime);
    }
    #endregion

    private IEnumerator ShootingRoutine()
    {
        if (bulletPrefab == null) yield break;

        yield return new WaitForSeconds(fireRate / 2f);

        while (true)
        {
            if (AudioManager.instance != null && shootSound != null)
            {
                AudioManager.instance.PlaySFX(shootSound);
            }

            FireBulletsInPattern(bulletFirePattern);
            yield return new WaitForSeconds(fireRate);
        }
    }

    private bool CanShootDuringPattern(MovementPattern pattern)
    {
        switch (pattern)
        {
            case MovementPattern.Cross:
            case MovementPattern.Diamond:
            case MovementPattern.CustomPatrol:
            case MovementPattern.LungeTowardsPlayer:
            case MovementPattern.ArcJumps:
            case MovementPattern.SummonMinion:
                return true;
            case MovementPattern.MoonLeap:
            default:
                return false;
        }
    }

    private void FireBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction.normalized * bulletSpeed;
        }
    }

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

    private IEnumerator SummonMinionRoutine()
    {
        yield return new WaitForSeconds(telegraphTime);

        if (AudioManager.instance != null && summonSound != null)
        {
            AudioManager.instance.PlaySFX(summonSound);
        }

        GameObject summonedBoss = null;
        int bossChoice = Random.Range(0, 3);
        Vector3 spawnPos = transform.position + summonOffset;

        if (bossChoice == 0 && vorrakPrefab != null)
        {
            summonedBoss = Instantiate(vorrakPrefab, spawnPos, Quaternion.identity);

            VorrakMovement vorrakScript = summonedBoss.GetComponent<VorrakMovement>();
            Animator vorrakAnim = summonedBoss.GetComponent<Animator>();

            if (vorrakScript != null)
            {
                vorrakScript.player = playerTransform;
                vorrakScript.BeginMovementPhase();

                int attackChoice = Random.Range(0, 3);

                if (attackChoice == 0)
                {
                    float followTime = 1.5f;
                    vorrakScript.MoveNearPlayerWithDuration(followTime);

                    yield return new WaitForSeconds(followTime);

                    if (vorrakAnim != null) vorrakAnim.SetTrigger("isAttacking");
                    vorrakScript.ActivateMeleeHitbox();
                }
                else if (attackChoice == 1)
                {
                    if (vorrakAnim != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            vorrakAnim.SetTrigger("isShootingArm");
                            yield return new WaitForSeconds(0.8f);
                        }
                    }
                }
                else
                {
                    if (vorrakAnim != null) vorrakAnim.SetTrigger("isShootingLaser");
                }
            }
        }
        else if (bossChoice == 1 && baelorisPrefab != null)
        {
            summonedBoss = Instantiate(baelorisPrefab, spawnPos, Quaternion.identity);

            BaelorisMovement baelorisScript = summonedBoss.GetComponent<BaelorisMovement>();
            EnemyShooting baelorisShooting = summonedBoss.GetComponent<EnemyShooting>();
            EnemyReceiveDamage baelorisHealth = summonedBoss.GetComponent<EnemyReceiveDamage>();

            int baelorisAttackChoice = Random.Range(0, 2);

            if (baelorisHealth != null && baelorisAttackChoice == 1)
            {
                baelorisHealth.health = baelorisHealth.maxHealth / 2f;
            }

            if (baelorisScript != null)
            {
                baelorisScript.player = playerTransform;
                baelorisScript.BeginMovementPhase();

                if (baelorisAttackChoice == 0)
                {
                    baelorisScript.PickNewTargetPosition();
                }
            }

            if (baelorisShooting != null)
            {
                baelorisShooting.player = playerTransform;
                baelorisShooting.BeginShootingPhase();
            }
        }
        else if (bossChoice == 2 && zhavokPrefab != null)
        {
            summonedBoss = Instantiate(zhavokPrefab, spawnPos, Quaternion.identity);

            ZhavokMovement zhavokMovement = summonedBoss.GetComponent<ZhavokMovement>();
            ZhavokAttack zhavokAttack = summonedBoss.GetComponent<ZhavokAttack>();
            ZhavokPhase2Summon zhavokSummon = summonedBoss.GetComponent<ZhavokPhase2Summon>();

            if (zhavokMovement != null) zhavokMovement.BeginMovementPhase();
            if (zhavokAttack != null) zhavokAttack.BeginAttackPhase();

            if (zhavokSummon != null)
            {
                yield return null;

                for (int i = 0; i < 4; i++)
                {
                    summonedBoss.SendMessage("DoSummon", i, SendMessageOptions.DontRequireReceiver);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
        else
        {
            yield break;
        }

        yield return new WaitForSeconds(minionActiveTime);

        if (summonedBoss != null)
        {
            VorrakMovement vScript = summonedBoss.GetComponent<VorrakMovement>();
            if (vScript != null) vScript.StopMovementPhase();

            BaelorisMovement bScript = summonedBoss.GetComponent<BaelorisMovement>();
            if (bScript != null) bScript.StopMovementPhase();

            EnemyShooting eShooting = summonedBoss.GetComponent<EnemyShooting>();
            if (eShooting != null) eShooting.StopShootingPhase();

            ZhavokMovement zMovement = summonedBoss.GetComponent<ZhavokMovement>();
            if (zMovement != null) zMovement.StopMovementPhase();

            ZhavokAttack zAttack = summonedBoss.GetComponent<ZhavokAttack>();
            if (zAttack != null) zAttack.StopAttackPhase();

            Destroy(summonedBoss);
        }
    }

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

    private Vector3 GetNextTargetPosition()
    {
        Vector3 target;
        switch (currentPattern)
        {
            case MovementPattern.LungeTowardsPlayer:
                if (playerTransform == null) return startPosition;
                target = playerTransform.position;
                break;
            case MovementPattern.CustomPatrol:
                if (patrolPoints == null || patrolPoints.Length == 0) return startPosition;
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
                    target = startPosition;
                }
                break;
        }
        return target;
    }
}