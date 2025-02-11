using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public GameObject projectile;
    public Transform player;
    public int minDamage;
    public int maxDamage;
    public float projectileForce;
    public float cooldown;

    void Start()
    {
        StartCoroutine(ShootPlayer());
    }

    IEnumerator ShootPlayer()
    {
        yield return new WaitForSeconds(cooldown);
        if (player != null) 
        {
            GameObject spell = Instantiate(projectile, transform.position, Quaternion.identity);
            Vector2 myPos = transform.position;
            Vector2 targetPos = player.position;
            Vector2 direction = (targetPos - myPos).normalized;
            spell.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileForce;
            spell.GetComponent<TestEnemyProjectile>().damage = Random.Range(minDamage, maxDamage);
            StartCoroutine(ShootPlayer());
        }
    }
}
