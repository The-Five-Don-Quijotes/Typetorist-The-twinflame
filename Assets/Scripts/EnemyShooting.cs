using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public GameObject bulletA; // Sine wave bullet
    public GameObject bulletB; // Cosine wave bullet
    public GameObject projectile;
    public Transform player;
    public int minDamage;
    public int maxDamage;
    public float projectileForce;
    public float cooldown;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(ShootPlayer());
        StartCoroutine(Phase2ShootPlayer());
    }

    void OnEnable()
    {
        StartCoroutine(ShootPlayer());
        StartCoroutine(Phase2ShootPlayer());
    }

    IEnumerator ShootPlayer()
    {
        if(player != null)
        {
            yield return new WaitForSeconds(cooldown);
            if (GetComponent<EnemyReceiveDamage>().health > GetComponent<EnemyReceiveDamage>().maxHealth / 2)
            {
                // Flip the enemy to face the player
                if (player.position.x > transform.position.x)
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                if (player != null)
                {
                    // Play the attack animation
                    animator.SetTrigger("Attack");

                    GameObject spell = Instantiate(projectile, transform.position, Quaternion.identity);
                    Vector2 myPos = transform.position;
                    Vector2 targetPos = player.position;
                    Vector2 direction = (targetPos - myPos).normalized;
                    spell.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileForce;

                    // Rotate the bullet to face the direction it's moving
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180;
                    spell.transform.rotation = Quaternion.Euler(0, 0, angle);

                    spell.GetComponent<TestEnemyProjectile>().damage = Random.Range(minDamage, maxDamage);
                }
            }
            if (!animator.GetBool("isDeath"))
            {
                StartCoroutine(ShootPlayer()); // Stop the coroutine if isDeath is triggered
            }
        }
    }

    IEnumerator Phase2ShootPlayer()
    {
        if(player != null)
        {
            yield return new WaitForSeconds(cooldown);

            if (GetComponent<EnemyReceiveDamage>().health <= GetComponent<EnemyReceiveDamage>().maxHealth / 2 && GetComponent<EnemyReceiveDamage>().health != 0) //Health below 50
            {
                cooldown = 0.1f;
                if (transform != null) yield return null;
                // Face the player
                if (player.position.x > transform.position.x)
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }

                if (player != null)
                {
                    // Shoot Bullet A (Sine wave)
                    GameObject spellA = Instantiate(bulletA, transform.position, Quaternion.identity);
                    SetupBullet(spellA, true, false);

                    // Shoot Bullet B (Cosine wave)
                    GameObject spellB = Instantiate(bulletB, transform.position, Quaternion.identity);
                    SetupBullet(spellB, false, false);

                    // Shoot Bullet C (Sine wave)
                    GameObject spellC = Instantiate(bulletA, transform.position, Quaternion.identity);
                    SetupBullet(spellC, true, true);

                    // Shoot Bullet D (Cosine wave)
                    GameObject spellD = Instantiate(bulletB, transform.position, Quaternion.identity);
                    SetupBullet(spellD, false, true);
                }
            }

            if (!animator.GetBool("isDeath"))
            {
                StartCoroutine(Phase2ShootPlayer());
            }
        }
    }

    void SetupBullet(GameObject bullet, bool isSineWave, bool isOposite)
    {
        Vector2 myPos = transform.position;
        Vector2 targetPos = player.position;
        Vector2 direction = (targetPos - myPos).normalized;
        if(isOposite)
        {
            direction = -direction;
        }
        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileForce;

        BulletMovement bulletMovement = bullet.GetComponent<BulletMovement>();
        bulletMovement.isSineWave = isSineWave;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
