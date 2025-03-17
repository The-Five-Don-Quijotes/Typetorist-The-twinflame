using System.Collections;
using UnityEngine;

public class SummonPhase2Behavior : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float cooldowns = 5f;
    public int numberOfBullet = 8;
    public float bulletSpeed = 5f;
    public float bulletLife = 30f;
    public float stoppingDistnce = 0.5f;

    private void Start()
    {
        gameObject.GetComponent<ZhavokSummonMovement>().stoppingDistance = stoppingDistnce;
        StartCoroutine(SummonBullets());
    }

    IEnumerator SummonBullets()
    {
        yield return new WaitForSeconds(cooldowns);
        for (int i = 0; i < numberOfBullet; i++) // x Bullets
        {
            float angle = i * (360 / numberOfBullet); // x bullets equally spaced (360° / x)
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            GameObject spawnedBullet = Instantiate(bulletPrefab, transform.position, rotation);
            Bullet bulletComponent = spawnedBullet.GetComponent<Bullet>();

            if (bulletComponent != null)
            {
                bulletComponent.speed = bulletSpeed;
                bulletComponent.bulletLife = bulletLife;
            }

            // Move bullet in its forward direction
            Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = rotation * Vector2.up * bulletSpeed;
            }
        }

        StartCoroutine(SummonBullets());
    }
}
