using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletLife;
    public float rotation;
    public float speed;
    public float waveAmplitude = 1f; // How big the wave is
    public float waveFrequency = 1f; // How fast the wave oscillates

    private enum MovementType { Straight, SineWave, CosineWave }
    [SerializeField] private MovementType movementType;

    private Vector2 spawnPoint;
    private float timer;

    void Start()
    {
        spawnPoint = new Vector2(transform.position.x, transform.position.y);
    }

    void Update()
    {
        if (timer > bulletLife)
        {
            Destroy(gameObject);
        }
        timer += Time.deltaTime;
        transform.position = Movement(timer);
    }

    private Vector2 Movement(float timer)
    {
        float x = timer * speed * transform.right.x;
        float y = timer * speed * transform.right.y;

        switch (movementType)
        {
            case MovementType.SineWave:
                y += Mathf.Sin(timer * waveFrequency) * waveAmplitude;
                break;
            case MovementType.CosineWave:
                y += Mathf.Cos(timer * waveFrequency) * waveAmplitude;
                break;
        }

        return new Vector2(x + spawnPoint.x, y + spawnPoint.y);
    }
}
