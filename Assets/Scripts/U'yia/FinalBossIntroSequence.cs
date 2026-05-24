using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FinalBossIntroSequence : MonoBehaviour
{
    [Header("UI & Combat Control")]
    public GameObject worldGUI;
    public GameObject typingText;
    public BaelorisTyper combatTyperScript;

    [Header("Characters & Positions")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour mainCameraScript;
    public MonoBehaviour uiiaMovementScript;
    public Transform playerTransform;

    [Tooltip("The NPC version of Uiiia before transformation (UiiiaPrev)")]
    public GameObject uiiiaPrevObject;

    [Tooltip("The actual Boss GameObject (Uiiiiiiia)")]
    public GameObject realBossObject;

    [Tooltip("The center point where UiiiaPrev flies to, and where the Real Boss spawns")]
    public Transform bossSpawnLocation;

    [Header("Audio Settings")]
    [Tooltip("The AudioSource responsible for playing background music.")]
    public AudioSource bgmAudioSource;
    [Tooltip("The music track that plays during the intro dialogue sequence.")]
    public AudioClip introMusic;
    [Tooltip("The music track that plays when the boss battle begins.")]
    public AudioClip battleMusic;
    [Tooltip("The track that plays when the boss begins charging.")]
    public AudioClip chargeSound;
    [Tooltip("The track that plays when the boss break.")]
    public AudioClip shatterSound;

    [Header("Dialogues - Phase 1 (Before Break)")]
    public Dialogue narratorIntro;
    public Dialogue playerReply;
    public Dialogue narratorSecondLine;
    public Dialogue playerSecondReply;
    public Dialogue narratorScream;

    [Header("Dialogues - Phase 2 (After Break)")]
    public Dialogue bossPostBreakWords;
    public Dialogue playerResponseWords;
    public Dialogue bossPreCombatWords;

    [Header("VFX & Props")]
    public float shakeDuration = 1.0f;
    public float shakeMagnitude = 0.5f;
    public GameObject arcaneCirclePrefab;
    private GameObject spawnedArcaneCircle;

    [Tooltip("Prefab for the glowing book that appears when typing 'resist'")]
    public GameObject glowingBookPrefab;
    private GameObject spawnedGlowingBook;

    [Header("Fragment Visuals")]
    public GameObject[] fragmentPrefabs = new GameObject[3];
    public float transferDuration = 1.0f;
    public float circleRadius = 2f;
    public float circleSpeed = 150f;

    [Header("Typing Interrupt Phase")]
    public string breakWord = "resist";
    public TextMeshProUGUI wordOutput;
    private int currentIndex = 0;

    [Header("Events")]
    public UnityEvent OnCombatBegin;

    private GameObject[] activeFragments = new GameObject[3];
    private bool isTypingPhase = false;
    private bool isCircling = false;
    private bool sequenceBroken = false;
    private Vector3 originalWordPos;

    // Dedicated AudioSource for the looping charge sound
    private AudioSource chargeAudioSource;

    public void StartIntroSequence()
    {
        if (worldGUI != null) worldGUI.SetActive(false);
        if (typingText != null) typingText.SetActive(false);
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (uiiaMovementScript != null) uiiaMovementScript.enabled = false;
        if (wordOutput != null) wordOutput.gameObject.SetActive(false);

        // Ensure the NPC actor is visible at the start of the cutscene
        if (uiiiaPrevObject != null) uiiiaPrevObject.SetActive(true);

        // Ensure real boss is hidden initially until the swap
        if (realBossObject != null) realBossObject.SetActive(false);

        if (combatTyperScript != null) combatTyperScript.enabled = false;

        // Dynamic Audio Source Linking
        if (bgmAudioSource == null)
        {
            GameObject audioManagerObj = GameObject.Find("AudioManager");
            if (audioManagerObj != null)
            {
                bgmAudioSource = audioManagerObj.GetComponentInChildren<AudioSource>();
            }
            else
            {
                Debug.LogWarning("FinalBossIntroSequence: Could not find 'AudioManager' in the scene!");
            }
        }

        // Initialize and play the intro music track
        if (bgmAudioSource != null && introMusic != null)
        {
            bgmAudioSource.clip = introMusic;
            bgmAudioSource.Play();
        }

        StartCoroutine(IntroCoroutine());
    }

    private IEnumerator PlayDialogueSequence(Dialogue dialogue)
    {
        if (dialogue == null || dialogue.sentences == null || dialogue.sentences.Length == 0) yield break;

        bool dialogueFinished = false;
        System.Action onDialogueEnd = () => dialogueFinished = true;

        DialogueManager.instance.OnDialogueEnded += onDialogueEnd;
        DialogueManager.instance.StartDialogue(dialogue);

        yield return new WaitUntil(() => dialogueFinished);

        DialogueManager.instance.OnDialogueEnded -= onDialogueEnd;
    }

    private IEnumerator IntroCoroutine()
    {
        // 1. UiiiaPrev yells at player 
        yield return StartCoroutine(PlayDialogueSequence(narratorIntro));

        // 2. Player talks back
        yield return StartCoroutine(PlayDialogueSequence(playerReply));

        // 3. UiiiaPrev second warning 
        yield return StartCoroutine(PlayDialogueSequence(narratorSecondLine));

        // 4. Player accuses UiiiaPrev 
        yield return StartCoroutine(PlayDialogueSequence(playerSecondReply));

        // 5. UiiiaPrev loses temper: Screen Shake & Scream 
        StartCoroutine(ScreenShake(shakeDuration, shakeMagnitude));
        yield return StartCoroutine(PlayDialogueSequence(narratorScream));

        // 6. Pan Camera to the Spawn Location
        if (mainCameraScript != null && bossSpawnLocation != null)
        {
            mainCameraScript.SendMessage("SetTarget", bossSpawnLocation, SendMessageOptions.DontRequireReceiver);
        }

        // 7. UiiiaPrev flies to the center spawn location
        if (uiiiaPrevObject != null && bossSpawnLocation != null)
        {
            yield return StartCoroutine(MoveEntityToPosition(uiiiaPrevObject.transform, bossSpawnLocation.position, 1.5f));
        }

        // 8. Summon Arcane Circle under UiiiaPrev
        if (arcaneCirclePrefab != null && bossSpawnLocation != null)
        {
            Vector3 circlePos = new Vector3(bossSpawnLocation.position.x, bossSpawnLocation.position.y, 1f);
            spawnedArcaneCircle = Instantiate(arcaneCirclePrefab, circlePos, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
        }

        // 9. UiiiaPrev takes fragments from player
        yield return StartCoroutine(TransferFragmentsToBoss(uiiiaPrevObject.transform));
        isCircling = true;

        // 10. Typing Interrupt Phase Initialization
        isTypingPhase = true;
        sequenceBroken = false;
        currentIndex = 0;

        // Spawn the glowing book on the player
        if (glowingBookPrefab != null && playerTransform != null)
        {
            spawnedGlowingBook = Instantiate(glowingBookPrefab, playerTransform.position, Quaternion.identity);
        }

        // Setup the typing UI under the boss location so the camera sees it
        if (wordOutput != null && bossSpawnLocation != null)
        {
            originalWordPos = wordOutput.transform.position;
            wordOutput.transform.position = bossSpawnLocation.position + new Vector3(0, -1.5f, 0);
            wordOutput.text = breakWord;
            wordOutput.gameObject.SetActive(true);
        }

        // --- Start looping Charge Sound ---
        if (chargeSound != null)
        {
            if (chargeAudioSource == null)
            {
                chargeAudioSource = gameObject.AddComponent<AudioSource>();
            }
            chargeAudioSource.clip = chargeSound;
            chargeAudioSource.loop = true;
            // Sync volume with AudioManager settings
            chargeAudioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
            chargeAudioSource.Play();
        }

        // Wait indefinitely until the player successfully types the word
        yield return new WaitUntil(() => sequenceBroken);

        // 11. Sequence Broken - Interrupt successful

        // --- Stop Charge Sound & Trigger Shatter Sound ---
        if (chargeAudioSource != null && chargeAudioSource.isPlaying)
        {
            chargeAudioSource.Stop();
        }

        if (AudioManager.instance != null && shatterSound != null)
        {
            AudioManager.instance.PlaySFX(shatterSound);
        }

        isTypingPhase = false;
        isCircling = false;

        if (wordOutput != null)
        {
            wordOutput.gameObject.SetActive(false);
            wordOutput.transform.position = originalWordPos;
        }

        ClearFragments();
        if (spawnedArcaneCircle != null) Destroy(spawnedArcaneCircle);
        if (spawnedGlowingBook != null) Destroy(spawnedGlowingBook);

        yield return new WaitForSeconds(0.5f);

        // 12. THE SWAP: Disable NPC, Enable Real Boss
        if (uiiiaPrevObject != null) uiiiaPrevObject.SetActive(false);
        if (realBossObject != null)
        {
            realBossObject.transform.position = bossSpawnLocation.position;
            realBossObject.SetActive(true);
        }

        yield return new WaitForSeconds(1f);

        // 13. Real Boss talks after interruption
        yield return StartCoroutine(PlayDialogueSequence(bossPostBreakWords));

        // 14. Player replies
        yield return StartCoroutine(PlayDialogueSequence(playerResponseWords));

        // 15. Real Boss final words before combat
        yield return StartCoroutine(PlayDialogueSequence(bossPreCombatWords));

        // 16. Restore States and Begin Combat
        if (mainCameraScript != null)
        {
            mainCameraScript.SendMessage("SetTarget", playerTransform, SendMessageOptions.DontRequireReceiver);
        }

        if (worldGUI != null) worldGUI.SetActive(true);
        if (typingText != null) typingText.SetActive(true);
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        if (combatTyperScript != null)
        {
            combatTyperScript.enabled = true;
            combatTyperScript.ResetLine();
        }

        // Switch to battle music track exactly when combat logic resumes
        if (bgmAudioSource != null && battleMusic != null)
        {
            bgmAudioSource.clip = battleMusic;
            bgmAudioSource.Play();
        }

        OnCombatBegin?.Invoke();
    }

    private IEnumerator MoveEntityToPosition(Transform entity, Vector3 targetPos, float duration)
    {
        Vector3 startPos = entity.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            entity.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        entity.position = targetPos;
    }

    private void Update()
    {
        if (isTypingPhase) CheckTypingInput();

        if (isCircling && uiiiaPrevObject != null) AnimateFragments(uiiiaPrevObject.transform);
    }

    private void CheckTypingInput()
    {
        if (!Input.anyKeyDown) return;

        string inputString = Input.inputString;
        foreach (char c in inputString)
        {
            if (c == '\b' || c == '\n' || c == '\r') continue;

            string typedLetter = c.ToString().ToLower();
            string targetLetter = breakWord[currentIndex].ToString().ToLower();

            if (typedLetter == targetLetter)
            {
                currentIndex++;
                UpdateRemainingWord();

                if (currentIndex == breakWord.Length)
                {
                    sequenceBroken = true;
                    return;
                }
            }
            else if (currentIndex > 0)
            {
                currentIndex--;
                UpdateRemainingWord();
            }
        }
    }

    private void UpdateRemainingWord()
    {
        if (wordOutput != null)
        {
            wordOutput.text = breakWord.Substring(currentIndex);
        }
    }

    private IEnumerator TransferFragmentsToBoss(Transform targetEntity)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < fragmentPrefabs.Length && fragmentPrefabs[i] != null)
            {
                activeFragments[i] = Instantiate(fragmentPrefabs[i], playerTransform.position, Quaternion.identity);
            }
        }

        float elapsed = 0f;
        while (elapsed < transferDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transferDuration;

            for (int i = 0; i < 3; i++)
            {
                if (activeFragments[i] != null)
                {
                    activeFragments[i].transform.position = Vector3.Lerp(playerTransform.position, targetEntity.position, t);
                }
            }
            yield return null;
        }
    }

    private void AnimateFragments(Transform centerEntity)
    {
        if (centerEntity == null) return;

        float time = Time.time * circleSpeed * Mathf.Deg2Rad;
        float angleStep = (360f / 3) * Mathf.Deg2Rad;

        for (int i = 0; i < 3; i++)
        {
            if (activeFragments[i] != null)
            {
                float currentAngle = time + (angleStep * i);
                Vector3 offset = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0) * circleRadius;
                activeFragments[i].transform.position = centerEntity.position + offset;
            }
        }
    }

    private void ClearFragments()
    {
        for (int i = 0; i < 3; i++)
        {
            if (activeFragments[i] != null)
            {
                Destroy(activeFragments[i]);
            }
        }
    }

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        Transform camTransform = Camera.main.transform;
        Vector3 originalPos = camTransform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            camTransform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = originalPos;
    }
}