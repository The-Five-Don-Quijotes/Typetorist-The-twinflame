using System.Collections;
using TMPro;
using UnityEngine;

public class SummonBehavior : MonoBehaviour
{
    private TextMeshProUGUI typingText; // Reference to TypingText

    private void Start()
    {
        GameObject textObject = GameObject.Find("TypingText");
        if (textObject != null)
        {
            typingText = textObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogWarning("TypingText object not found in the scene.");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetTextAlpha(0f); // Make text fully transparent
            Destroy(gameObject);
        }
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    private void SetTextAlpha(float alpha)
    {
        if (typingText != null)
        {
            Color color = typingText.color;
            color.a = alpha;
            typingText.color = color;
        }
        else
        {
            Debug.LogWarning("TypingText reference is null. Make sure the object exists.");
        }
    }
}
