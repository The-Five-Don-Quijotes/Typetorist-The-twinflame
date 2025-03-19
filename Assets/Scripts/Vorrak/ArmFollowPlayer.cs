using UnityEngine;

public class ArmFollowPlayer : MonoBehaviour
{
    private Transform target; // Assign the player or target in Inspector or via script
    public float speed = 5f; // How fast the arm moves
    public float rotationSpeed = 200f; // How fast it rotates towards the target
    public float lifetime = 5f; // How long before it destroys itself

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform; // Find the player
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject); // If no target, self-destruct
            return;
        }

        // Calculate direction to target
        Vector3 direction = (target.position - transform.position).normalized;

        // Rotate towards target smoothly
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move towards target
        transform.position += transform.right * speed * Time.deltaTime;
    }
}
