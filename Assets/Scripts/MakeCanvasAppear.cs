using System.Collections;
using UnityEngine;

public class MakeCanvasAppear : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup not found! Adding one automatically.");
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void ShowCanvas(float delay)
    {
        StartCoroutine(AppearAfterDelay(delay));
    }

    private IEnumerator AppearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canvasGroup.alpha = 1f; // Make it visible
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
}
