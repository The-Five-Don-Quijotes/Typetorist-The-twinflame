using UnityEngine;

public class LaserDamage : MonoBehaviour
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
        }
    }
}
