using UnityEngine;

public class GateController : MonoBehaviour
{
    private GameObject gateTilemap;
    private GameObject boss;
    private Transform player;
    private MonoBehaviour[] bossScripts;

    public float activationY = -12f; // Y position threshold for activation

    void Start()
    {
        gateTilemap = GameObject.Find("Gate"); // Find the tilemap GameObject
        boss = GameObject.FindWithTag("Boss"); // Find the boss
        player = GameObject.FindGameObjectWithTag("Player")?.transform; // Find player

        if (boss != null)
        {
            bossScripts = boss.GetComponents<MonoBehaviour>(); // Get all scripts on the boss
        }

        if (gateTilemap != null)
            gateTilemap.SetActive(false); // Deactivate Gate at start
        else
            Debug.LogError("Gate tilemap not found!");
    }

    void Update()
    {
        if (player != null && gateTilemap != null)
        {
            if (player.position.y > activationY)
            {
                gateTilemap.SetActive(true);
                SetBossScriptsActive(true);
            }
            else
            {
                SetBossScriptsActive(false); // Disable all boss scripts
            }
        }

        if (boss != null)
        {
            float bossHealth = boss.GetComponent<EnemyReceiveDamage>().health;
            if (bossHealth == 0 && gateTilemap != null)
            {
                gateTilemap.SetActive(false);
            }
        }
    }

    private void SetBossScriptsActive(bool state)
    {
        if (bossScripts != null)
        {
            foreach (MonoBehaviour script in bossScripts)
            {
                if (!state) // If disabling the scripts
                {
                    script.StopAllCoroutines(); // Stop all running coroutines
                }
                script.enabled = state;
            }
        }
    }

    public bool isGateActive()
    {
        if(gateTilemap == null) gateTilemap = GameObject.Find("Gate");
        if(gateTilemap == null) return false;
        return gateTilemap.activeSelf;
    }
}
