using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats playerStats;

    public GameObject Player;

    public GameObject Book;
    public float minRadius ; //The radius which the book is dropped
    public float maxRadius ;
    public TextMeshProUGUI TypingLine;
    public TextMeshProUGUI TypingText;
    private float bookDropTime = -1f;
    public float TimeToRecollect = 5f;
    public Typer typer;
    private Vector3 respawnPosition;

    public int health;
    public int maxHealth;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    private AudioSource audioSource;

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
        typer = FindFirstObjectByType<Typer>();
        health = maxHealth;
        DisplayHeart();
        audioSource = GetComponent<AudioSource>();
    }

    public void DealDamage(int damage)
    {
        // Check if a Book instance exists in the scene
        if (GameObject.FindWithTag("Book") == null) // Check if a book exists
        {
            Vector3 spawnPosition = GetRandomPositionAroundPlayer();
            TypingText.gameObject.SetActive(false); //Hide the Typer when the book is dropped
            bookDropTime = Time.time;
            Instantiate(Book, spawnPosition, Quaternion.identity);
        }
        else
        {
            // If the Book already exists, deal damage to the player
            health -= damage;
            if (health > 0)
            {
                respawnPosition = transform.position;
                Player.gameObject.SetActive(false);
                Invoke("Respawn", 0.5f);
            }
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
            audioSource.Play();
            Destroy(TypingLine.gameObject);
            Destroy(Player); //dead
        }
    }

    private void Respawn()
    {
        // Reactivate the player
        Player.gameObject.SetActive(true);

        // Make the player temporarily invulnerable
        StartCoroutine(TemporaryInvulnerability(5f));
    }

    private IEnumerator TemporaryInvulnerability(float duration)
    {
        Player.GetComponent<Collider2D>().enabled = false;
        SpriteRenderer spriteRenderer = Player.GetComponent<SpriteRenderer>();

        float elapsedTime = 0f;
        bool isVisible = true;
        float blinkInterval = 0.2f; 

        while (elapsedTime < duration)
        {
            isVisible = !isVisible;
            spriteRenderer.enabled = isVisible;
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
            Book.GetComponent<Collider2D>().enabled = false;
        }

        // Ensure the player is visible and re-enable the collider
        spriteRenderer.enabled = true;
        Player.GetComponent<Collider2D>().enabled = true;
        Book.GetComponent<Collider2D>().enabled = true;
    }


    private Vector3 GetRandomPositionAroundPlayer()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized; 
        float randomDistance = Random.Range(minRadius, maxRadius * 1.5f); 

        Vector2 randomOffset = randomDirection * randomDistance;

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

    private void Update()
    {
        if (GameObject.FindWithTag("Book") != null && (bookDropTime > 0 && Time.time - bookDropTime > TimeToRecollect))
        {
            typer.ResetLine();
            bookDropTime = -1f; // Reset to avoid continuous resetting
        }
    }
}