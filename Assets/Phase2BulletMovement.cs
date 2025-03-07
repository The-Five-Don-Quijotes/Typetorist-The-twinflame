using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    public float speed;
    public float amplitude = 1f;
    public float frequency = 2f;
    public bool isSineWave;

    private Vector2 startPosition;
    private float timeElapsed;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        float waveOffset = isSineWave ? Mathf.Sin(timeElapsed * frequency) : Mathf.Cos(timeElapsed * frequency);

        transform.position = new Vector2(startPosition.x + timeElapsed * speed, startPosition.y + waveOffset * amplitude);
    }
}