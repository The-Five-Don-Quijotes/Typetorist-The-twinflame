using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats playerStats;

    public GameObject Player;
    public GameObject Boss;

    public GameObject Book;
    public GameObject spawnedBook;
    public float minRadius ; //The radius which the book is dropped
    public float maxRadius ;
    public TextMeshProUGUI TypingLine;
    public TextMeshProUGUI TypingText;
    private float bookDropTime = -1f;
    public float TimeToRecollect = 3f;
    public float minDistanceFromPlayer = 3f;
    public float safeDistanceFromBoss = 4f;
    public Typer typer;
    private Vector3 respawnPosition;

    public int health;
    public int maxHealth;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    private AudioSource audioSource;

    public bool isGodMode = false;

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

        isGodMode = true;
    }

    public void DealDamage(int damage) 
    {
        if (isGodMode)
        {
            Debug.Log("God Mode is ON! No damage taken.");
            return; // Player is invincible
        }

        // Check if a Book instance exists in the scene
        if (GameObject.FindWithTag("Book") == null) // Check if a book exists
        {
            Vector3 spawnPosition = Player.transform.position;
            TypingText.gameObject.SetActive(false); //Hide the Typer when the book is dropped
            bookDropTime = Time.time;
            spawnedBook = Instantiate(Book, spawnPosition, Quaternion.identity);
            BookMovement bookScript = spawnedBook.GetComponent<BookMovement>();

            if (bookScript != null)
            {
                bookScript.StartBookMovement(spawnPosition, GetRandomPositionAroundPlayer());
            }
        }
        else
        {
            // If the Book already exists, deal damage to the playerQF
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

        StartCoroutine(EnableBookColliderAfterDelay(3f));
    }

    private IEnumerator TemporaryInvulnerability(float duration)
    {
        if(Player != null)
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
            }

            // Ensure the player is visible and re-enable the collider
            spriteRenderer.enabled = true;
            Player.GetComponent<Collider2D>().enabled = true;
        }
    }

    private IEnumerator EnableBookColliderAfterDelay(float delay)
    {
        if (Book != null)
        {
            Book.GetComponent<Collider2D>().enabled = false; 
            yield return new WaitForSeconds(delay); 
            Book.GetComponent<Collider2D>().enabled = true; 
        }
    }


    private Vector3 GetRandomPositionAroundPlayer()
    {
        Vector3 spawnPosition;
        int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minRadius * 1.5f, maxRadius * 1.5f);
            Vector2 randomOffset = randomDirection * randomDistance;

            spawnPosition = new Vector3(Player.transform.position.x + randomOffset.x,
                                        Player.transform.position.y + randomOffset.y,
                                        1);

            if (IsPositionValid(spawnPosition, minDistanceFromPlayer, safeDistanceFromBoss))
            {
                return spawnPosition;
            }
        }

        return GetFallbackSpawnPosition();
    }

    private bool IsPositionValid(Vector3 position, float minPlayerDist, float minBossDist)
    {
        float mapMinX = -15.32f, mapMaxX = 14.68f;
        float mapMinY = -14.4f, mapMaxY = 9.6f;

        if (position.x < mapMinX || position.x > mapMaxX || position.y < mapMinY || position.y > mapMaxY)
        {
            return false;
        }

        if (Vector3.Distance(position, Player.transform.position) < minPlayerDist)
        {
            return false;
        }

        if (Vector3.Distance(position, Boss.transform.position) < minBossDist)
        {
            return false;
        }

        return true;
    }

    private Vector3 GetFallbackSpawnPosition()
    {
        float bookMargin = 3f;
        float mapMinX = -15.32f + bookMargin, mapMaxX = 14.68f - bookMargin;
        float mapMinY = -14.4f + bookMargin, mapMaxY = 9.6f - bookMargin;

        for (int i = 0; i < 10; i++) 
        {
            float randomX = Random.Range(mapMinX, mapMaxX);
            float randomY = Random.Range(mapMinY, mapMaxY);
            Vector3 fallbackPosition = new Vector3(randomX, randomY, 1);

            if (IsPositionValid(fallbackPosition, 3f, 3f))
            {
                return fallbackPosition;
            }
        }

        return new Vector3(0, 0, 1); 
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
