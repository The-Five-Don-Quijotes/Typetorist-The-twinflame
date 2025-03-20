using System.Collections;
using UnityEngine;
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
    private AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public float phase2ShootingDuration;

    void Start()
    {
        animator = GetComponent<Animator>();
        bossHealthBar.SetActive(true);
        health = maxHealth;
        healthSlider.value = CalculateHealthPercentage();
        audioSource = GetComponent<AudioSource>();
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
            if(shieldScript.isShieldOn())
            {
                shieldScript.HideShield();
                return;
            }
        }
        animator.SetTrigger("isHurt");
        audioSource.PlayOneShot(hurtSound);
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
        if (health <= 0)
        {
            animator.SetTrigger("isDeath");
            audioSource.PlayOneShot(deathSound);
            bossHealthBar.SetActive(false); // Hide health bar when death
        }
    }

    private void CheckHalfHealth()
    {
        if (health <= maxHealth / 2)
        {
            wordBank.SetNewLines(wordBank.phase2Lines);
            wordBank.ResetToFirstWordOfCurrentLine();
            StartShooting();
        }
    }

    void StartShooting()
    {
        if (animator != null && HasParameter(animator, "StartShooting"))
        {
            animator.SetTrigger("StartShooting");
            animator.SetBool("isShooting", true);
        }

        // Stay in shooting for x seconds, then go back to idle
        Invoke(nameof(StopShooting), phase2ShootingDuration);
    }

    void StopShooting()
    {
        animator.SetBool("isShooting", false);
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
