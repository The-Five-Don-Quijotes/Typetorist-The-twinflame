using System.Collections;
using UnityEngine;

public class BookMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpHeight = 3f;
    public float jumpDelay = 0.5f;

    // Set at runtime by PlayerStats when the book is spawned — no need to assign in Inspector
    [HideInInspector] public LayerMask wallLayerMask;
    public float bookRadius = 0.3f; // Overlap check radius, adjust to match book sprite size

    private Vector3 finalPosition;
    private bool canBeCollected = false;

    public void StartBookMovement(Vector3 targetPosition)
    {
        finalPosition = targetPosition;
        canBeCollected = false;
        StartCoroutine(DoubleJumpRoutine());
    }

    private IEnumerator DoubleJumpRoutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 midPoint = (startPosition + finalPosition) / 2;

        // First jump to midpoint
        yield return JumpToPosition(midPoint);
        yield return new WaitForSeconds(jumpDelay);

        // Second jump to final position
        yield return JumpToPosition(finalPosition);

        // After landing, push the book out if it ended up inside a wall or collider
        SnapToSafePosition();

        yield return new WaitForSeconds(0.3f);
        canBeCollected = true;
    }

    private IEnumerator JumpToPosition(Vector3 target)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3 start = transform.position;

        while (elapsedTime < duration)
        {
            float t = Mathf.Clamp01(elapsedTime / duration);
            Vector3 currentPos = Vector3.Lerp(start, target, t);

            // Parabolic arc
            float height = 4f;
            currentPos.y = Mathf.Lerp(start.y, target.y, t) + height * Mathf.Sin(Mathf.PI * t);

            transform.position = currentPos;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }

    // If the book landed inside a wall, search outward in all directions for the nearest open spot
    private void SnapToSafePosition()
    {
        // Already in a safe spot, nothing to do
        if (Physics2D.OverlapCircle(transform.position, bookRadius, wallLayerMask) == null)
            return;

        float[] radii = { 1f, 1.5f, 2f, 2.5f, 3f };
        int angleSteps = 16;

        foreach (float radius in radii)
        {
            for (int i = 0; i < angleSteps; i++)
            {
                float angle = i * (360f / angleSteps);
                Vector2 dir = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad));

                Vector3 candidate = transform.position + (Vector3)(dir * radius);

                // Found a clear spot, move the book there
                if (Physics2D.OverlapCircle(candidate, bookRadius, wallLayerMask) == null)
                {
                    transform.position = candidate;
                    return;
                }
            }
        }

        // Last resort: place the book right next to the player
        if (PlayerStats.playerStats?.Player != null)
        {
            transform.position = PlayerStats.playerStats.Player.transform.position
                                  + new Vector3(2f, 0f, 0f);
        }
    }

    public bool CanBeCollected()
    {
        return canBeCollected;
    }
}