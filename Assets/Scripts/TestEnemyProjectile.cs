using System;
using UnityEngine;

public class TestEnemyProjectile : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null && !playerMovement.isInvincible)
            {
                PlayerStats.playerStats.DealDamage(damage);
            }
            Destroy(gameObject); 
        }

        if (collision.CompareTag("Wall") || collision.CompareTag("Furniture"))
        {
            Destroy(gameObject);
        }
    }
}
