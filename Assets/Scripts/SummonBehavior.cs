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

        if (textObject != null) typingText = textObject.GetComponent<TextMeshProUGUI>();
        if (textObject2 != null) typingLine = textObject2.GetComponent<TextMeshProUGUI>();
        if (healthBar != null) bossHealthCanvas = healthBar.GetComponent<CanvasGroup>();
        if (playerHealth != null) playerHealthCanvas = playerHealth.GetComponent<CanvasGroup>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetTextAlpha(0f); // Make text fully transparent
            if (animator != null) animator.SetTrigger("isDeath");
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

            MakeTextAppear script = typingText.GetComponent<MakeTextAppear>();
            if (script != null) script.ShowText(disappearDuration);
        }
        else if (typingLine != null && typingLine.color.a != 0)
        {
            Color color = typingLine.color;
            color.a = alpha;
            typingLine.color = color;

            MakeTextAppear script = typingLine.GetComponent<MakeTextAppear>();
            if (script != null) script.ShowText(disappearDuration);
        }
        else if (playerHealthCanvas != null && playerHealthCanvas.alpha != 0)
        {
            playerHealthCanvas.alpha = alpha;

            // Safe check to prevent crash if MakeCanvasAppear is missing
            MakeCanvasAppear script = playerHealthCanvas.GetComponent<MakeCanvasAppear>();
            if (script != null) script.ShowCanvas(disappearDuration);
        }
        else if (bossHealthCanvas != null && bossHealthCanvas.alpha != 0)
        {
            bossHealthCanvas.alpha = alpha;

            // Safe check to prevent crash if MakeCanvasAppear is missing
            MakeCanvasAppear script = bossHealthCanvas.GetComponent<MakeCanvasAppear>();
            if (script != null) script.ShowCanvas(disappearDuration);
        }
    }
}