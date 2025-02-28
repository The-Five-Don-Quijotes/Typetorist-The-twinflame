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
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(ShootPlayer());
    }

    IEnumerator ShootPlayer()
    {
        yield return new WaitForSeconds(cooldown);
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
            if (!animator.GetBool("isDeath"))
            {
                StartCoroutine(ShootPlayer()); // Stop the coroutine if isDeath is triggered
            }
        }
    }
}
