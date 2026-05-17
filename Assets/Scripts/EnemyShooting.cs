using System.Collections;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public GameObject bulletA;
    public GameObject bulletB;
    public GameObject projectile;
    public Transform player;
    public int minDamage;
    public int maxDamage;
    public float projectileForce;
    public float cooldown;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        // Execution removed from Start/OnEnable
    }

    // Explicitly call this when cutscene ends
    public void BeginShootingPhase()
    {
        StartCoroutine(ShootPlayer());
        StartCoroutine(Phase2ShootPlayer());
    }

    public void StopShootingPhase()
    {
        StopAllCoroutines();
    }

    private IEnumerator ShootPlayer()
    {
        if (player != null)
        {
            yield return new WaitForSeconds(cooldown);

            // Replaced repeated GetComponent calls with local logic or cached references in production
            EnemyReceiveDamage damageScript = GetComponent<EnemyReceiveDamage>();

            if (damageScript.health > damageScript.maxHealth / 2)
            {
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
                    animator.SetTrigger("Attack");

                    GameObject spell = Instantiate(projectile, transform.position, Quaternion.identity);
                    Vector2 myPos = transform.position;
                    Vector2 targetPos = player.position;
                    Vector2 direction = (targetPos - myPos).normalized;
                    spell.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileForce;

                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180;
                    spell.transform.rotation = Quaternion.Euler(0, 0, angle);

                    spell.GetComponent<TestEnemyProjectile>().damage = Random.Range(minDamage, maxDamage);
                }
            }

            if (!animator.GetBool("isDeath"))
            {
                StartCoroutine(ShootPlayer());
            }
        }
    }

    private IEnumerator Phase2ShootPlayer()
    {
        if (player != null)
        {
            yield return new WaitForSeconds(cooldown);

            if (player == null)
            {
                yield break;
            }

            EnemyReceiveDamage damageScript = GetComponent<EnemyReceiveDamage>();

            if (damageScript.health <= damageScript.maxHealth / 2 && damageScript.health > 0)
            {
                cooldown = 0.1f;
                if (transform != null) yield return null;

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
                    GameObject spellA = Instantiate(bulletA, transform.position, Quaternion.identity);
                    SetupBullet(spellA, true, false);

                    GameObject spellB = Instantiate(bulletB, transform.position, Quaternion.identity);
                    SetupBullet(spellB, false, false);

                    GameObject spellC = Instantiate(bulletA, transform.position, Quaternion.identity);
                    SetupBullet(spellC, true, true);

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

    private void SetupBullet(GameObject bullet, bool isSineWave, bool isOpposite)
    {
        Vector2 myPos = transform.position;
        Vector2 targetPos = player.position;
        Vector2 direction = (targetPos - myPos).normalized;

        if (isOpposite)
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