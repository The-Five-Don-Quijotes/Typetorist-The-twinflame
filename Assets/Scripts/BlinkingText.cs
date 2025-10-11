using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class BlinkingText : MonoBehaviour
{
    [Tooltip("How fast the text will blink in and out.")]
    public float blinkSpeed = 1.5f;

    [Tooltip("The dimmest the text will get (0 = fully invisible).")]
    public float minAlpha = 0.2f;

    [Tooltip("The brightest the text will get (1 = fully visible).")]
    public float maxAlpha = 1.0f;

    private TextMeshProUGUI textComponent;

    void Awake()
    {
        // Get the TextMeshPro component attached to this GameObject.
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // Use a sine wave to create a smooth, looping value between -1 and 1.
        float wave = Mathf.Sin(Time.time * blinkSpeed);

        // Remap the sine wave from the -1 to 1 range to our minAlpha to maxAlpha range.
        float targetAlpha = Mathf.Lerp(minAlpha, maxAlpha, (wave + 1) / 2f);

        // Get the current color of the text.
        Color currentColor = textComponent.color;

        // Set the alpha of the color to our new target alpha.
        currentColor.a = targetAlpha;

        // Apply the new color back to the text component.
        textComponent.color = currentColor;
    }
}