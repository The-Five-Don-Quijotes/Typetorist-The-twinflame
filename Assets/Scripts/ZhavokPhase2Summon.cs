using UnityEngine;

public class ZhavokPhase2Summon : MonoBehaviour
{
    private Animator animator;
    private EnemyReceiveDamage healthComponent;
    public GameObject summon;

    [Header("Summon Formation Settings")]
    [Tooltip("The distance from Zhavok to spawn the minions on the X and Y axis.")]
    public float spawnOffsetRadius = 6f;

    [Header("Cooldown Settings")]
    [Tooltip("Time in seconds before the boss can summon another minion.")]
    public float summonCooldown = 3f;
    private float nextSummonTime = 0f;

    [Header("Audio Settings")]
    public AudioClip summonSound;

    private void Start()
    {
        animator = GetComponent<Animator>();
        healthComponent = GetComponent<EnemyReceiveDamage>();
    }

    private void Update()
    {
        if (healthComponent == null) return;

        // Execute summon logic only if health is below threshold and the cooldown has expired
        if (healthComponent.health < 75 && Time.time >= nextSummonTime)
        {
            // Placed inside the time check to prevent expensive tag searches every frame
            GameObject[] activeSummons = GameObject.FindGameObjectsWithTag("Summon");

            if (activeSummons.Length < 4)
            {
                DoSummon(activeSummons.Length);

                // Set the timestamp for when the next summon is permitted
                nextSummonTime = Time.time + summonCooldown;
            }
        }
    }

    private void DoSummon(int sequenceIndex)
    {
        if (AudioManager.instance != null && summonSound != null)
        {
            AudioManager.instance.PlaySFX(summonSound);
        }

        if (animator != null)
        {
            animator.SetTrigger("isSummoning");
        }

        if (summon == null)
        {
            Debug.LogError("Summon prefab is missing from ZhavokPhase2Summon!", this);
            return;
        }

        Vector3 spawnPosition = CalculateSafeSpawnPosition(sequenceIndex);
        Instantiate(summon, spawnPosition, Quaternion.identity);
    }

    /// <summary>
    /// Calculates a localized spawn coordinate relative to Zhavok's position
    /// to guarantee the minion does not spawn inside environment walls.
    /// </summary>
    private Vector3 CalculateSafeSpawnPosition(int index)
    {
        Vector3 anchorPos = transform.position;

        switch (index)
        {
            case 0: // Bottom Left
                return anchorPos + new Vector3(-spawnOffsetRadius, -spawnOffsetRadius, 0);
            case 1: // Top Left
                return anchorPos + new Vector3(-spawnOffsetRadius, spawnOffsetRadius, 0);
            case 2: // Bottom Right
                return anchorPos + new Vector3(spawnOffsetRadius, -spawnOffsetRadius, 0);
            case 3: // Top Right
                return anchorPos + new Vector3(spawnOffsetRadius, spawnOffsetRadius, 0);
            default:
                return anchorPos;
        }
    }
}