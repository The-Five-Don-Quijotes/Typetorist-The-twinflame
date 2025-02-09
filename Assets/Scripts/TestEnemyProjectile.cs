using System;
using UnityEngine;

public class TestEnemyProjectile : MonoBehaviour
{
    public float damage;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) // Avoid destroying when hitting enemies
        {
            Destroy(gameObject);
        }
    }
}
