using System.Collections;
using UnityEngine;

public class BookMovement : MonoBehaviour
{
    public float moveSpeed = 5f;   // Speed towards target
    public float jumpHeight = 3f;  // Jump force
    public float jumpDelay = 0.5f; // Time before second jump

    private Vector3 finalPosition;

    public void StartBookMovement(Vector3 targetPosition)
    {
        finalPosition = targetPosition;
        StartCoroutine(DoubleJumpRoutine());
    }

    private IEnumerator DoubleJumpRoutine()
    {

        Vector3 halfwayPoint = new Vector3(finalPosition.x, (finalPosition.y + transform.position.y) / 2, finalPosition.z);
        yield return MoveToPosition(halfwayPoint, moveSpeed);

        yield return JumpToPosition(new Vector3(halfwayPoint.x, halfwayPoint.y - jumpHeight, halfwayPoint.z));

        yield return new WaitForSeconds(jumpDelay);

        yield return JumpToPosition(finalPosition);
    }

    private IEnumerator MoveToPosition(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator JumpToPosition(Vector3 target)
    {
        float duration = 0.5f; // Adjust jump speed
        float elapsedTime = 0f;
        Vector3 start = transform.position;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smooth curve for better jumping feel
            transform.position = Vector3.Lerp(start, target, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }
}
