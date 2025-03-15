using System.Collections;
using UnityEngine;

public class BookMovement : MonoBehaviour
{
    public float moveSpeed = 3f;   // Speed towards target
    public float jumpHeight = 3f;  // Height of the jump
    public float jumpDelay = 0.5f; // Delay before the second jump

    private Vector3 finalPosition;
    private bool canBeCollected = false; // Prevents instant recollection

    public void StartBookMovement(Vector3 targetPosition)
    {
        finalPosition = targetPosition;
        canBeCollected = false; // Prevent immediate collection
        StartCoroutine(DoubleJumpRoutine());
    }

    private IEnumerator DoubleJumpRoutine()
    {
        Vector3 startPosition = transform.position;

        // Calculate midpoint EXACTLY between start and final position
        Vector3 midPoint = (startPosition + finalPosition) / 2;
        midPoint.y += jumpHeight; // Raise midpoint to create a jump effect

        // First jump to the midpoint
        yield return JumpToPosition(midPoint);

        yield return new WaitForSeconds(jumpDelay);

        // Second jump to the final position
        yield return JumpToPosition(finalPosition);

        // Allow collection after the second jump
        yield return new WaitForSeconds(0.3f);
        canBeCollected = true;
    }

    private IEnumerator JumpToPosition(Vector3 target)
    {
        float duration = 0.5f; // Jump speed
        float elapsedTime = 0f;
        Vector3 start = transform.position;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smooth curve
            transform.position = Vector3.Lerp(start, target, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }

    public bool CanBeCollected()
    {
        return canBeCollected;
    }
}
