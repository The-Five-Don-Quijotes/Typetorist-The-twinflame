using System.Collections;
using UnityEngine;

public class BookMovement : MonoBehaviour
{
    public float moveSpeed = 3f;   
    public float jumpHeight = 3f;  
    public float jumpDuration = 0f;
    public float jumpDelay = 0.5f; 

    private Vector3 spawnPostion;
    private Vector3 finalPosition;
    private bool canBeCollected = false; 

    public void StartBookMovement(Vector3 spawnedPosition, Vector3 targetPosition)
    {
        spawnPostion = spawnedPosition;
        finalPosition = targetPosition;
        canBeCollected = false; 
        StartCoroutine(DoubleJumpRoutine());
    }

    private IEnumerator DoubleJumpRoutine()
    {
        Vector3 startPosition = spawnPostion;

        Vector3 midPoint = (startPosition + finalPosition) / 2;
        midPoint.y += jumpHeight; 

        yield return SmoothJump(startPosition, midPoint, jumpDuration);

        yield return new WaitForSeconds(jumpDelay);

        yield return SmoothJump(midPoint, finalPosition, jumpDuration);

        yield return new WaitForSeconds(0.3f);
        canBeCollected = true;
    }

    private IEnumerator SmoothJump(Vector3 start, Vector3 target, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

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
