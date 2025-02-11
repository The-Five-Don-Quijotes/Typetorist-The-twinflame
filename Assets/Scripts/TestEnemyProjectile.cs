using System;
using UnityEngine;

public class TestEnemyProjectile : MonoBehaviour
{
    public int damage;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats.playerStats.DealDamage(damage);
            Destroy(gameObject);
        }
    }
}
