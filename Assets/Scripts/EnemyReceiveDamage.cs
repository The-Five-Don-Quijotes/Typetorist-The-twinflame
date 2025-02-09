using UnityEngine;
using UnityEngine.UI;

public class EnemyReceiveDamage : MonoBehaviour
{
    public float health;
    public float maxHealth;

    public GameObject bossHealthBar;
    public Slider healthSlider;

    void Start()
    {
        bossHealthBar.SetActive(true);
        health = maxHealth;
        healthSlider.value = CalculateHealthPercentage();
    }

    public void DealDamage(float damage)
    {   
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
            Destroy(gameObject);
            bossHealthBar.SetActive(false); //hide health bar when death
        }
    }

    private float CalculateHealthPercentage()
    {
        return (health / maxHealth);
    }
}
