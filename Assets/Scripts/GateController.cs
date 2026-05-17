using UnityEngine;
using System.Collections;

public class GateController : MonoBehaviour
{
    public float activationY = -12f;
    public float animationFrameTime = 0.2f;

    // --- CHANGE 1: Update to the new generic controller ---
    public BossCutsceneController dialogueController;

    private GameObject gateTilemap;
    private GameObject gateTilemap1;
    private GameObject gateTilemap2;
    private GameObject gateTilemap3;
    private GameObject boss;
    private Transform player;

    private bool hasBeenTriggered = false;
    private bool isDeactivating = false;
    private bool isAnimatedScene = false;

    private void Start()
    {
        gateTilemap = GameObject.Find("Gate");
        gateTilemap1 = GameObject.Find("Gate (1)");
        gateTilemap2 = GameObject.Find("Gate (2)");
        gateTilemap3 = GameObject.Find("Gate (3)");
        boss = GameObject.FindWithTag("Boss");
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (gateTilemap1 != null || gateTilemap2 != null || gateTilemap3 != null)
        {
            isAnimatedScene = true;
        }

        if (gateTilemap != null) gateTilemap.SetActive(false);
        if (gateTilemap2 != null) gateTilemap2.SetActive(false);
        if (gateTilemap3 != null) gateTilemap3.SetActive(false);
    }

    private void Update()
    {
        if (player != null && !hasBeenTriggered)
        {
            if (player.position.y > activationY)
            {
                StartCoroutine(PlayActivationSequence());
            }
        }

        if (hasBeenTriggered && boss != null)
        {
            EnemyReceiveDamage damageScript = boss.GetComponent<EnemyReceiveDamage>();
            if (damageScript != null && damageScript.health <= 0 && gateTilemap != null)
            {
                if (!isDeactivating)
                {
                    StartCoroutine(PlayDeactivationAnimation());
                }
            }
        }
    }

    private IEnumerator PlayActivationSequence()
    {
        hasBeenTriggered = true;

        if (isAnimatedScene)
        {
            yield return new WaitForSeconds(animationFrameTime);

            if (gateTilemap1 != null) gateTilemap1.SetActive(false);
            if (gateTilemap2 != null) gateTilemap2.SetActive(true);
            yield return new WaitForSeconds(animationFrameTime);

            if (gateTilemap2 != null) gateTilemap2.SetActive(false);
            if (gateTilemap3 != null) gateTilemap3.SetActive(true);
            yield return new WaitForSeconds(animationFrameTime);

            if (gateTilemap3 != null) gateTilemap3.SetActive(false);
        }

        if (gateTilemap != null)
        {
            gateTilemap.SetActive(true);
        }

        if (dialogueController != null)
        {
            dialogueController.TriggerSequence();
        }
        else
        {
            // --- CHANGE 2: Update the debug log message ---
            Debug.LogError("BossCutsceneController reference is missing in GateController.");
        }
    }

    private IEnumerator PlayDeactivationAnimation()
    {
        isDeactivating = true;

        if (gateTilemap != null) gateTilemap.SetActive(false);

        if (isAnimatedScene)
        {
            if (gateTilemap3 != null) gateTilemap3.SetActive(true);
            yield return new WaitForSeconds(animationFrameTime);

            if (gateTilemap3 != null) gateTilemap3.SetActive(false);
            if (gateTilemap2 != null) gateTilemap2.SetActive(true);
            yield return new WaitForSeconds(animationFrameTime);

            if (gateTilemap2 != null) gateTilemap2.SetActive(false);
            if (gateTilemap1 != null) gateTilemap1.SetActive(true);
        }

        if (boss != null)
        {
            boss.GetComponent<EnemyShooting>()?.StopShootingPhase();
            boss.GetComponent<BaelorisMovement>()?.StopMovementPhase();
        }
    }

    public bool isGateActive()
    {
        if (gateTilemap == null) gateTilemap = GameObject.Find("Gate");
        if (gateTilemap == null) return false;
        return gateTilemap.activeSelf;
    }
}