using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BossCutsceneController : MonoBehaviour
{
    [Header("Characters & Camera")]
    public Transform player;
    public GameObject bossObject;
    public Transform bossSpawnPos;
    public CameraFollowPlayer mainCamera;
    public MonoBehaviour playerMovementScript;

    [Header("UI Control")]
    public GameObject worldGUI;
    public GameObject typingText;

    [Header("Dialogues")]
    public Dialogue initialDialogue;
    public Dialogue bossDialogue;

    [Header("Audio")]
    public AudioClip dialogueMusic;

    [Header("Settings")]
    public Vector2 teleportPos;

    [Header("Combat Trigger")]
    public UnityEvent OnCombatStart;

    [Header("Death Sequence")]
    public Dialogue deathDialogue;     
    public UnityEvent OnBossPerish;     
    public Dialogue postDeathDialogue; 

    [Header("Fragment Drop")]
    public FragmentData bossFragment;

    public UnityEvent OnDeathComplete;

    private void Start()
    {
        // Initialize base states
        if (bossObject != null) bossObject.SetActive(false);
        if (worldGUI != null) worldGUI.SetActive(false);
        if (typingText != null) typingText.SetActive(false);
        if (mainCamera != null && player != null) mainCamera.SetTarget(player);

        if (AudioManager.instance != null && dialogueMusic != null)
        {
            AudioManager.instance.PlayMusic(dialogueMusic);
        }
    }

    public void TriggerSequence()
    {
        StartCoroutine(ExecuteCutsceneSequence());
    }

    private IEnumerator ExecuteCutsceneSequence()
    {
        // Lock player movement
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        yield return new WaitForSeconds(0.5f);

        bool dialogueFinished = false;
        System.Action onDialogueEnd = () => dialogueFinished = true;

        // Phase 1: Initial Dialogue
        DialogueManager.instance.OnDialogueEnded += onDialogueEnd;
        DialogueManager.instance.StartDialogue(initialDialogue);
        yield return new WaitUntil(() => dialogueFinished);
        DialogueManager.instance.OnDialogueEnded -= onDialogueEnd;

        // Phase 2: Boss Appearance
        mainCamera.SetTarget(bossSpawnPos);
        yield return new WaitForSeconds(1.5f);
        bossObject.SetActive(true);

        // Phase 3: Boss Dialogue
        dialogueFinished = false;
        DialogueManager.instance.OnDialogueEnded += onDialogueEnd;
        DialogueManager.instance.StartDialogue(bossDialogue);
        yield return new WaitUntil(() => dialogueFinished);
        DialogueManager.instance.OnDialogueEnded -= onDialogueEnd;

        FinishCutscene();
    }

    private void FinishCutscene()
    {
        // Restore player position and camera target
        player.position = teleportPos;
        mainCamera.SetTarget(player);

        // Restore UI and movement
        if (worldGUI != null) worldGUI.SetActive(true);
        if (typingText != null) typingText.SetActive(true);
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusic(AudioManager.instance.backgroundmusic);
        }

        // Fire all assigned combat methods
        OnCombatStart?.Invoke();

        this.enabled = false;
    }

    public void TriggerDeathSequence()
    {
        StartCoroutine(ExecuteDeathSequence());
    }

    private IEnumerator ExecuteDeathSequence()
    {
        Vector3 dropPosition = Vector3.zero;
        if (bossObject != null)
        {
            dropPosition = bossObject.transform.position;
        }
        else if (bossSpawnPos != null)
        {
            dropPosition = bossSpawnPos.position;
        }

        // Stop boss actions and play death animation/sound
        if (bossObject != null)
        {
            bossObject.GetComponent<EnemyShooting>()?.StopShootingPhase();
            bossObject.GetComponent<BaelorisMovement>()?.StopMovementPhase();

            Animator bossAnim = bossObject.GetComponent<Animator>();
            if (bossAnim != null) bossAnim.SetTrigger("isDeath");

            EnemyReceiveDamage damageScript = bossObject.GetComponent<EnemyReceiveDamage>();
            if (damageScript != null && damageScript.deathSound != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(damageScript.deathSound);
            }
        }

        // Lock player movement during death sequence
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        yield return new WaitForSeconds(1.5f);

        // Show boss death dialogue (if any)
        if (deathDialogue != null && deathDialogue.sentences != null && deathDialogue.sentences.Length > 0)
        {
            bool dialogueFinished = false;
            System.Action onDialogueEnd = () => dialogueFinished = true;

            DialogueManager.instance.OnDialogueEnded += onDialogueEnd;
            DialogueManager.instance.StartDialogue(deathDialogue);
            yield return new WaitUntil(() => dialogueFinished);
            DialogueManager.instance.OnDialogueEnded -= onDialogueEnd;
        }

        // Boss disappears and fire any assigned events
        OnBossPerish?.Invoke();
        yield return new WaitForSeconds(1.0f);

        // Post-death dialogue (if any)
        if (postDeathDialogue != null && postDeathDialogue.sentences != null && postDeathDialogue.sentences.Length > 0)
        {
            bool dialogueFinished = false;
            System.Action onDialogueEnd = () => dialogueFinished = true;

            DialogueManager.instance.OnDialogueEnded += onDialogueEnd;
            DialogueManager.instance.StartDialogue(postDeathDialogue);
            yield return new WaitUntil(() => dialogueFinished);
            DialogueManager.instance.OnDialogueEnded -= onDialogueEnd;
        }

        // Fragment drop sequence (if applicable)
        if (bossFragment != null && bossFragment.fragmentSprite != null)
        {
            bool fragmentCollected = false;

            // Call the FragmentDropManager to handle the drop and collection process
            FragmentDropManager.instance.DropAndCollectFragment(bossFragment, dropPosition, player, () => {
                fragmentCollected = true;
            });

            // Wait until the fragment is collected before proceeding
            yield return new WaitUntil(() => fragmentCollected);
        }

        // Unlock player movement and restore UI
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        // Finalize death sequence and fire any completion events
        OnDeathComplete?.Invoke();
    }
}