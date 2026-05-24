using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class FinalSealingCutscene : MonoBehaviour
{
    [Header("Core References")]
    public Transform playerTransform;
    public Transform bossTransform;
    public Camera mainCamera;
    public MonoBehaviour mainCameraScript;
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour bossMovementScript;
    public MonoBehaviour baelorisTyperScript;

    [Header("UI Control")]
    public GameObject worldGUI;
    public TextMeshProUGUI typingOutputText;
    public TextMeshProUGUI sentenceOutputText;

    [Header("Book & Scene Transition")]
    public SpriteRenderer bookSpriteRenderer;
    public Animator bookAnimator;
    [Tooltip("The exact name of the scene you want to load after the book closes.")]
    public string nextSceneName = "MainMenu";
    [Tooltip("How long to wait for the BookClose animation to finish before loading the scene.")]
    public float sceneTransitionDelay = 2.0f;
    [Tooltip("Extra camera zoom padding so the book isn't touching the edge of the screen.")]
    public float cameraPadding = 1.0f;

    [Header("Chain Configurations")]
    public GameObject chainPrefab;
    public Transform[] cornerSpawns = new Transform[4];

    [Header("Visual Distinction (Set Styles in Inspector)")]
    public Color playerChainStyle = Color.white;
    public Color bossChainStyle = Color.red;

    [Header("Audio Settings")]
    public AudioClip chainSound;
    [Tooltip("The duration (in seconds) it takes for the chains to reach their target. The sound will loop for this duration and then stop.")]
    public float chainSoundDuration = 0.5f;

    [Header("Incantations")]
    public string[] incantations = new string[4];

    [Header("Dialogues")]
    public Dialogue bookIntroDialogue;
    public Dialogue bossBeforeIncan2Dialogue;
    public Dialogue bossBeforeIncan3Dialogue;
    public Dialogue playerBeforeIncan3Dialogue;
    public Dialogue finalBeggingDialogueBoss;
    public Dialogue finalBeggingDialoguePlayer;

    // State Management
    private int currentSentenceIndex = 0;
    private string[] currentWords;
    private int currentWordIndex = 0;

    private string currentWordTarget = "";
    private int currentCharIndex = 0;

    private bool isTypingActive = false;
    private bool pendingFinalWordInteraction = false;

    // Audio State
    private AudioSource chainAudioSource;

    public void StartSealingSequence()
    {
        if (worldGUI != null) worldGUI.SetActive(false);

        // Ensure the book starts at -20 in the background
        if (bookSpriteRenderer != null) bookSpriteRenderer.sortingOrder = -20;

        LockEntities();
        StartCoroutine(FrameBothEntities());

        currentSentenceIndex = 0;
        StartCoroutine(StartNextSentenceCoroutine());
    }

    private void LockEntities()
    {
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (baelorisTyperScript != null) baelorisTyperScript.enabled = false;

        if (bossMovementScript != null)
        {
            bossMovementScript.StopAllCoroutines();
            bossMovementScript.enabled = false;
        }

        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.bodyType = RigidbodyType2D.Kinematic;
        }

        Rigidbody2D bossRb = bossTransform.GetComponent<Rigidbody2D>();
        if (bossRb != null)
        {
            bossRb.linearVelocity = Vector2.zero;
            bossRb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D playerCol = playerTransform.GetComponent<Collider2D>();
        if (playerCol != null) playerCol.enabled = true;

        if (mainCameraScript != null) mainCameraScript.enabled = false;
    }

    private IEnumerator FrameBothEntities()
    {
        Vector3 midPoint = (playerTransform.position + bossTransform.position) / 2f;
        midPoint.z = mainCamera.transform.position.z;

        float distance = Vector3.Distance(playerTransform.position, bossTransform.position);
        float targetOrthographicSize = Mathf.Clamp(distance * 0.6f + 2f, 5f, 15f);

        float transitionDuration = 1.5f;
        float elapsed = 0f;

        Vector3 startCamPos = mainCamera.transform.position;
        float startOrthographicSize = mainCamera.orthographicSize;

        while (elapsed < transitionDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(startCamPos, midPoint, elapsed / transitionDuration);
            mainCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, targetOrthographicSize, elapsed / transitionDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = midPoint;
        mainCamera.orthographicSize = targetOrthographicSize;
    }

    private IEnumerator StartNextSentenceCoroutine()
    {
        if (currentSentenceIndex >= 4) yield break;

        if (currentSentenceIndex == 0)
        {
            yield return StartCoroutine(PlayDialogue(bookIntroDialogue));
        }
        else if (currentSentenceIndex == 1)
        {
            yield return StartCoroutine(PlayDialogue(bossBeforeIncan2Dialogue));
        }
        else if (currentSentenceIndex == 2)
        {
            yield return StartCoroutine(PlayDialogue(bossBeforeIncan3Dialogue));
            yield return StartCoroutine(PlayDialogue(playerBeforeIncan3Dialogue));
        }

        string fullSentence = incantations[currentSentenceIndex].Trim();
        if (sentenceOutputText != null) sentenceOutputText.text = fullSentence;

        currentWords = fullSentence.Split(' ');
        currentWordIndex = 0;

        if (currentSentenceIndex == 3)
        {
            pendingFinalWordInteraction = true;
        }

        StartNextWord();
    }

    private void StartNextWord()
    {
        if (currentWordIndex >= currentWords.Length)
        {
            isTypingActive = false;
            StartCoroutine(HandleSentenceCompletion());
            return;
        }

        currentWordTarget = currentWords[currentWordIndex].ToLower();
        currentCharIndex = 0;
        UpdateTypingUI();
        isTypingActive = true;
    }

    private void Update()
    {
        if (!isTypingActive) return;
        else if (typingOutputText != null && !typingOutputText.gameObject.activeSelf) typingOutputText.gameObject.SetActive(true);

        ProcessTypingInput();
    }

    private void ProcessTypingInput()
    {
        if (!Input.anyKeyDown) return;

        if (currentCharIndex == 0 && typingOutputText != null && typingOutputText.text == "")
        {
            typingOutputText.text = currentWordTarget;
        }

        string inputString = Input.inputString;
        foreach (char c in inputString)
        {
            if (c == '\b' || c == '\n' || c == '\r' || c == ' ') continue;

            if (pendingFinalWordInteraction && currentWordIndex == currentWords.Length - 1 && currentCharIndex == 0)
            {
                isTypingActive = false;
                pendingFinalWordInteraction = false;
                StartCoroutine(FinalBeggingSequence());
                return;
            }

            string typedChar = c.ToString().ToLower();
            string targetChar = currentWordTarget[currentCharIndex].ToString();

            if (typedChar == targetChar)
            {
                currentCharIndex++;
                UpdateTypingUI();

                if (currentCharIndex >= currentWordTarget.Length)
                {
                    currentWordIndex++;
                    StartNextWord();
                    return;
                }
            }
            else if (currentCharIndex > 0)
            {
                currentCharIndex--;
                UpdateTypingUI();
            }
        }
    }

    private void UpdateTypingUI()
    {
        if (typingOutputText != null)
        {
            typingOutputText.text = currentWordTarget.Substring(currentCharIndex);
        }
    }

    private IEnumerator HandleSentenceCompletion()
    {
        if (typingOutputText != null) typingOutputText.text = "";
        if (sentenceOutputText != null) sentenceOutputText.text = "";

        if (currentSentenceIndex < cornerSpawns.Length)
        {
            FireChainsFromCorner(cornerSpawns[currentSentenceIndex].position);

            // Execute the audio management independently from the sequence delay
            StartCoroutine(ManageChainAudioRoutine());
        }

        // Wait time between completing a sentence and starting the next one
        yield return new WaitForSeconds(1.0f);

        currentSentenceIndex++;

        if (currentSentenceIndex < 4)
        {
            StartCoroutine(StartNextSentenceCoroutine());
        }
        else
        {
            StartCoroutine(ExecuteFinalClosingSequence());
        }
    }

    private IEnumerator ManageChainAudioRoutine()
    {
        if (chainSound == null) yield break;

        if (chainAudioSource == null)
        {
            chainAudioSource = gameObject.AddComponent<AudioSource>();
            chainAudioSource.loop = true;
            chainAudioSource.playOnAwake = false;
        }

        chainAudioSource.Stop();
        chainAudioSource.clip = chainSound;
        chainAudioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        chainAudioSource.time = 0f; 

        chainAudioSource.Play();

        yield return new WaitForSeconds(chainSoundDuration);

        if (chainAudioSource != null && chainAudioSource.isPlaying)
        {
            chainAudioSource.Stop();
        }
    }

    private IEnumerator FinalBeggingSequence()
    {
        yield return StartCoroutine(PlayDialogue(finalBeggingDialogueBoss));
        yield return StartCoroutine(PlayDialogue(finalBeggingDialoguePlayer));

        isTypingActive = true;
    }

    private void FireChainsFromCorner(Vector3 originPoint)
    {
        if (chainPrefab == null) return;

        GameObject chainToPlayer = Instantiate(chainPrefab, originPoint, Quaternion.identity);
        ChainProjectile playerChainScript = chainToPlayer.GetComponent<ChainProjectile>();
        if (playerChainScript != null && playerTransform != null)
        {
            playerChainScript.SetChainVisuals(playerChainStyle);
            playerChainScript.FireChainAtTarget(originPoint, playerTransform);
        }

        GameObject chainToBoss = Instantiate(chainPrefab, originPoint, Quaternion.identity);
        ChainProjectile bossChainScript = chainToBoss.GetComponent<ChainProjectile>();
        if (bossChainScript != null && bossTransform != null)
        {
            bossChainScript.SetChainVisuals(bossChainStyle);
            bossChainScript.FireChainAtTarget(originPoint, bossTransform);
        }
    }

    private IEnumerator ExecuteFinalClosingSequence()
    {
        if (bookSpriteRenderer == null)
        {
            Debug.LogError("Book SpriteRenderer is not assigned!");
            yield break;
        }

        Bounds bookBounds = bookSpriteRenderer.bounds;
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetSizeY = bookBounds.size.y / 2f;
        float targetSizeX = (bookBounds.size.x / 2f) / screenRatio;

        float targetOrthographicSize = Mathf.Max(targetSizeX, targetSizeY) + cameraPadding;

        float zoomDuration = 3.0f;
        float elapsed = 0f;

        float startSize = mainCamera.orthographicSize;
        Vector3 startCamPos = mainCamera.transform.position;

        Vector3 targetCamPos = bookBounds.center;
        targetCamPos.z = startCamPos.z;

        while (elapsed < zoomDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(startCamPos, targetCamPos, elapsed / zoomDuration);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetOrthographicSize, elapsed / zoomDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetCamPos;
        mainCamera.orthographicSize = targetOrthographicSize;

        bookSpriteRenderer.sortingOrder = 1;

        if (bookAnimator != null)
        {
            bookAnimator.Play("BookClose");
        }

        yield return new WaitForSeconds(sceneTransitionDelay);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next Scene Name is empty. Scene transition skipped.");
        }
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