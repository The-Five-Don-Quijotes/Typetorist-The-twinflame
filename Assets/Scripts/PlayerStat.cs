using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using UnityEditor.SearchService;
using Assets.Interface;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats playerStats;
    private SceneTransition sceneTransition;
    public GameObject spawnedBook;
    public GameObject Player;
    public GameObject Boss;

    public GameObject Book;
    public float minRadius ; //The radius which the book is dropped
    public float maxRadius ;
    public TextMeshProUGUI TypingLine;
    public TextMeshProUGUI TypingText;
    private float bookDropTime = -1f;
    public float TimeToRecollect = 3f;
    public float minDistanceFromPlayer = 10f;
    public float safeDistanceFromBoss = 10f;
    public ITyper typer;
    private Vector3 respawnPosition;
    public LayerMask wallLayerMask;
    [SerializeField] private CompositeCollider2D mapCollider;
    private Bounds mapBounds;




    public int health;
    public int maxHealth;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    private AudioSource audioSource;

    public bool isGodMode = false;

    private void Awake()
    {
        if (playerStats != null)
        {
            Destroy(playerStats.gameObject);
        }

        playerStats = this;
        DontDestroyOnLoad(gameObject);

        // Ensure AudioSource is assigned early
        audioSource = GetComponent<AudioSource>();
        Player = GameObject.FindWithTag("Player");
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        health = maxHealth;
        DisplayHeart();
        audioSource = GetComponent<AudioSource>();
        mapBounds = mapCollider.bounds;
        sceneTransition = FindFirstObjectByType<SceneTransition>(); 

        isGodMode = false;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; 
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name);

        StartCoroutine(AssignTyper(scene.name));
    }

    private IEnumerator AssignTyper(string sceneName)
    {
        if (sceneName == "Scene2")
        {
            while (typer == null)
            {
                typer = FindFirstObjectByType<ZhavokTyper>();
                if (typer != null)
                {
                    Debug.Log("Assigned typer to ZhavokTyper");
                    break;
                }
                yield return null; // wait frame
            }
        }
        else
        {
            while (typer == null)
            {
                typer = FindFirstObjectByType<BaelorisTyper>();
                if (typer != null)
                {
                    Debug.Log("Assigned typer to BaelorisTyper");
                    break;
                }
                yield return null;
            }
        }
    }


    private void DebugInput()
    {
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            DealDamage(1);
        }
    }

    public void DealDamage(int damage)
    {
        // Check if a Book instance exists in the scene
        if (GameObject.FindWithTag("Book") == null)
        {
            Vector3 spawnPosition = Player.transform.position + new Vector3(0, 1f, 1f);
            TypingText.gameObject.SetActive(false); // Hide the Typer when the book is dropped
            bookDropTime = Time.time;
            spawnedBook = Instantiate(Book, spawnPosition, Quaternion.identity);
            Collider2D bookCollider = spawnedBook.GetComponent<Collider2D>();
            if (bookCollider != null)
            {
                bookCollider.enabled = false;
                StartCoroutine(EnableBookColliderAfterDelay(bookCollider, 2.9f)); // Delay before it can be recollected
            }

            BookMovement bookScript = spawnedBook.GetComponent<BookMovement>();

            if (bookScript != null)
            {
                bookScript.StartBookMovement(GetRandomPositionAroundPlayer());
            }
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
            sceneTransition.LoadSceneWithFade("DedScreen");
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
            Collider2D[] colliders = Player.GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }
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
            foreach (Collider2D col in colliders)
            {
                col.enabled = true; 
            }
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

    private IEnumerator EnableBookColliderAfterDelay(Collider2D bookCollider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bookCollider != null)
        {
            bookCollider.enabled = true;
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
                                        0);

            if (IsPositionValid(spawnPosition, minDistanceFromPlayer, safeDistanceFromBoss))
            {
                return spawnPosition;
            }
        }

        return GetFallbackSpawnPosition();
    }

    private bool IsPositionValid(Vector3 position, float minPlayerDist, float minBossDist)
    {
        if (!mapBounds.Contains(position))
        {
            return false;
        }

        if (Player != null && Vector3.Distance(position, Player.transform.position) < minPlayerDist)
        {
            return false;
        }

        if (Boss != null && Vector3.Distance(position, Boss.transform.position) < minBossDist)
        {
            return false;
        }
        Debug.Log( wallLayerMask.value);

        Collider2D hit = Physics2D.OverlapCircle(position, 1f, wallLayerMask);
        if (hit != null)
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
        if(TypingText.color.a == 0)
        {
            TypingText.GetComponent<MakeTextAppear>()?.ShowText(0f);
        }
    }

    private void Update()
    {
        DebugInput();
        if (GameObject.FindWithTag("Book") != null && (bookDropTime > 0 && Time.time - bookDropTime > TimeToRecollect))
        {
            typer.ResetLine();
            bookDropTime = -1f; // Reset to avoid continuous resetting
        }
    }
}
