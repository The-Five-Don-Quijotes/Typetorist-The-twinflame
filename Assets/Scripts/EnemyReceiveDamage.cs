using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EnemyReceiveDamage : MonoBehaviour
{
    public float health;
    public float maxHealth;
    public BaelorisWordBank wordBank;

    public GameObject bossHealthBar;
    public Slider healthSlider;
    public CanvasGroup bossShield;
    private Animator animator;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public float phase2ShootingDuration;

    [Header("Death Events")]
    public UnityEvent OnHealthZero;
    private bool isDead = false;

    // Lock variable to prevent multiple phase 2 triggers
    private bool hasEnteredPhase2 = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (bossHealthBar != null)
            bossHealthBar.SetActive(true);
        else Debug.LogWarning("Boss Health Bar is not assigned in the inspector.");

        health = maxHealth;

        if (healthSlider != null)
            healthSlider.value = CalculateHealthPercentage();
        else Debug.LogWarning("Health Slider is not assigned in the inspector.");
    }

    private void Update()
    {
        DebugInput();
    }

    private void DebugInput()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1) && !animator.GetBool("isHurt"))
        {
            DealDamage(25);
        }
    }

    public void DealDamage(float damage)
    {
        if (bossShield != null)
        {
            var shieldScript = GetComponent<VorrakShieldCast>();
            if (shieldScript.isShieldOn())
            {
                shieldScript.HideShield();
                return;
            }
        }

        if (animator != null && HasParameter(animator, "isHurt"))
        {
            animator.SetTrigger("isHurt");
        }

        AudioManager.instance.PlaySFX(hurtSound);
        health -= damage;
        CheckDeath();

        if (animator != null && HasParameter(animator, "isShooting"))
        {
            if (!animator.GetBool("isShooting"))
            {
                CheckHalfHealth();
            }
        }
        else
        {
            // If the parameter doesn't exist, just run the function
            CheckHalfHealth();
        }

        healthSlider.value = CalculateHealthPercentage();
    }

    private bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    public void HealEnemy(float heal)
    {
        health += heal;
        CheckOverHeal();
        healthSlider.value = CalculateHealthPercentage();
    }

    private void CheckOverHeal()
    {
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        healthSlider.value = CalculateHealthPercentage();
    }

    private void CheckDeath()
    {
        if (health <= 0 && !isDead)
        {
            isDead = true;
            animator.SetTrigger("isDeath");
            AudioManager.instance.PlaySFX(deathSound);
            bossHealthBar.SetActive(false); // Hide health bar when death

            OnHealthZero?.Invoke();
        }
    }

    private void CheckHalfHealth()
    {
        if (health <= maxHealth / 2 && !hasEnteredPhase2)
        {
            hasEnteredPhase2 = true; // Lock the state

            if (!wordBank.isPhase2)
            {
                // Inject Phase 2 data
                wordBank.SetNewLines(wordBank.phase2Lines);

                // Locate the Typer script in the scene and force it to update the UI
                BaelorisTyper typer = FindFirstObjectByType<BaelorisTyper>();
                if (typer != null)
                {
                    typer.SetCurrentWord();
                    typer.SetCurrentLine();
                }
                else
                {
                    Debug.LogWarning("BaelorisTyper not found in scene.");
                }
            }

            StartShooting();
        }
    }

    void StartShooting()
    {
        if (animator != null)
        {
            if (HasParameter(animator, "StartShooting"))
            {
                animator.SetTrigger("StartShooting");
            }

            if (HasParameter(animator, "isShooting"))
            {
                animator.SetBool("isShooting", true);
            }
        }

        // Stay in shooting for x seconds, then go back to idle
        Invoke(nameof(StopShooting), phase2ShootingDuration);
    }

    void StopShooting()
    {
        if (animator != null && HasParameter(animator, "isShooting"))
        {
            animator.SetBool("isShooting", false);
        }
    }

    private float CalculateHealthPercentage()
    {
        return (health / maxHealth);
    }

    public void DesTroyBoss()
    {
        Destroy(gameObject);
    }
}