using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyReceiveDamage : MonoBehaviour
{
    public float health;
    public float maxHealth;

    public GameObject bossHealthBar;
    public Slider healthSlider;
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

    public void DealDamage(float damage)
    {
        animator.SetTrigger("isHurt");
        audioSource.PlayOneShot(hurtSound);
        health -= damage;
        CheckDeath();
        if (!animator.GetBool("isShooting"))
        {
            CheckHalfHealth();
        }
        healthSlider.value = CalculateHealthPercentage();
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
        if(health <= 0)
        {
            animator.SetTrigger("isDeath");
            audioSource.PlayOneShot(deathSound);
            StartCoroutine(HandleDeath());
            bossHealthBar.SetActive(false); //hide health bar when death
        }
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(6); // Wait for the death animation to finish
        Destroy(gameObject);
    }

    private void CheckHalfHealth()
    {
        if (health <= maxHealth / 2)
        {
            StartShooting();
        }
    }

    void StartShooting()
    {
        animator.SetTrigger("StartShooting");
        animator.SetBool("isShooting", true);

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
}
