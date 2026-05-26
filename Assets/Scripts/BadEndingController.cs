using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BadEndingController : MonoBehaviour
{
    [Header("Core References")]
    public Camera mainCamera;
    public CameraFollowPlayer cameraFollowScript;
    public Transform playerTransform;
    public Transform uiiiaTransform;
    [Tooltip("The portal GameObject they emerge from.")]
    public GameObject portal;

    [Header("The Betrayal (Crack VFX)")]
    public GameObject floorCrackPrefab;
    public float shakeDuration = 1.5f;
    public float shakeMagnitude = 0.4f;

    [Header("Dialogues")]
    public Dialogue preFallDialogue;
    public float delayAfterDialogue = 1.0f;

    [Header("Bad Ending UI")]
    public Image fadeScreen;
    public Image catSmileImage;
    public TextMeshProUGUI badEndingText;

    [Header("Timings")]
    public float fallDuration = 0.8f;
    public float jumpscareDisplayTime = 3.0f;
    public string creditSceneName = "Credits";

    [Header("Audio (Optional)")]
    public AudioClip earthquakeSound;
    public AudioClip jumpscareSound;

    private Vector3 defaultCameraPosition;

    private void Start()
    {
        if (catSmileImage != null) catSmileImage.gameObject.SetActive(false);
        if (badEndingText != null) badEndingText.gameObject.SetActive(false);

        if (fadeScreen != null)
        {
            fadeScreen.gameObject.SetActive(true);
            fadeScreen.color = new Color(0, 0, 0, 0);
        }

        defaultCameraPosition = mainCamera.transform.position;

        StartCoroutine(ExecuteBadEndingSequence());
    }

    private IEnumerator ExecuteBadEndingSequence()
    {
        yield return new WaitForSeconds(1.5f);

        if (portal != null) portal.SetActive(false);

        yield return StartCoroutine(PlayDialogue(preFallDialogue));
        yield return new WaitForSeconds(delayAfterDialogue);

        // PHASE 3: The Earthquake & The Crack
        if (AudioManager.instance != null && earthquakeSound != null)
        {
            AudioManager.instance.PlaySFX(earthquakeSound);
        }

        Coroutine shakeRoutine = StartCoroutine(ScreenShake(shakeDuration, shakeMagnitude));

        if (floorCrackPrefab != null && playerTransform != null)
        {
            Vector3 spawnPos = new Vector3(playerTransform.position.x, playerTransform.position.y, 0f);
            GameObject crackInstance = Instantiate(floorCrackPrefab, spawnPos, Quaternion.identity);
            crackInstance.SetActive(true);

            SpriteRenderer[] renderers = crackInstance.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in renderers)
            {
                sr.sortingOrder = 11;
            }
        }

        yield return new WaitForSeconds(shakeDuration * 0.5f);

        // PHASE 4: The Fall
        StartCoroutine(SimulateFalling(playerTransform));
        StartCoroutine(SimulateFalling(uiiiaTransform));

        yield return shakeRoutine;

        yield return new WaitForSeconds(1.0f);

        // PHASE 5: Fade to Black
        yield return StartCoroutine(FadeToBlack(1.0f));

        // PHASE 6: The Jumpscare / Cat Smile
        if (AudioManager.instance != null && jumpscareSound != null)
        {
            AudioManager.instance.PlaySFX(jumpscareSound);
        }

        if (catSmileImage != null)
        {
            catSmileImage.gameObject.SetActive(true);
            yield return StartCoroutine(FadeUIAlpha(catSmileImage, 0f, 1f, 0.2f));
        }

        yield return new WaitForSeconds(1.0f);

        if (badEndingText != null)
        {
            badEndingText.gameObject.SetActive(true);
            yield return StartCoroutine(FadeTextAlpha(badEndingText, 0f, 1f, 1.0f));
        }

        yield return new WaitForSeconds(jumpscareDisplayTime);

        yield return StartCoroutine(FadeUIAlpha(catSmileImage, 1f, 0f, 1.5f));
        if (badEndingText != null) yield return StartCoroutine(FadeTextAlpha(badEndingText, 1f, 0f, 1.5f));

        // PHASE 7: Transition to Credits
        if (!string.IsNullOrEmpty(creditSceneName))
        {
            SceneManager.LoadScene(creditSceneName);
        }
    }

    private IEnumerator SimulateFalling(Transform target)
    {
        if (target == null) yield break;

        Vector3 startScale = target.localScale;
        Vector3 endScale = Vector3.zero;

        Vector3 startPos = target.position;
        Vector3 endPos = startPos + new Vector3(0, -0.5f, 0);

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;

            target.Rotate(0, 0, 1000 * Time.deltaTime);

            target.localScale = Vector3.Lerp(startScale, endScale, t);
            target.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        target.gameObject.SetActive(false);
    }

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
    }

    private IEnumerator FadeToBlack(float duration)
    {
        if (fadeScreen == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeUIAlpha(Graphic uiElement, float startAlpha, float endAlpha, float duration)
    {
        if (uiElement == null) yield break;

        Color c = uiElement.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            uiElement.color = c;
            yield return null;
        }
        c.a = endAlpha;
        uiElement.color = c;
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