using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats playerStats;

    public GameObject Player;
    public GameObject PlayerHealthBar;
    public Slider PlayerHealthSlider;
    public TextMeshProUGUI HealthText;

    public TextMeshProUGUI TypingLine;

    public float health;
    public float maxHealth;

    private void Awake()
    {
        if(playerStats != null)
        {
            Destroy(playerStats);
        }
        else
        {
            playerStats = this;
        }
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        health = maxHealth;
        PlayerHealthSlider.value = CalculateHealthPercentage();
    }

    public void DealDamage(float damage)
    {
        health -= damage;
        CheckDeath();
        PlayerHealthSlider.value = CalculateHealthPercentage();
    }

    public void HealCharacter(float heal)
    {
        health += heal;
        CheckOverheal();
        PlayerHealthSlider.value = CalculateHealthPercentage();
    }

    private void CheckOverheal()
    {
        if(health > maxHealth)
        {
            health = maxHealth;
        }
    }

    private void CheckDeath()
    {
        if(health <= 0)
        {
            if(health < 0)
            {
                health = 0;
            }
            Destroy(TypingLine.gameObject);
            Destroy(Player); //dead
        }
        Debug.Log("Current health: " + health);
    }

    private float CalculateHealthPercentage()
    {
        HealthText.text = Mathf.RoundToInt(health) + "/" + Mathf.RoundToInt(maxHealth); //update health text
        return (health / maxHealth);
    }
}
