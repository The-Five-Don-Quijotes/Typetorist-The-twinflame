using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    enum SpawnerType { Straight, Spin, Wave }

    [Header("Bullet Attributes")]
    public GameObject bullet;
    public float bulletLife = 1f;
    public float speed = 1f;

    [Header("Spawner Attributes")]
    [SerializeField] private SpawnerType spawnerType;
    [SerializeField] private float firingRate = 1f;
    [SerializeField] private float waveFrequency = 1f;
    [SerializeField] private float waveAmplitude = 1f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip shootSound;

    private GameObject spawnedBullet;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (spawnerType == SpawnerType.Spin)
        {
            transform.eulerAngles = new Vector3(0f, 0f, transform.eulerAngles.z + 1f);
        }

        if (timer >= firingRate)
        {
            Fire();
            timer = 0;
        }
    }

    private void Fire()
    {
        if (bullet)
        {
            spawnedBullet = Instantiate(bullet, transform.position, Quaternion.identity);

            Bullet bulletScript = spawnedBullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.speed = speed;
                bulletScript.bulletLife = bulletLife;
            }

            spawnedBullet.transform.rotation = transform.rotation;

            if (spawnerType == SpawnerType.Wave)
            {
                spawnedBullet.AddComponent<BulletWaveMotion>().Initialize(waveFrequency, waveAmplitude);
            }

            // Trigger the audio playback
            if (AudioManager.instance != null && shootSound != null)
            {
                AudioManager.instance.PlaySFX(shootSound);
            }
        }
    }
}

public class BulletWaveMotion : MonoBehaviour
{
    private float waveFrequency;
    private float waveAmplitude;
    private float startTime;

    public void Initialize(float frequency, float amplitude)
    {
        waveFrequency = frequency;
        waveAmplitude = amplitude;
        startTime = Time.time;
    }

    void Update()
    {
        float waveOffset = Mathf.Sin((Time.time - startTime) * waveFrequency) * waveAmplitude;
        transform.position += new Vector3(waveOffset, 0, 0) * Time.deltaTime;
    }
}