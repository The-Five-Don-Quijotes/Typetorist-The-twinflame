using UnityEngine;
using System.Collections; // --- ADD THIS --- for using Coroutines

public class GateController : MonoBehaviour
{
    private GameObject gateTilemap;
    private GameObject gateTilemap1;
    private GameObject gateTilemap2;
    private GameObject gateTilemap3;
    private GameObject boss;
    private Transform player;
    private MonoBehaviour[] bossScripts;

    public float activationY = -12f; // Y position threshold for activation

    public float animationFrameTime = 0.2f; // Time between each gate frame
    private bool hasBeenTriggered = false; // Prevents the animation from playing more than once
    private bool isDeactivating = false;
    private bool isAnimatedScene = false; // Checks if gates 1, 2, 3 exist

    void Start()
    {
        // Find all GameObjects
        gateTilemap = GameObject.Find("Gate");
        gateTilemap1 = GameObject.Find("Gate (1)");
        gateTilemap2 = GameObject.Find("Gate (2)");
        gateTilemap3 = GameObject.Find("Gate (3)");
        boss = GameObject.FindWithTag("Boss");
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (boss != null)
        {
            bossScripts = boss.GetComponents<MonoBehaviour>();
        }

        // If any of the animation gates exist
        if (gateTilemap1 != null || gateTilemap2 != null || gateTilemap3 != null)
        {
            isAnimatedScene = true;
        }

        // Deactivate all gates at the start
        if (gateTilemap != null)
            gateTilemap.SetActive(false);
        else
            Debug.LogError("Main 'Gate' tilemap not found!");

        // Also deactivate the animation gates
        //if (gateTilemap1 != null) gateTilemap1.SetActive(false);
        if (gateTilemap2 != null) gateTilemap2.SetActive(false);
        if (gateTilemap3 != null) gateTilemap3.SetActive(false);
    }

    void Update()
    {
        if (player != null && !hasBeenTriggered) // Only run this if it hasn't been triggered yet
        {
            if (player.position.y > activationY)
            {
                // Player crossed the line for the first time
                // Start the activation sequence!
                StartCoroutine(PlayActivationAnimation());
            }
            else if (boss != null)
            {
                // Player is below the line, keep boss scripts disabled
                SetBossScriptsActive(false);
            }
        }

        // Only check boss health *after* the fight has started
        if (hasBeenTriggered && boss != null)
        {
            float bossHealth = 0;
            var damageScript = boss.GetComponent<EnemyReceiveDamage>();
            if (damageScript != null)
            {
                bossHealth = damageScript.health;
            }

            if (bossHealth <= 0 && gateTilemap != null)
            {
                //gateTilemap.SetActive(false);
                if (!isDeactivating)
                {
                    StartCoroutine(PlayDeactivationAnimation());
                }
            }
        }
    }

    private IEnumerator PlayActivationAnimation()
    {
        hasBeenTriggered = true;

        if (boss != null)
        {
            SetBossScriptsActive(true);
        }

        if (isAnimatedScene)
        {
            // Play the 3-stage animation
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
    }

    private IEnumerator PlayDeactivationAnimation()
    {
        // 1. Set the flag so this only runs once
        isDeactivating = true;

        // 2. Deactivate the main, solid gate
        if (gateTilemap != null)
        {
            gateTilemap.SetActive(false);
        }

        // 3. Play the reverse animation if in the animated scene
        if (isAnimatedScene)
        {
            // Show 3
            if (gateTilemap3 != null) gateTilemap3.SetActive(true);
            yield return new WaitForSeconds(animationFrameTime);

            // Hide 3, Show 2
            if (gateTilemap3 != null) gateTilemap3.SetActive(false);
            if (gateTilemap2 != null) gateTilemap2.SetActive(true);
            yield return new WaitForSeconds(animationFrameTime);

            // Hide 2, Show 1
            if (gateTilemap2 != null) gateTilemap2.SetActive(false);
            if (gateTilemap1 != null) gateTilemap1.SetActive(true);
        }

        // 4. Optional: Ensure boss scripts are off (since boss is dead)
        if (boss != null)
        {
            SetBossScriptsActive(false);
        }
    }

    private void SetBossScriptsActive(bool state)
    {
        if (bossScripts != null)
        {
            foreach (MonoBehaviour script in bossScripts)
            {
                if (script == null || script == this)
                {
                    continue;
                }

                if (!state) // If disabling the scripts
                {
                    script.StopAllCoroutines();
                }
                script.enabled = state;
            }
        }
    }

    public bool isGateActive()
    {
        if (gateTilemap == null) gateTilemap = GameObject.Find("Gate");
        if (gateTilemap == null) return false;
        return gateTilemap.activeSelf;
    }
}