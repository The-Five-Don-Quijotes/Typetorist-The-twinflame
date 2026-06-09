using UnityEngine;

public class AntiCampObserver : MonoBehaviour
{
    public Transform player;
    public GameObject ghostHandPrefab;

    [Header("Camping Detection")]
    public float timeToTriggerHand = 4f;
    public float campRadius = 1.5f;

    [Header("Telegraphing")]
    [Tooltip("Assign a SpriteRenderer attached to the Player (e.g., an icon above their head).")]
    public SpriteRenderer warningIcon;

    private Vector2 savedPosition;
    private float campTimer = 0f;
    private bool isPhase2Active = false;

    private EnemyReceiveDamage bossHealth;

    private void Start()
    {
        bossHealth = GetComponent<EnemyReceiveDamage>();
        if (player != null)
        {
            savedPosition = player.position;
        }

        // Hide the warning icon upon initialization
        SetWarningAlpha(0f);
    }

    private void Update()
    {
        if (player == null || bossHealth == null) return;

        // Activate the camping detection when boss health reaches the threshold
        if (!isPhase2Active && bossHealth.health <= bossHealth.maxHealth / 2)
        {
            isPhase2Active = true;
            savedPosition = player.position;
        }

        if (!isPhase2Active) return;

        // Reset logic if player breaks the camping radius
        if (Vector2.Distance(player.position, savedPosition) > campRadius)
        {
            savedPosition = player.position;
            campTimer = 0f;
            SetWarningAlpha(0f);
        }
        else
        {
            // Increment timer if player remains inside the radius
            campTimer += Time.deltaTime;

            // Calculate fade-in progress (0.0 to 1.0)
            float warningProgress = campTimer / timeToTriggerHand;
            SetWarningAlpha(warningProgress);

            // Execute the grab mechanic
            if (campTimer >= timeToTriggerHand)
            {
                TriggerGhostHand();
                campTimer = 0f;
                SetWarningAlpha(0f);
            }
        }
    }

    private void TriggerGhostHand()
    {
        GameObject hand = Instantiate(ghostHandPrefab, transform.position, Quaternion.identity);
        hand.GetComponent<GhostHand>().Initialize(player, transform);
    }

    private void SetWarningAlpha(float alpha)
    {
        if (warningIcon != null)
        {
            Color iconColor = warningIcon.color;
            iconColor.a = Mathf.Clamp01(alpha);
            warningIcon.color = iconColor;
        }
    }
}