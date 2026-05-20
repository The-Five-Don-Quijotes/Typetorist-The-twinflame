using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FinalBossIntroSequence : MonoBehaviour
{
    [Header("UI & Combat Control")]
    public GameObject worldGUI;
    public GameObject typingText;
    public BaelorisTyper combatTyperScript; // --- NEW: Reference to your combat typer ---

    [Header("Characters & Camera")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour mainCameraScript;
    public Transform playerTransform;
    public Transform bossTransform;

    [Header("Dialogues - Phase 1 (Before Break)")]
    public Dialogue narratorIntro;
    public Dialogue playerReply;
    public Dialogue narratorScream;

    [Header("Dialogues - Phase 2 (After Break)")]
    public Dialogue bossPostBreakWords;
    public Dialogue playerResponseWords;
    public Dialogue bossPreCombatWords;

    [Header("Screen Shake Settings")]
    public float shakeDuration = 1.0f;
    public float shakeMagnitude = 0.5f;

    [Header("Arcane Circle Setup")]
    public GameObject arcaneCirclePrefab;
    private GameObject spawnedArcaneCircle;

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

    public void StartIntroSequence()
    {
        if (worldGUI != null) worldGUI.SetActive(false);
        if (typingText != null) typingText.SetActive(false);
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (wordOutput != null) wordOutput.gameObject.SetActive(false);

        // Disable combat typing to prevent input interference during cutscene
        if (combatTyperScript != null) combatTyperScript.enabled = false;

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
        // 1. Narrator speaks first
        yield return StartCoroutine(PlayDialogueSequence(narratorIntro));

        // 2. Player talks back
        yield return StartCoroutine(PlayDialogueSequence(playerReply));

        // 3. Narrator loses temper: Start Screen Shake AND Scream Dialogue simultaneously
        StartCoroutine(ScreenShake(shakeDuration, shakeMagnitude));
        yield return StartCoroutine(PlayDialogueSequence(narratorScream));

        // 4. Pan Camera to Boss
        if (mainCameraScript != null)
        {
            mainCameraScript.SendMessage("SetTarget", bossTransform, SendMessageOptions.DontRequireReceiver);
        }
        yield return new WaitForSeconds(1f);

        // 5. Summon Arcane Circle behind the boss
        if (arcaneCirclePrefab != null && bossTransform != null)
        {
            Vector3 circlePos = new Vector3(bossTransform.position.x, bossTransform.position.y, 1f);
            spawnedArcaneCircle = Instantiate(arcaneCirclePrefab, circlePos, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
        }

        // 6. Boss takes fragments from player
        yield return StartCoroutine(TransferFragmentsToBoss());
        isCircling = true;

        // 7. Typing Interrupt Phase Initialization
        isTypingPhase = true;
        sequenceBroken = false;
        currentIndex = 0;

        if (wordOutput != null)
        {
            originalWordPos = wordOutput.transform.position;
            wordOutput.transform.position = bossTransform.position + new Vector3(0, -1.5f, 0);
            wordOutput.text = breakWord;
            wordOutput.gameObject.SetActive(true);
        }

        // Wait indefinitely until the player successfully types the word
        yield return new WaitUntil(() => sequenceBroken);

        // 8. Sequence Broken
        isTypingPhase = false;
        isCircling = false;

        if (wordOutput != null)
        {
            wordOutput.gameObject.SetActive(false);
            wordOutput.transform.position = originalWordPos;
        }

        ClearFragments();
        if (spawnedArcaneCircle != null) Destroy(spawnedArcaneCircle);

        yield return new WaitForSeconds(1f);

        // 9. Boss talks after fragments shatter
        yield return StartCoroutine(PlayDialogueSequence(bossPostBreakWords));

        // 10. Player replies
        yield return StartCoroutine(PlayDialogueSequence(playerResponseWords));

        // 11. Boss talks final time before combat
        yield return StartCoroutine(PlayDialogueSequence(bossPreCombatWords));

        // 12. Restore States and Begin Combat
        if (mainCameraScript != null)
        {
            mainCameraScript.SendMessage("SetTarget", playerTransform, SendMessageOptions.DontRequireReceiver);
        }

        if (worldGUI != null) worldGUI.SetActive(true);
        if (typingText != null) typingText.SetActive(true);
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        // --- NEW: Reactivate and force UI refresh for combat typing ---
        if (combatTyperScript != null)
        {
            combatTyperScript.enabled = true;
            combatTyperScript.ResetLine();
        }

        OnCombatBegin?.Invoke();
    }

    private void Update()
    {
        if (isTypingPhase) CheckTypingInput();
        if (isCircling) AnimateFragments();
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

    private IEnumerator TransferFragmentsToBoss()
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
                    activeFragments[i].transform.position = Vector3.Lerp(playerTransform.position, bossTransform.position, t);
                }
            }
            yield return null;
        }
    }

    private void AnimateFragments()
    {
        if (bossTransform == null) return;

        float time = Time.time * circleSpeed * Mathf.Deg2Rad;
        float angleStep = (360f / 3) * Mathf.Deg2Rad;

        for (int i = 0; i < 3; i++)
        {
            if (activeFragments[i] != null)
            {
                float currentAngle = time + (angleStep * i);
                Vector3 offset = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0) * circleRadius;
                activeFragments[i].transform.position = bossTransform.position + offset;
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