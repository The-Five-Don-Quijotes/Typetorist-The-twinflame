using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ZhavokAttack : MonoBehaviour
{
    private Animator animator;
    private Transform player;
    public float ZhavokOffset = 2f;

    [Header("Attack Settings")]
    public float cooldown = 0.1f;
    public float hitboxDuration = 0.2f;

    [Header("Hitboxes")]
    public GameObject melee1;
    public GameObject melee2;

    [Header("Bullet")]
    public GameObject meleeBulletPrefab;
    public float bulletSpeed = 10f;
    public float bulletAngleOffset = 30f; // Angle difference for top and bottom bullets

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.Find("Player").transform;
        StartCoroutine(AttackPlayer());
    }

    IEnumerator AttackPlayer()
    {
        if (player != null)
        {
            if (!animator.GetBool("isSummoning"))
            {
                DoAttack();
            }

            //Attack again
            yield return new WaitForSeconds(cooldown);
            StartCoroutine(AttackPlayer());
        }
        else
        {
            Debug.Log("Player not found.");
        }
    }

    private void DoAttack()
    {
        animator.SetTrigger("isAttacking");
    }

    public void ActivateFirstHitbox()
    {
        StartCoroutine(EnableHitbox(melee1));
        FireBulletPattern(false);
    }

    public void ActivateSecondHitbox()
    {
        StartCoroutine(EnableHitbox(melee2));
        FireBulletPattern(true);
    }

    IEnumerator EnableHitbox(GameObject hitbox)
    {
        hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxDuration);
        hitbox.SetActive(false);
    }

    private void FireBulletPattern(Boolean isDown)
    {
        if (GetComponent<EnemyReceiveDamage>().health < GetComponent<EnemyReceiveDamage>().maxHealth)
        {
            float baseAngle = transform.rotation.y == 0 ? 0f : 180f;

            if (isDown)
            {
                baseAngle -= 90f; // Middle bullet points downward
            }

            SpawnBullet(baseAngle); // Middle bullet
            SpawnBullet(baseAngle + bulletAngleOffset); // Top bullet
            SpawnBullet(baseAngle + bulletAngleOffset / 2); // Top bullet
            SpawnBullet(baseAngle - bulletAngleOffset); // Bottom bullet
            SpawnBullet(baseAngle - bulletAngleOffset / 2); // Bottom bullet
        }
    }

    private void SpawnBullet(float angle)
    {
        GameObject bullet = Instantiate(meleeBulletPrefab,new Vector3 (transform.position.x, transform.position.y + ZhavokOffset, transform.position.z), Quaternion.identity);

        Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * bulletSpeed;
    }
}
