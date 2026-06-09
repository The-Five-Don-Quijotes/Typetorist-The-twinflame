using System.Collections;
using UnityEngine;

public class VorrakController : MonoBehaviour
{
    private Animator animator;
    private EnemyReceiveDamage healthSystem;
    private VorrakMovement movementScript;

    [Header("Combat Settings")]
    public float baseAttackCooldown = 3.5f;
    public float minAttackCooldown = 1.5f;

    [Header("Melee Settings")]
    public float followDuration = 4f;
    public float meleeStrikeRange = 2.5f;

    [Header("Shield Settings")]
    public float shieldCooldown = 12f;

    [Header("Cutscene Integration")]
    public BossCutsceneController mainCutsceneController;
    public GameObject armPrefab;
    public GameObject explosionVFX;
    public AudioClip explosionSound;
    public GameObject portal1;
    public AudioClip shootingSound;

    private float nextAttackTime;
    private float currentAttackCooldown;
    private float nextShieldTime;

    private bool isFirst50 = true;
    private bool isFirst25 = true;
    private bool isCombatActive = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<EnemyReceiveDamage>();
        movementScript = GetComponent<VorrakMovement>();
        if (portal1 != null) portal1.SetActive(false);
    }

    private void Update()
    {
        if (!isCombatActive) return;

        if (Time.time >= nextAttackTime)
        {
            ChooseAttack();
        }
    }

    public void BeginCombatPhase()
    {
        isCombatActive = true;
        currentAttackCooldown = baseAttackCooldown;
        nextAttackTime = Time.time + 1f;
        nextShieldTime = Time.time + shieldCooldown;
    }

    public void StopCombatPhase()
    {
        isCombatActive = false;

        GetComponent<VorrakMovement>()?.StopMovementPhase();
        if (GetComponent<VorrakShooting>() != null) GetComponent<VorrakShooting>().enabled = false;
        if (GetComponent<VorrakShootingArm>() != null) GetComponent<VorrakShootingArm>().enabled = false;
        if (GetComponent<VorrakShieldCast>() != null) GetComponent<VorrakShieldCast>().enabled = false;
    }

    public void ExecuteFinalGateSequence()
    {
        if (mainCutsceneController != null && mainCutsceneController.playerMovementScript != null)
        {
            mainCutsceneController.playerMovementScript.enabled = false;
        }

        StopCombatPhase();
        StopAllCoroutines();

        animator.ResetTrigger("isDeath");
        animator.Play("Idle", -1, 0f);

        StartCoroutine(FinalSequenceCoroutine());
    }

    private IEnumerator FinalSequenceCoroutine()
    {
        animator.SetTrigger("isShootingArm");
        yield return new WaitForSeconds(0.5f);

        Vector3 spawnPosition = transform.position + new Vector3(0, 2f, 0);
        GameObject cutsceneArm = Instantiate(armPrefab, spawnPosition, Quaternion.identity);

        if (cutsceneArm.GetComponent<TestEnemyProjectile>() != null) Destroy(cutsceneArm.GetComponent<TestEnemyProjectile>());
        if (cutsceneArm.GetComponent<ArmFollowPlayer>() != null) Destroy(cutsceneArm.GetComponent<ArmFollowPlayer>());

        Collider2D[] colliders = cutsceneArm.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        Vector3 explosionTargetPos = new Vector3(0, 12f, 0);

        if (mainCutsceneController != null && mainCutsceneController.mainCamera != null)
        {
            mainCutsceneController.mainCamera.SetTarget(cutsceneArm.transform);
        }

        float armSpeed = 15f;
        while (cutsceneArm != null && Vector3.Distance(cutsceneArm.transform.position, explosionTargetPos) > 0.1f)
        {
            cutsceneArm.transform.position = Vector3.MoveTowards(cutsceneArm.transform.position, explosionTargetPos, armSpeed * Time.deltaTime);

            Vector3 direction = (explosionTargetPos - cutsceneArm.transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            cutsceneArm.transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        if (cutsceneArm != null) Destroy(cutsceneArm);

        GameObject tempCamTarget = new GameObject("TempCamTarget");
        tempCamTarget.transform.position = explosionTargetPos;
        if (mainCutsceneController != null && mainCutsceneController.mainCamera != null)
        {
            mainCutsceneController.mainCamera.SetTarget(tempCamTarget.transform);
        }

        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, explosionTargetPos, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        if (explosionSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(explosionSound);
        }

        if (portal1 != null)
        {
            portal1.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        if (mainCutsceneController != null && mainCutsceneController.mainCamera != null && mainCutsceneController.player != null)
        {
            mainCutsceneController.mainCamera.SetTarget(mainCutsceneController.player);
        }

        Destroy(tempCamTarget);

        animator.SetTrigger("isDeath");

        if (mainCutsceneController != null)
        {
            mainCutsceneController.TriggerDeathSequence();
        }
    }

    private void ChooseAttack()
    {
        if (healthSystem == null) return;

        float healthPercentage = healthSystem.health / healthSystem.maxHealth;
        int attackChoice = Random.Range(0, 100);

        // Dynamically scale cooldown based on health
        currentAttackCooldown = Mathf.Lerp(minAttackCooldown, baseAttackCooldown, healthPercentage);

        // Ensure standard attacks are halted if a shield cast takes priority
        if (healthPercentage <= 0.5f && Time.time >= nextShieldTime)
        {
            TriggerShield();
            nextShieldTime = Time.time + shieldCooldown;
            nextAttackTime = Time.time + currentAttackCooldown;
            return;
        }

        if (healthPercentage > 0.75f)
        {
            if (attackChoice < 60) ExecuteChaseAndMelee();
            else TriggerShootingArm(3);
        }
        else if (healthPercentage > 0.5f)
        {
            if (attackChoice < 40) ExecuteChaseAndMelee();
            else if (attackChoice < 80)
            {
                // Align to player's Y-axis before firing to deny safe zones
                movementScript?.TeleportToPlayerYLane();
                TriggerShootingArm(4);
            }
            else TriggerLaserSequence();
        }
        else if (healthPercentage > 0.25f)
        {
            if (isFirst50)
            {
                isFirst50 = false;
                TriggerShield();
                nextShieldTime = Time.time + shieldCooldown;
                nextAttackTime = Time.time + currentAttackCooldown;
                return;
            }

            if (attackChoice < 40)
            {
                // Track player vertically to eliminate bottom corner camping
                movementScript?.TeleportToPlayerYLane();
                TriggerShootingArm(5);
            }
            else if (attackChoice < 70)
            {
                TriggerLaserSequence();
            }
            else
            {
                // Retain random teleports to maintain unpredictability
                movementScript?.TeleportToRandomPosition();
                TriggerShootingArm(3);
            }
        }
        else
        {
            if (isFirst25)
            {
                isFirst25 = false;
                TriggerShield();
                nextShieldTime = Time.time + shieldCooldown;
                nextAttackTime = Time.time + currentAttackCooldown;
                return;
            }

            if (attackChoice < 30)
            {
                movementScript?.TeleportToPlayerYLane();
                TriggerShootingArm(7);
            }
            else if (attackChoice < 60)
            {
                TriggerLaserSequence();
            }
            else
            {
                ExecuteChaseAndMelee();
            }
        }

        // Apply default cooldown
        nextAttackTime = Time.time + currentAttackCooldown;
    }

    private void ExecuteChaseAndMelee()
    {
        nextAttackTime = Time.time + followDuration + currentAttackCooldown;
        StartCoroutine(movementScript.ChasePlayerForMelee(followDuration, meleeStrikeRange, TriggerMeleeAttack));
    }

    private void TriggerMeleeAttack()
    {
        animator.SetTrigger("isAttacking");
        nextAttackTime = Time.time + currentAttackCooldown;
    }

    private void TriggerShootingArm(int count)
    {
        StartCoroutine(ShootArms(count));
    }

    private IEnumerator ShootArms(int count)
    {
        // Suspend attack logic while shooting arms
        nextAttackTime = Time.time + (count * 0.8f) + currentAttackCooldown;

        for (int i = 0; i < count; i++)
        {
            if (shootingSound != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(shootingSound);
            }
            animator.SetTrigger("isShootingArm");
            yield return new WaitForSeconds(0.8f);
        }
    }

    private void TriggerLaserSequence()
    {
        // Calculates total sweep time: Distance 18 (from Y=7 to Y=-11) / moveSpeed + Telegraph delay
        float estimatedLaserDuration = (18f / movementScript.moveSpeed) + 0.6f;

        // Locks the state machine until the sweep completes
        nextAttackTime = Time.time + estimatedLaserDuration + currentAttackCooldown;

        StartCoroutine(LaserAlignmentCoroutine());
    }

    private IEnumerator LaserAlignmentCoroutine()
    {
        movementScript?.TeleportForLaserSweep();

        yield return new WaitForSeconds(0.6f);

        animator.SetTrigger("isShootingLaser");
    }

    private void TriggerShield()
    {
        animator.SetTrigger("ShieldCast");
    }
}