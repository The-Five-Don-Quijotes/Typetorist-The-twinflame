using UnityEngine;
using System.Collections;

public class ZhavokSummonMovement : MonoBehaviour
{
    private Transform player;
    private Transform targetBook;
    private bool isInteractingWithBook = false;

    [Header("Movement Settings")]
    public float moveSpeed = 4f; // Speed at which summon moves
    public float stoppingDistance = 0.5f; // Minimum distance before stopping
    public float bookDetectionRange = 5f; // How far the summon can see a book
    public float bookMoveRadius = 2f; // How far the book is moved when picked

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
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
            MoveTowards(player.position); // Default: Follow the player
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
        if(targetBook != null)
        {
            isInteractingWithBook = true;

            // Move towards the book
            while (targetBook != null && Vector2.Distance(transform.position, targetBook.position) > stoppingDistance)
            {
                MoveTowards(targetBook.position);
                yield return null; // Wait for next frame
            }

            if(targetBook == null)
            {
                isInteractingWithBook = false;
            }

            // Move book to a random nearby position
            if(targetBook != null)
            {
                Vector2 randomOffset = Random.insideUnitCircle * bookMoveRadius;
                Vector2 newPos = (Vector2)transform.position + randomOffset;
                while (targetBook != null && Vector2.Distance(targetBook.position, newPos) > 0)
                {
                    MoveTowards(newPos);
                    targetBook.position = Vector2.MoveTowards(targetBook.position, newPos, moveSpeed * Time.deltaTime);
                    yield return null;
                }

                // Reset book target and resume normal behavior
                targetBook.GetComponent<BookRecollect>().isTaken = false;
                targetBook = null;
                isInteractingWithBook = false;
            }
        }
    }

    private void MoveTowards(Vector2 targetPosition)
    {
        if (Vector2.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
}
