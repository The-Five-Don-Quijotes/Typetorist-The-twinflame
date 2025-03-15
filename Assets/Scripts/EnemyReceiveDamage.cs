using System.Collections;
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
        if (health <= 0)
        {
            animator.SetTrigger("isDeath");
            audioSource.PlayOneShot(deathSound);
            float deathAnimDuration = GetAnimationClipLength("death"); // Get exact animation length
            Debug.Log(deathAnimDuration);
            StartCoroutine(HandleDeath(deathAnimDuration));
            bossHealthBar.SetActive(false); // Hide health bar when death
        }
    }

    private IEnumerator HandleDeath(float duration)
    {
        yield return new WaitForSeconds(duration*10); // Wait for the actual death animation to finish
        Destroy(gameObject);
    }

    private float GetAnimationClipLength(string clipName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        Debug.LogWarning($"Animation clip '{clipName}' not found!");
        return 0f; // Default if not found
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
