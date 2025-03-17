using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonBehavior : MonoBehaviour
{
    private TextMeshProUGUI typingText; // Reference to TypingText
    private TextMeshProUGUI typingLine; // Reference to TypingLine
    private CanvasGroup bossHealthCanvas;
    private CanvasGroup playerHealthCanvas;
    private Animator animator;
    public float disappearDuration = 3f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        GameObject textObject = GameObject.Find("TypingText");
        GameObject textObject2 = GameObject.Find("TypingLine");
        GameObject healthBar = GameObject.Find("BossHealthBar");
        GameObject playerHealth = GameObject.Find("HeartsContainer");
        if (textObject != null)
        {
            typingText = textObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogWarning("TypingText object not found in the scene.");
        }

        if (textObject2 != null)
        {
            typingLine = textObject2.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogWarning("TypingLine object not found in the scene.");
        }

        if (healthBar != null)
        {
            bossHealthCanvas = healthBar.GetComponent<CanvasGroup>();
        }
        else
        {
            Debug.LogWarning("Boss health bar object not found in the scene.");
        }

        if (playerHealth != null)
        {
            playerHealthCanvas = playerHealth.GetComponent<CanvasGroup>();
        }
        else
        {
            Debug.LogWarning("Player health object not found in the scene.");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetTextAlpha(0f); // Make text fully transparent
            animator.SetTrigger("isDeath");
            Destroy(gameObject);
        }
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    private void SetTextAlpha(float alpha)
    {
        if (typingText != null && typingText.color.a != 0)
        {
            Color color = typingText.color;
            color.a = alpha;
            typingText.color = color;
            typingText.GetComponent<MakeTextAppear>().ShowText(disappearDuration);
        } else if (typingLine != null && typingLine.color.a != 0)
        {
            Color color = typingLine.color;
            color.a = alpha;
            typingLine.color = color;
            typingLine.GetComponent<MakeTextAppear>().ShowText(disappearDuration);
        } else if (playerHealthCanvas != null && playerHealthCanvas.alpha != 0) 
        {
            playerHealthCanvas.alpha = alpha;
            playerHealthCanvas.GetComponent<MakeCanvasAppear>().ShowCanvas(disappearDuration);
        } else if (bossHealthCanvas != null)
        {
            bossHealthCanvas.alpha = alpha;
            bossHealthCanvas.GetComponent<MakeCanvasAppear>().ShowCanvas(disappearDuration);
        }
        else
        {
            Debug.LogWarning("TypingText reference is null. Make sure the object exists.");
        }
    }
}
