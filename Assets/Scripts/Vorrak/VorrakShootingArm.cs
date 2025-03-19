using UnityEngine;

public class VorrakShootingArm : MonoBehaviour
{
    public GameObject armPrefab; // Assign the arm prefab in the Inspector
    public Vector2 spawnOffset = new Vector2(1.41f, 2.96f); // Offset values

    public void InitArm()
    {
        if (armPrefab == null)
        {
            Debug.LogError("Arm Prefab is not assigned!");
            return;
        }

        // Check if the boss is flipped (180° on Y means flipped)
        bool isFlipped = transform.rotation.eulerAngles.y == 180;

        // Flip the spawnOffset.x if the boss is flipped
        Vector3 adjustedOffset = new Vector3(isFlipped ? -spawnOffset.x : spawnOffset.x, spawnOffset.y, 0);

        // Calculate spawn position
        Vector3 spawnPosition = transform.position + adjustedOffset;

        // Instantiate the arm
        GameObject newArm = Instantiate(armPrefab, spawnPosition, Quaternion.identity);

        //// Flip the arm if the boss is flipped
        newArm.transform.rotation = Quaternion.Euler(isFlipped ? 180 : 0, isFlipped ? 180 : 0, 0);
    }
}
