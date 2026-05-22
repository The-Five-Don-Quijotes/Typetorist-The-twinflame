using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSequenceController : MonoBehaviour
{
    [Header("UI & Screen Fade")]
    [Tooltip("A black UI Image stretched across a Canvas to act as a fade screen.")]
    public Image fadeScreen;
    [Tooltip("The UI container holding the 3 Health Hearts.")]
    public GameObject healthUIContainer;
    public GameObject dashUI;

    [Header("Camera & Transforms")]
    public Camera mainCamera;

    // Reference to your camera follow script to prevent conflicts
    public CameraFollowPlayer cameraFollowScript;

    public MonoBehaviour playerMovementScript;
    public Transform playerTransform;
    [Tooltip("The physical book GameObject sitting on the desk.")]
    public Transform deskBookTransform;
    [Tooltip("The exact center point where the portal is located.")]
    public Transform portalSpawnLocation;

    [Header("Camera Settings")]
    public float zoomedInSize = 2f;
    public float normalCameraSize = 5f;
    public float cameraTransitionSpeed = 2f;

    [Header("Typing Tutorial Setup")]
    public IntroTyper typerScript;
    public BaelorisWordBank wordBankScript;
    public GameObject typingUIContainer;
    public GameObject typingLineUIContainer;
    public List<string> tutorialSentences;

    [Header("VFX & Entities")]
    public float shakeDuration = 1.5f;
    public float shakeMagnitude = 0.3f;
    [Tooltip("The existing Portal GameObject in the scene.")]
    public GameObject existingScenePortal;
    public GameObject magicCirclePrefab;

    [Header("Audio")]
    [Tooltip("The SFX played when the portal and magic circle appear.")]
    public AudioClip portalSpawnSound; // --- NEW: Audio reference for portal spawn ---

    [Header("Dialogues")]
    public Dialogue narratorDialogue;
    public Dialogue npcDialogue;
    public Dialogue bookExplanationDialogue;

    private Vector3 defaultCameraPosition;

    private void Start()
    {
        // 1. Initial State Lock
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (typerScript != null) typerScript.enabled = false;
        if (typingUIContainer != null) typingUIContainer.SetActive(false);
        if (typingLineUIContainer != null) typingLineUIContainer.SetActive(false);
        if (healthUIContainer != null) healthUIContainer.SetActive(false);
        if (dashUI != null) dashUI.SetActive(false);

        // Disable Camera Follow to allow cinematic panning
        if (cameraFollowScript != null) cameraFollowScript.enabled = false;

        // Ensure the scene portal is hidden until Phase 4
        if (existingScenePortal != null) existingScenePortal.SetActive(false);

        defaultCameraPosition = mainCamera.transform.position;

        StartCoroutine(ExecuteTutorialSequence());
    }

    private IEnumerator ExecuteTutorialSequence()
    {
        // PHASE 1: Fade In & Zoom Out from the Book
        mainCamera.orthographicSize = zoomedInSize;
        Vector3 bookCamPos = deskBookTransform.position;
        bookCamPos.z = defaultCameraPosition.z;
        mainCamera.transform.position = bookCamPos;

        yield return StartCoroutine(FadeFromBlack());
        yield return new WaitForSeconds(0.5f);

        // Zoom out to normal player view
        yield return StartCoroutine(MoveCameraSmoothly(defaultCameraPosition, normalCameraSize, cameraTransitionSpeed));

        // PHASE 2: Narrator Dialogue
        yield return StartCoroutine(PlayDialogue(narratorDialogue));

        // PHASE 3: Typing Tutorial
        if (wordBankScript != null && tutorialSentences.Count > 0)
        {
            wordBankScript.SetNewLines(tutorialSentences);
        }

        if (typingUIContainer != null) typingUIContainer.SetActive(true);
        if (typingLineUIContainer != null) typingLineUIContainer.SetActive(true);
        if (typerScript != null) typerScript.enabled = true;

        // Wait until the player finishes all sentences
        yield return new WaitUntil(() => typerScript.IsTypingSequenceComplete);

        if (typingUIContainer != null) typingUIContainer.SetActive(false);
        if (typingLineUIContainer != null) typingLineUIContainer.SetActive(false);
        if (typerScript != null) typerScript.enabled = false;

        // PHASE 4: Earthquake & Portal Activation
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(ScreenShake(shakeDuration, shakeMagnitude));

        Vector3 portalCamPos = portalSpawnLocation.position;
        portalCamPos.z = defaultCameraPosition.z;

        // --- NEW: Execute portal spawn SFX via AudioManager ---
        if (AudioManager.instance != null && portalSpawnSound != null)
        {
            AudioManager.instance.PlaySFX(portalSpawnSound);
        }

        // Zoom into portal location
        yield return StartCoroutine(MoveCameraSmoothly(portalCamPos, zoomedInSize + 1f, cameraTransitionSpeed * 1.5f));

        Vector3 spawnPos2D = new Vector3(portalSpawnLocation.position.x, portalSpawnLocation.position.y, 0f);

        // Activate existing scene portal and its collider
        if (existingScenePortal != null)
        {
            existingScenePortal.SetActive(true);

            CircleCollider2D portalCollider = existingScenePortal.GetComponent<CircleCollider2D>();
            if (portalCollider != null)
            {
                portalCollider.enabled = true;
            }

            SpriteRenderer[] renderers = existingScenePortal.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in renderers)
            {
                sr.sortingOrder = 11;
            }
        }

        // Spawn magic circle if prefab exists
        if (magicCirclePrefab != null)
        {
            GameObject circleObj = Instantiate(magicCirclePrefab, spawnPos2D, Quaternion.identity);
            circleObj.SetActive(true);

            SpriteRenderer[] renderers = circleObj.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in renderers)
            {
                sr.sortingOrder = 10;
            }
        }

        yield return new WaitForSeconds(2.0f);

        // Return camera to player
        yield return StartCoroutine(MoveCameraSmoothly(defaultCameraPosition, normalCameraSize, cameraTransitionSpeed));

        // PHASE 5: NPC Dialogue
        yield return StartCoroutine(PlayDialogue(npcDialogue));

        // PHASE 6: Book Flies to Player & UI Appears
        yield return StartCoroutine(FlyBookToPlayer());

        if (healthUIContainer != null) healthUIContainer.SetActive(true);
        if (dashUI != null) dashUI.SetActive(true);

        // PHASE 7: Book Explanation Dialogue
        yield return StartCoroutine(PlayDialogue(bookExplanationDialogue));

        // PHASE 8: Return Control & Re-enable Camera
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        // Restore Camera Follow System
        if (cameraFollowScript != null)
        {
            cameraFollowScript.SetTarget(playerTransform);
            cameraFollowScript.enabled = true;
        }
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeScreen == null) yield break;

        fadeScreen.color = new Color(0, 0, 0, 1);
        fadeScreen.gameObject.SetActive(true);

        float elapsed = 0f;
        float duration = 2.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeScreen.gameObject.SetActive(false);
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

    private IEnumerator FlyBookToPlayer()
    {
        if (deskBookTransform == null || playerTransform == null) yield break;

        Vector3 startPos = deskBookTransform.position;
        float elapsed = 0f;
        float duration = 1.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            deskBookTransform.position = Vector3.Lerp(startPos, playerTransform.position, elapsed / duration);

            deskBookTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsed / duration);
            yield return null;
        }

        deskBookTransform.gameObject.SetActive(false);
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