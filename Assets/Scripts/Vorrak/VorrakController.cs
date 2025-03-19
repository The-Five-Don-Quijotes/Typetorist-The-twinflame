using UnityEngine;

public class VorrakController : MonoBehaviour
{
    private Animator animator;
    private EnemyReceiveDamage healthSystem;

    public float attackCooldown = 3f; // Time between attacks
    private float nextAttackTime;

    private bool isFirst50 = true;
    private bool isFirst25 = true;

    private void Start()
    {
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<EnemyReceiveDamage>();
        nextAttackTime = Time.time + attackCooldown;
    }

    private void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            ChooseAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void ChooseAttack()
    {
        if (healthSystem == null) return;

        float healthPercentage = healthSystem.health / healthSystem.maxHealth;
        int attackChoice = Random.Range(0, 100);

        if (healthPercentage > 0.75f)
        {
            // Phase 1: 75% Melee, 25% Arm
            if (attackChoice < 75)
            {
                TriggerMeleeAttack();
            }
            else
            {
                TriggerShootingArm(3);
            }
        }
        else if (healthPercentage > 0.5f)
        {
            // Phase 2: 50% Melee, 50% Arm (Shoots 2 arms)
            if (attackChoice < 50)
            {
                TriggerMeleeAttack();
            }
            else
            {
                TriggerShootingArm(5);
            }
        }
        else if (healthPercentage > 0.25f)
        {
            if (isFirst50)
            {
                isFirst50 = false;
                TriggerShield();
            }
            // Phase 3: Uses Laser & Arm
            if (attackChoice < 50)
            {
                TriggerShootingArm(7);
            }
            else
            {
                TriggerLaser();
            }
            if (attackChoice < 10) TriggerShield();
        }
        else
        {
            if (isFirst25)
            {
                isFirst25 = false;
                TriggerShield();
            }
            if (attackChoice < 50)
            {
                TriggerShootingArm(9);
            }
            else
            {
                TriggerLaser();
            }
            if (attackChoice < 20) TriggerShield();
        }
    }

    private void TriggerMeleeAttack()
    {
        animator.SetTrigger("isAttacking");
    }

    private void TriggerShootingArm(int count)
    {
        animator.SetTrigger("isShootingArm");
        //animator.SetInteger("ArmCount", count); // ArmCount controls how many arms to shoot
    }

    private void TriggerLaser()
    {
        animator.SetTrigger("isShootingLaser");
    }

    private void TriggerShield()
    {
        animator.SetTrigger("ShieldCast");
    }
}
