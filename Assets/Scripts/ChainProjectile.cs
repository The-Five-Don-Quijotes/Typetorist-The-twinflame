using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ChainProjectile : MonoBehaviour
{
    [Header("Configuration")]
    public float extensionSpeed = 15f;
    [Tooltip("How far from the EXACT center of the target the chain should stop.")]
    public float stopRadius = 0.5f;

    [Tooltip("Keep true if the Sprite Pivot in the Import Settings is set to 'Center'.")]
    public bool isPivotCenter = true;

    [Header("Visuals")]
    public Color chainColor = Color.white;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = chainColor;
    }

    public void SetChainVisuals(Color visuals)
    {
        chainColor = visuals;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = visuals;
        }
    }

    public void FireChainAtTarget(Vector3 startPoint, Transform exactTarget)
    {
        if (exactTarget == null) return;
        StartCoroutine(AnimateChainToTarget(startPoint, exactTarget));
    }

    private IEnumerator AnimateChainToTarget(Vector3 start3D, Transform targetTransform)
    {
        // 1. FORCE 2D MATH: Convert 3D positions to pure 2D to ignore Z-axis depth differences
        Vector2 start2D = new Vector2(start3D.x, start3D.y);
        Vector2 targetCenter2D = new Vector2(targetTransform.position.x, targetTransform.position.y);

        Vector2 direction2D = (targetCenter2D - start2D).normalized;

        // 2. Calculate pure 2D visual distance
        float flatDistance = Vector2.Distance(start2D, targetCenter2D) - stopRadius;
        if (flatDistance < 0) flatDistance = 0f;

        // Compensate for Prefab Scale
        float localFinalSize = flatDistance / Mathf.Max(0.01f, transform.localScale.y);

        // Orient the chain sprite
        float angle = Mathf.Atan2(direction2D.y, direction2D.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector2 currentSize = spriteRenderer.size;
        currentSize.y = 0f;
        spriteRenderer.size = currentSize;

        while (currentSize.y < localFinalSize)
        {
            currentSize.y += (extensionSpeed / Mathf.Max(0.01f, transform.localScale.y)) * Time.deltaTime;

            if (currentSize.y >= localFinalSize)
            {
                currentSize.y = localFinalSize;
            }

            spriteRenderer.size = currentSize;

            if (isPivotCenter)
            {
                float currentWorldLength = currentSize.y * transform.localScale.y;
                Vector2 newPos2D = start2D + direction2D * (currentWorldLength / 2f);

                // Apply the new 2D position, but preserve the original Z depth so it renders correctly
                transform.position = new Vector3(newPos2D.x, newPos2D.y, start3D.z);
            }
            else
            {
                transform.position = start3D;
            }

            if (currentSize.y >= localFinalSize) yield break;

            yield return null;
        }
    }
}