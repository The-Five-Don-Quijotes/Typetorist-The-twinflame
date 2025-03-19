using UnityEngine;

public class VorrakShooting : MonoBehaviour
{
    private Animator animator;
    public GameObject laserPrefab;  // Assign in Inspector
    private GameObject currentLaser;
    public Vector2 laserOffset = new Vector2(2.6f, 1.7f); // Offset values

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void CreateLaser()
    {
        bool isFlipped = transform.rotation.y != 0; // Check if Vorrak is flipped
        float directionMultiplier = isFlipped ? -1f : 1f; // Reverse if flipped

        // Apply offset with direction adjustment
        Vector3 spawnPosition = transform.position + new Vector3(laserOffset.x * directionMultiplier, laserOffset.y, 10);

        // Instantiate laser
        currentLaser = Instantiate(laserPrefab, spawnPosition, transform.rotation);
        currentLaser.transform.parent = transform;

        // If Vorrak is flipped, also flip the laser's rotation
        if (isFlipped)
        {
            currentLaser.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
}
