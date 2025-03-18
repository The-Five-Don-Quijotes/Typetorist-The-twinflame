using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using static UnityEngine.GraphicsBuffer;
using System.ComponentModel;
using Unity.VisualScripting;

public class ZhavokSummonMovement : MonoBehaviour
{
    private Transform player;
    private Transform targetBook;
    private GameObject boss;
    private bool isInteractingWithBook = false;

    [Header("Movement Settings")]
    public float moveSpeed = 4f; // Speed at which summon moves
    public float stoppingDistance = 0.5f; // Minimum distance before stopping
    public float bookDetectionRange = 5f; // How far the summon can see a book
    public float bookMoveRadius = 2f; // How far the book is moved when picked
    private Tilemap tilemap; // Reference to the tilemap

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        boss = GameObject.Find("Zhavok");
        tilemap = GameObject.Find("Wall")?.GetComponent<Tilemap>();
    }

    void Update()
    {
        if (player == null || isInteractingWithBook) return;

        if (targetBook == null)
        {
            FindBook(); // Look for books if not currently targeting one
        }

        if (targetBook != null)
        {
            StartCoroutine(InteractWithBook());
        }
        else
        {
            MoveTowards(player.position, false); // Default: Follow the player
        }

        if (boss != null)
        {
            float bossHealth = boss.GetComponent<EnemyReceiveDamage>().health;
            float bossMaxHealth = boss.GetComponent<EnemyReceiveDamage>().maxHealth;
            if (gameObject.GetComponent<SummonBehavior>() == null)
                Debug.LogError("SummonBehavior is missing!");

            if (gameObject.GetComponent<SummonPhase2Behavior>() == null)
                Debug.LogError("SummonPhase2Behavior is missing!");
            if (bossHealth > bossMaxHealth / 4) //50% boss
            {
                gameObject.GetComponent<SummonPhase2Behavior>().enabled = false;
                gameObject.GetComponent<SummonBehavior>().enabled = true;
            }
            else //25% boss
            {
                gameObject.GetComponent<SummonBehavior>().enabled = false;
                gameObject.GetComponent<SummonPhase2Behavior>().enabled = true;
            }
        }
    }

    private void FindBook()
    {
        GameObject[] books = GameObject.FindGameObjectsWithTag("Book");

        foreach (GameObject book in books)
        {
            if (!book.GetComponent<BookRecollect>().isTaken && Vector2.Distance(transform.position, book.transform.position) <= bookDetectionRange)
            {
                if (Random.value < 0.001f) // 0.1% chance to target this book
                {
                    book.GetComponent<BookRecollect>().isTaken = true;
                    targetBook = book.transform;
                    break;
                }
            }
        }
    }

    private IEnumerator InteractWithBook()
    {
        if (targetBook != null)
        {
            isInteractingWithBook = true;

            // Move towards the book
            while (targetBook != null && Vector2.Distance(transform.position, targetBook.position) > 0.1f)
            {
                MoveTowards(targetBook.position, true);
                yield return null; // Wait for next frame
            }

            if (targetBook == null)
            {
                isInteractingWithBook = false;
            }

            // Move book to a random nearby position
            if (targetBook != null)
            {
                // Try to find a valid position for the book
                Vector2 newPos = FindValidBookPosition();
                if (newPos != Vector2.zero)
                {
                    while (Vector2.Distance(targetBook.position, newPos) > 0.1f)
                    {
                        MoveTowards(newPos, true);
                        targetBook.position = Vector2.MoveTowards(targetBook.position, newPos, moveSpeed * Time.deltaTime);
                        yield return null;
                    }
                }
                else
                {
                    Debug.LogWarning("No valid position found for book relocation.");
                }

                // Reset book target and resume normal behavior
                if (targetBook != null)
                {
                    targetBook.GetComponent<BookRecollect>().isTaken = false;
                    targetBook = null;
                }
                isInteractingWithBook = false;
            }
        }
    }

    private void MoveTowards(Vector2 targetPosition, bool isBook)
    {
        if (isBook)
        {
            if (Vector2.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (Vector2.Distance(transform.position, targetPosition) > stoppingDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
    }

    private Vector2 FindValidBookPosition()
    {
        int maxAttempts = 10; // Avoid infinite loops
        while (maxAttempts > 0)
        {
            maxAttempts--;

            // Generate a random offset within bookMoveRadius
            Vector2 randomOffset = Random.insideUnitCircle * bookMoveRadius;
            Vector3Int targetCell = tilemap.WorldToCell((Vector2)targetBook.position + randomOffset);

            // Ensure target cell is inside tilemap bounds
            targetCell.x = Mathf.Clamp(targetCell.x, tilemap.cellBounds.xMin, tilemap.cellBounds.xMax - 1);
            targetCell.y = Mathf.Clamp(targetCell.y, tilemap.cellBounds.yMin, tilemap.cellBounds.yMax - 1);

            // Check if the tile is empty AND within bounds
            if (IsValidTile(targetCell))
            { 
                return tilemap.GetCellCenterWorld(targetCell);
            }
        }

        return targetBook.position; // Default to the book's original position if no valid position is found
    }

    // Ensure the tile is within valid bounds and walkable
    private bool IsValidTile(Vector3Int cell)
    {
        return tilemap != null && tilemap.cellBounds.Contains(cell) && !tilemap.HasTile(cell);
    }
}
