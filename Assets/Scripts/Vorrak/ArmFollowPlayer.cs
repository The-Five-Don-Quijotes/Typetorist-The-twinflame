using UnityEngine;

public class ArmFollowPlayer : MonoBehaviour
{
    private Transform target;
    public float speed = 5f;
    public float rotationSpeed = 200f;
    public float lifetime = 5f;

    // --- NEW: Support for fixed target in cutscenes ---
    private Vector3? fixedTargetPos = null;

    private void Start()
    {
        // Only find player if no fixed target was set before Start
        if (!fixedTargetPos.HasValue)
        {
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        Vector3 currentDestination;

        // Determine destination based on mode
        if (fixedTargetPos.HasValue)
        {
            currentDestination = fixedTargetPos.Value;

            // Destroy if it reached the fixed target
            if (Vector3.Distance(transform.position, currentDestination) < 0.5f)
            {
                Destroy(gameObject);
                return;
            }
        }
        else if (target != null)
        {
            currentDestination = target.position;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Calculate direction to destination
        Vector3 direction = (currentDestination - transform.position).normalized;

        // Rotate towards target smoothly
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move towards target
        transform.position += transform.right * speed * Time.deltaTime;
    }

    // --- NEW: Method to set a specific point for the cutscene ---
    public void SetFixedTarget(Vector3 position)
    {
        fixedTargetPos = position;
        target = null; // Ignore player
    }
}