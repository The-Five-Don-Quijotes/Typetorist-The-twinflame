using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SafePortalSpawn : MonoBehaviour
{
    private Collider2D portalCollider;
    private Transform player;

    [Header("Configuration")]
    public float safeDistance = 2.0f; // Minimum distance the player must be to arm the portal

    private void Awake()
    {
        portalCollider = GetComponent<Collider2D>();
        // 1. Hard-disable the collider immediately upon instantiation/Awake
        if (portalCollider != null)
        {
            portalCollider.enabled = false;
        }
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void OnEnable()
    {
        // 2. Redundancy check: Ensure collider remains disabled when SetActive(true) is called
        if (portalCollider != null)
        {
            portalCollider.enabled = false;
        }
    }

    private void Update()
    {
        // Ignore execution if dependencies are missing or the portal is already armed
        if (portalCollider == null || portalCollider.enabled || player == null) return;

        // 3. Strict spatial check. 
        // If player is standing exactly on the portal (distance is near 0), this evaluates false.
        // The portal will ONLY arm when the player walks outside the safe radius.
        if (Vector3.Distance(transform.position, player.position) > safeDistance)
        {
            portalCollider.enabled = true;
            this.enabled = false; // Disable script execution overhead once armed
        }
    }
}