using System.Collections;
using TMPro;
using UnityEngine;

public class MakeTextAppear : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        if (textMeshPro == null)
        {
            Debug.LogWarning("TextMeshProUGUI component not found!");
        }
    }

    public void ShowText(float delay)
    {
        StartCoroutine(AppearAfterDelay(delay));
    }

    private IEnumerator AppearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (textMeshPro != null)
        {
            Color color = textMeshPro.color;
            color.a = 1f; // Fully visible
            textMeshPro.color = color;
        }
    }
}
