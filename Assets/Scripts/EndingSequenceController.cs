using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Required for TextMeshPro

public class EndingSequenceController : MonoBehaviour
{
    [Header("UI & Cinematic Effects")]
    [Tooltip("A black UI Image stretched across a Canvas to act as a fade screen.")]
    public Image fadeScreen;
    public float fadeDuration = 2.0f;

    [Header("Good Ending UI")]
    [Tooltip("The TextMeshProUGUI object displaying the 'GOOD ENDING' text.")]
    public TextMeshProUGUI goodEndingText;
    public float textFadeDuration = 1.5f;
    public float textDisplayDuration = 3.0f;

    [Header("Camera & Transforms")]
    public Camera mainCamera;

    // Reference to your camera follow script to prevent conflicts
    public CameraFollowPlayer cameraFollowScript;
    public MonoBehaviour playerMovementScript;

    [Tooltip("The physical book GameObject to start the zoom out from.")]
    public Transform deskBookTransform;
    public Transform playerTransform;
    public Transform npcTransform;

    [Header("Camera Settings")]
    public float zoomedInSize = 2f;
    public float normalCameraSize = 5f;
    public float cameraTransitionSpeed = 2.5f;

    [Header("Dialogues")]
    public Dialogue narratorDialogue;
    public Dialogue npcDialogue;

    [Header("Transition")]
    [Tooltip("The exact name of the scene you want to load after the screen fades to black.")]
    public string creditSceneName = "Credits";

    private Vector3 defaultCameraPosition;

    private void Start()
    {
        // 1. Initial State Lock
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        // Disable Camera Follow to allow cinematic panning
        if (cameraFollowScript != null) cameraFollowScript.enabled = false;

        // Ensure the Good Ending text is hidden at the start
        if (goodEndingText != null)
        {
            Color c = goodEndingText.color;
            c.a = 0f;
            goodEndingText.color = c;
            goodEndingText.gameObject.SetActive(false);
        }

        defaultCameraPosition = mainCamera.transform.position;

        StartCoroutine(ExecuteEndingSequence());
    }

    private IEnumerator ExecuteEndingSequence()
    {
        // PHASE 1: Start at the Book & Fade In
        mainCamera.orthographicSize = zoomedInSize;
        Vector3 bookCamPos = deskBookTransform.position;
        bookCamPos.z = defaultCameraPosition.z; // Maintain camera depth
        mainCamera.transform.position = bookCamPos;

        yield return StartCoroutine(FadeFromBlack());
        yield return new WaitForSeconds(0.5f);

        // PHASE 2: Zoom out to frame both the Player and NPC
        Vector3 midPoint = (playerTransform.position + npcTransform.position) / 2f;
        midPoint.z = defaultCameraPosition.z;

        yield return StartCoroutine(MoveCameraSmoothly(midPoint, normalCameraSize, cameraTransitionSpeed));

        // PHASE 3: Narrator Dialogue
        yield return StartCoroutine(PlayDialogue(narratorDialogue));

        yield return new WaitForSeconds(1f);

        // PHASE 4: NPC Final Dialogue
        yield return StartCoroutine(PlayDialogue(npcDialogue));

        // PHASE 5: Fade to Black
        yield return StartCoroutine(FadeToBlack());

        // PHASE 6: Display "GOOD ENDING" Text
        if (goodEndingText != null)
        {
            goodEndingText.gameObject.SetActive(true);
            yield return StartCoroutine(FadeTextAlpha(goodEndingText, 0f, 1f, textFadeDuration));
            yield return new WaitForSeconds(textDisplayDuration);
            yield return StartCoroutine(FadeTextAlpha(goodEndingText, 1f, 0f, textFadeDuration));
        }

        // PHASE 7: Transition to Credits
        if (!string.IsNullOrEmpty(creditSceneName))
        {
            SceneManager.LoadScene(creditSceneName);
        }
        else
        {
            Debug.LogWarning("Credit Scene Name is missing. Please assign it in the Inspector.");
        }
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeScreen == null) yield break;

        fadeScreen.color = new Color(0, 0, 0, 1);
        fadeScreen.gameObject.SetActive(true);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeScreen.gameObject.SetActive(false);
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeScreen == null) yield break;

        fadeScreen.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeTextAlpha(TextMeshProUGUI textElement, float startAlpha, float endAlpha, float duration)
    {
        if (textElement == null) yield break;

        Color c = textElement.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            textElement.color = c;
            yield return null;
        }

        c.a = endAlpha;
        textElement.color = c;
    }

    private IEnumerator MoveCameraSmoothly(Vector3 targetPos, float targetSize, float duration)
    {
        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / duration);
            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.orthographicSize = targetSize;
    }

    private IEnumerator PlayDialogue(Dialogue dialogue)
    {
        if (dialogue == null || dialogue.sentences == null || dialogue.sentences.Length == 0) yield break;

        bool dialogueFinished = false;
        System.Action onDialogueEnd = () => dialogueFinished = true;

        DialogueManager.instance.OnDialogueEnded += onDialogueEnd;
        DialogueManager.instance.StartDialogue(dialogue);

        yield return new WaitUntil(() => dialogueFinished);

        DialogueManager.instance.OnDialogueEnded -= onDialogueEnd;
    }
}