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

            // Execute the visual cooldown effect on the newly spawned book
            StartCoroutine(VisualBookCooldown(spawnedBook, 2.9f));

            BookMovement bookScript = spawnedBook.GetComponent<BookMovement>();

            if (bookScript != null)
            {
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

        // Find the active book in the scene to apply the cooldown effect
        GameObject activeBook = GameObject.FindWithTag("Book");
        if (activeBook != null)
        {
            StartCoroutine(VisualBookCooldown(activeBook, 3f));
        }
    }

    private IEnumerator TemporaryInvulnerability(float duration)
    {
        if (Player == null) yield break;

        PlayerMovement playerMovement = Player.GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.isInvincible = true;

        int playerLayer = Player.layer;
        int enemyLayer = LayerMask.NameToLayer("Default");
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        SpriteRenderer spriteRenderer = Player.GetComponent<SpriteRenderer>();
        float elapsedTime = 0f;
        float blinkInterval = 0.2f;

        while (spriteRenderer != null && elapsedTime < duration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        if (playerMovement != null) playerMovement.isInvincible = false;
    }

    // Consolidated method handling both visual feedback and collider state
    private IEnumerator VisualBookCooldown(GameObject bookObj, float delay)
    {
        if (bookObj == null) yield break;

        Collider2D bookCollider = bookObj.GetComponent<Collider2D>();
        SpriteRenderer bookSprite = bookObj.GetComponent<SpriteRenderer>();

        if (bookCollider != null) bookCollider.enabled = false;

        if (bookSprite != null)
        {
            float elapsedTime = 0f;
            float blinkInterval = 0.15f;
            bool isDimmed = true;

            // Blinking effect loop
            while (elapsedTime < delay)
            {
                if (bookObj == null) yield break;

                Color currentColor = bookSprite.color;
                currentColor.a = isDimmed ? 0.3f : 0.8f;
                bookSprite.color = currentColor;

                isDimmed = !isDimmed;
                yield return new WaitForSeconds(blinkInterval);
                elapsedTime += blinkInterval;
            }

            // Restore solid color when ready
            if (bookObj != null)
            {
                Color finalColor = bookSprite.color;
                finalColor.a = 1f;
                bookSprite.color = finalColor;
            }
        }
        else
        {
            // Fallback if the object lacks a SpriteRenderer
            yield return new WaitForSeconds(delay);
        }

        if (bookCollider != null && bookObj != null) bookCollider.enabled = true;
    }

    private Vector3 GetRandomPositionAroundPlayer()
    {
        Vector3 spawnPosition;
        int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minRadius * 1.5f, maxRadius * 1.5f);
            Vector2 randomOffset = randomDirection * randomDistance;

            spawnPosition = new Vector3(
                Player.transform.position.x + randomOffset.x,
                Player.transform.position.y + randomOffset.y,
                0);

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

        Collider2D hit = Physics2D.OverlapCircle(position, 1f, wallLayerMask);
        if (hit != null) return false;

        return true;
    }

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
        if (typer == null) return;

        if (GameObject.FindWithTag("Book") != null
            && (bookDropTime > 0 && Time.time - bookDropTime > TimeToRecollect)
            && Boss != null)
        {
            typer.ResetLine();
            bookDropTime = -1f;
        }
    }

    // Public method to trigger I-frames from external sources like the Ghost Hand
    public void TriggerExternalInvincibility(float duration)
    {
        StartCoroutine(TemporaryInvulnerability(duration));
    }
}