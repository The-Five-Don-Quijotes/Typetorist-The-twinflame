using UnityEngine;

public class ActiveSpawner : MonoBehaviour
{
    [Header("Spawners")]
    [Tooltip("Assign all BulletSpawner GameObjects here in the Inspector.")]
    public GameObject[] bulletSpawners;

    [Header("Boss Reference")]
    [Tooltip("Assign the Baeloris GameObject here to avoid searching at runtime.")]
    public EnemyReceiveDamage bossHealthComponent;

    // Tracks the current state to prevent redundant SetActive calls every frame
    private bool areSpawnersActive = false;

    private void Start()
    {
        // Fallback: Find the boss once on startup if not assigned in the Inspector
        if (bossHealthComponent == null)
        {
            GameObject boss = GameObject.Find("Baeloris");
            if (boss != null)
            {
                bossHealthComponent = boss.GetComponent<EnemyReceiveDamage>();
            }
            else
            {
                Debug.LogError("Baeloris not found in the scene.");
            }
        }

        // Initialize all spawners to an inactive state
        SetSpawnersState(false);
    }

    private void Update()
    {
        if (bossHealthComponent == null) return;

        float health = bossHealthComponent.health;

        // Spawners are active in two phases: between 75-50, and 25-0
        bool shouldBeActive = (health <= 75f && health > 50f) || (health <= 25f && health > 0f);

        // Execute state change only when transitioning between phases
        if (shouldBeActive != areSpawnersActive)
        {
            SetSpawnersState(shouldBeActive);
        }
    }

    private void SetSpawnersState(bool state)
    {
        areSpawnersActive = state;

        foreach (GameObject spawner in bulletSpawners)
        {
            if (spawner != null)
            {
                spawner.SetActive(state);
            }
        }
    }
}