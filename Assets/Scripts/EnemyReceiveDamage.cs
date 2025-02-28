using UnityEngine;
using UnityEngine.UI;

public class EnemyReceiveDamage : MonoBehaviour
{
    public float health;
    public float maxHealth;

    public GameObject bossHealthBar;
    public Slider healthSlider;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        bossHealthBar.SetActive(true);
        health = maxHealth;
        healthSlider.value = CalculateHealthPercentage();
    }

    public void DealDamage(float damage)
    {
        animator.SetTrigger("isHurt");
        health -= damage;
        CheckDeath();
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
            Destroy(gameObject);
            bossHealthBar.SetActive(false); //hide health bar when death
        }
    }

    private float CalculateHealthPercentage()
    {
        return (health / maxHealth);
    }
}
