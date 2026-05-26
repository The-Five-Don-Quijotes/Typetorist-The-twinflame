using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
    public float minRadius;
    public float maxRadius;
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

    public bool isGodMode = false;
    private bool canBeDamaged = true;

    private void Awake()
    {
        if (playerStats != null)
        {
            Destroy(playerStats.gameObject);
        }

        playerStats = this;
        DontDestroyOnLoad(gameObject);

        Player = GameObject.FindWithTag("Player");
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        health = maxHealth;
        DisplayHeart();
        if (mapCollider != null)
        {
            mapBounds = mapCollider.bounds;
        }
        sceneTransition = FindFirstObjectByType<SceneTransition>();

        isGodMode = false;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(AssignTyper(scene.name));
    }

    private IEnumerator AssignTyper(string sceneName)
    {
        if (sceneName == "Scene2")
        {
            while (typer == null)
            {
                typer = FindFirstObjectByType<ZhavokTyper>();
                if (typer != null) break;
                yield return null;
            }
        }
        else
        {
            while (typer == null)
            {
                typer = FindFirstObjectByType<BaelorisTyper>();
                if (typer != null) break;
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
        if (!canBeDamaged) return;
        if (Boss != null && Boss.GetComponent<EnemyReceiveDamage>().health <= 0) return;

        StartCoroutine(DamageCooldown(0.5f));

        if (AudioManager.instance != null && AudioManager.instance.damagedClip != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.damagedClip);
        }

        if (GameObject.FindWithTag("Book") == null)
        {
            Vector3 spawnPosition = Player.transform.position + new Vector3(0, 1f, 1f);
            TypingText.gameObject.SetActive(false);
            bookDropTime = Time.time;
            spawnedBook = Instantiate(Book, spawnPosition, Quaternion.identity);
            Collider2D bookCollider = spawnedBook.GetComponent<Collider2D>();

            if (bookCollider != null)
            {
                bookCollider.enabled = false;
                StartCoroutine(EnableBookColliderAfterDelay(bookCollider, 2.9f));
            }

            BookMovement bookScript = spawnedBook.GetComponent<BookMovement>();

            if (bookScript != null)
            {
                // Pass the wall layer mask to the spawned book so it can avoid walls on landing
                bookScript.wallLayerMask = wallLayerMask;
                bookScript.StartBookMovement(GetRandomPositionAroundPlayer());
            }
        }
        else
        {
            if (isGodMode) return;

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

    private IEnumerator DamageCooldown(float duration)
    {
        canBeDamaged = false;
        yield return new WaitForSeconds(duration);
        canBeDamaged = true;
    }

    public void HealCharacter(int heal)
    {
        health += heal;
        CheckOverheal();
        DisplayHeart();
    }

    private void CheckOverheal()
    {
        if (health > maxHealth) health = maxHealth;
    }

    private void CheckDeath()
    {
        if (health <= 0)
        {
            if (health < 0) health = 0;

            if (AudioManager.instance != null && AudioManager.instance.dieClip != null)
            {
                AudioManager.instance.PlaySFX(AudioManager.instance.dieClip);
            }

            if (TypingLine != null) Destroy(TypingLine.gameObject);
            Destroy(Player);
            sceneTransition.LoadSceneWithFade("DedScreen");
        }
    }

    private void Respawn()
    {
        Player.gameObject.SetActive(true);
        StartCoroutine(TemporaryInvulnerability(5f));
        StartCoroutine(EnableBookColliderAfterDelay(3f));
    }

    private IEnumerator TemporaryInvulnerability(float duration)
    {
        if (Player != null)
        {
            PlayerMovement playerMovement = Player.GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.isInvincible = true;

            Collider2D[] colliders = Player.GetComponents<Collider2D>();
            foreach (Collider2D col in colliders) col.enabled = false;

            SpriteRenderer spriteRenderer = Player.GetComponent<SpriteRenderer>();
            float elapsedTime = 0f;
            bool isVisible = true;
            float blinkInterval = 0.2f;

            while (spriteRenderer != null && elapsedTime < duration)
            {
                isVisible = !isVisible;
                spriteRenderer.enabled = isVisible;
                yield return new WaitForSeconds(blinkInterval);
                elapsedTime += blinkInterval;
            }

            if (spriteRenderer == null) yield break;

            spriteRenderer.enabled = true;
            foreach (Collider2D col in colliders) col.enabled = true;
            if (playerMovement != null) playerMovement.isInvincible = false;
        }
    }

    private IEnumerator EnableBookColliderAfterDelay(float delay)
    {
        if (Book != null)
        {
            Collider2D bookCollider = Book.GetComponent<Collider2D>();
            if (bookCollider != null)
            {
                bookCollider.enabled = false;
                yield return new WaitForSeconds(delay);
                bookCollider.enabled = true;
            }
        }
    }

    private IEnumerator EnableBookColliderAfterDelay(Collider2D bookCollider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bookCollider != null) bookCollider.enabled = true;
    }

    private Vector3 GetRandomPositionAroundPlayer()
    {
        Vector3 spawnPosition;
        int maxAttempts = 20; // Increased attempts to find a valid position

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minRadius * 1.5f, maxRadius * 1.5f);
            Vector2 randomOffset = randomDirection * randomDistance;

            spawnPosition = new Vector3(
                Player.transform.position.x + randomOffset.x,
                Player.transform.position.y + randomOffset.y,
                0);

            // Check both position validity AND that the path from player to book is clear
            if (IsPositionValid(spawnPosition, minDistanceFromPlayer, safeDistanceFromBoss)
                && IsPathClear(Player.transform.position, spawnPosition))
            {
                return spawnPosition;
            }
        }

        return GetFallbackSpawnPosition();
    }

    private bool IsPositionValid(Vector3 position, float minPlayerDist, float minBossDist)
    {
        if (!mapBounds.Contains(position)) return false;

        if (Player != null && Vector3.Distance(position, Player.transform.position) < minPlayerDist)
            return false;

        if (Boss != null && Vector3.Distance(position, Boss.transform.position) < minBossDist)
            return false;

        // Check if the landing spot itself overlaps a wall
        Collider2D hit = Physics2D.OverlapCircle(position, 1f, wallLayerMask);
        if (hit != null) return false;

        return true;
    }

    // Raycast from player to target to make sure no wall is blocking the path
    private bool IsPathClear(Vector3 from, Vector3 to)
    {
        Vector2 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);
        RaycastHit2D hit = Physics2D.Raycast(from, direction, distance, wallLayerMask);
        return hit.collider == null;
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
                return fallbackPosition;
        }

        return new Vector3(0, 0, 1);
    }

    public void DisplayHeart()
    {
        foreach (Image img in hearts) img.sprite = emptyHeart;
        for (int i = 0; i < health; i++) hearts[i].sprite = fullHeart;
    }

    public void ShowTyper()
    {
        TypingText.gameObject.SetActive(true);
        if (TypingText.color.a == 0)
            TypingText.GetComponent<MakeTextAppear>()?.ShowText(0f);
    }

    private void Update()
    {
        DebugInput();
        if (typer == null) return;

        if (GameObject.FindWithTag("Book") != null
            && (bookDropTime > 0 && Time.time - bookDropTime > TimeToRecollect)
            && Boss != null)
        {
            typer.ResetLine();
            bookDropTime = -1f;
        }
    }
}