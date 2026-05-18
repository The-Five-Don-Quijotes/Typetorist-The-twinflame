using System.Collections;
using UnityEngine;

public class VorrakController : MonoBehaviour
{
    private Animator animator;
    private EnemyReceiveDamage healthSystem;

    [Header("Combat Settings")]
    public float attackCooldown = 3f;
    public float followDuration = 5f;

    [Header("Cutscene Integration")]
    public BossCutsceneController mainCutsceneController;
    public GameObject armPrefab;
    public GameObject explosionVFX;
    public AudioClip explosionSound;
    public GameObject portal1;

    private float nextAttackTime;
    private bool isFirst50 = true;
    private bool isFirst25 = true;
    private bool isCombatActive = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<EnemyReceiveDamage>();
        if (portal1 != null) portal1.SetActive(false);
    }

    private void Update()
    {
        if (!isCombatActive) return;

        if (Time.time >= nextAttackTime)
        {
            ChooseAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    public void BeginCombatPhase()
    {
        isCombatActive = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    public void StopCombatPhase()
    {
        isCombatActive = false;

        // Disable all modular combat components to prevent execution during cutscene
        GetComponent<VorrakMovement>()?.StopMovementPhase();
        if (GetComponent<VorrakShooting>() != null) GetComponent<VorrakShooting>().enabled = false;
        if (GetComponent<VorrakShootingArm>() != null) GetComponent<VorrakShootingArm>().enabled = false;
        if (GetComponent<VorrakShieldCast>() != null) GetComponent<VorrakShieldCast>().enabled = false;
    }

    public void ExecuteFinalGateSequence()
    {
        // Lock player movement immediately
        if (mainCutsceneController != null && mainCutsceneController.playerMovementScript != null)
        {
            mainCutsceneController.playerMovementScript.enabled = false;
        }

        StopCombatPhase();
        StopAllCoroutines();

        // Resolve Animator Conflict: Cancel the generic death trigger set by EnemyReceiveDamage
        animator.ResetTrigger("isDeath");
        animator.Play("Idle", -1, 0f); // Force idle state before shooting

        StartCoroutine(FinalSequenceCoroutine());
    }

    private IEnumerator FinalSequenceCoroutine()
    {
        animator.SetTrigger("isShootingArm");
        yield return new WaitForSeconds(0.5f);

        Vector3 spawnPosition = transform.position + new Vector3(0, 2f, 0);
        GameObject cutsceneArm = Instantiate(armPrefab, spawnPosition, Quaternion.identity);

        // Strip combat components from the cutscene arm to prevent physics interference
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

        // Manually translate the arm to ensure strict progression without orbiting or hanging
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

        // Anchor camera at explosion site
        GameObject tempCamTarget = new GameObject("TempCamTarget");
        tempCamTarget.transform.position = explosionTargetPos;
        if (mainCutsceneController != null && mainCutsceneController.mainCamera != null)
        {
            mainCutsceneController.mainCamera.SetTarget(tempCamTarget.transform);
        }

        // Execute visual/audio effects
        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, explosionTargetPos, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        if (explosionSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(explosionSound);
        }

        // Activate assigned portal
        if (portal1 != null)
        {
            portal1.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        // Restore camera to player focus
        if (mainCutsceneController != null && mainCutsceneController.mainCamera != null && mainCutsceneController.player != null)
        {
            mainCutsceneController.mainCamera.SetTarget(mainCutsceneController.player);
        }

        Destroy(tempCamTarget);

        // Re-apply the death trigger for the final sequence
        animator.SetTrigger("isDeath");

        // Handshake to main sequence
        if (mainCutsceneController != null)
        {
            mainCutsceneController.TriggerDeathSequence();
        }
    }

    // Standard combat logic mapping
    private void ChooseAttack()
    {
        if (healthSystem == null) return;
        float healthPercentage = healthSystem.health / healthSystem.maxHealth;
        int attackChoice = Random.Range(0, 100);

        if (healthPercentage > 0.75f)
        {
            if (attackChoice < 75) { GetComponent<VorrakMovement>()?.MoveNearPlayerWithDuration(followDuration); TriggerMeleeAttack(); }
            else TriggerShootingArm(3);
        }
        else if (healthPercentage > 0.5f)
        {
            if (attackChoice < 50) { GetComponent<VorrakMovement>().MoveNearPlayerWithDuration(followDuration); TriggerMeleeAttack(); }
            else TriggerShootingArm(5);
        }
        else if (healthPercentage > 0.25f)
        {
            if (isFirst50) { isFirst50 = false; TriggerShield(); }
            if (attackChoice < 50) TriggerShootingArm(7); else TriggerLaser();
            if (attackChoice < 5) TriggerShield();
        }
        else
        {
            if (isFirst25) { isFirst25 = false; TriggerShield(); }
            if (attackChoice < 50) TriggerShootingArm(9); else TriggerLaser();
            if (attackChoice < 10) TriggerShield();
        }
    }

    private void TriggerMeleeAttack() { animator.SetTrigger("isAttacking"); }
    private void TriggerShootingArm(int count) { StartCoroutine(ShootArms(count)); }
    private IEnumerator ShootArms(int count) { for (int i = 0; i < count; i++) { animator.SetTrigger("isShootingArm"); yield return new WaitForSeconds(0.8f); } }
    private void TriggerLaser() { animator.SetTrigger("isShootingLaser"); }
    private void TriggerShield() { animator.SetTrigger("ShieldCast"); }
}