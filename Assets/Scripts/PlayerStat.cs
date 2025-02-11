using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats playerStats;

    public GameObject Player;

    public GameObject Book;
    public float minRadius; //The radius which the book is dropped
    public float maxRadius;
    public TextMeshProUGUI TypingLine;
    public TextMeshProUGUI TypingText;

    public int health;
    public int maxHealth;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

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
        DisplayHeart();
    }

    public void DealDamage(int damage)
    {
        // Check if a Book instance exists in the scene
        if (GameObject.FindWithTag("Book") == null) // Check if a book exists
        {
            Vector3 spawnPosition = GetRandomPositionAroundPlayer();
            TypingText.gameObject.SetActive(false); //Hide the Typer when the book is dropped
            Instantiate(Book, spawnPosition, Quaternion.identity);
        }
        else
        {
            // If the Book already exists, deal damage to the player
            health -= damage;
            CheckDeath();
            DisplayHeart();
        }
    }

    public void HealCharacter(int heal)
    {
        health += heal;
        CheckOverheal();
        DisplayHeart();
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
    }

    private Vector3 GetRandomPositionAroundPlayer()
    {
        Vector2 randomOffset;
        do
        {
            randomOffset = Random.insideUnitCircle * maxRadius; // Get a random point
        }
        while (randomOffset.magnitude < minRadius); // Re-roll if inside minRadius

        return new Vector3(Player.transform.position.x + randomOffset.x,
                           Player.transform.position.y,
                           1);
    }

    public void DisplayHeart()
    {
        foreach (Image img in hearts)
        {
            img.sprite = emptyHeart;
        }
        for(int i = 0; i < health; i++)
        {
            hearts[i].sprite = fullHeart;
        }
    }

    public void ShowTyper()
    {
        TypingText.gameObject.SetActive(true);
    }
}
