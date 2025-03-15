using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    public float speed;
    public float amplitude = 1f;
    public float frequency = 2f;
    public bool isSineWave;

    private Vector2 moveDirection;
    private float timeElapsed;

    void Start()
    {
        // Capture the bullet's initial movement direction from Rigidbody2D velocity
        moveDirection = GetComponent<Rigidbody2D>().linearVelocity.normalized;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        // Calculate the wave offset perpendicular to the movement direction
        Vector2 perpendicular = new Vector2(-moveDirection.y, moveDirection.x);
        float waveOffset = isSineWave
            ? Mathf.Sin(timeElapsed * frequency) * amplitude
            : -Mathf.Sin(timeElapsed * frequency) * amplitude;

        // Move the bullet forward and apply the wave offset
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime + perpendicular * waveOffset * Time.deltaTime);
    }
}
