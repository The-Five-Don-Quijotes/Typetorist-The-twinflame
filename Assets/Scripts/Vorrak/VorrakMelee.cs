using UnityEngine;

public class VorrakMeele : MonoBehaviour
{
    public int damage = 1;
    private Transform bossTransform;

    private void Start()
    {
        bossTransform = transform.parent; // Assuming melee is a child of the boss
    }

    private void Update()
    {
        if (bossTransform != null)
        {
            // Flip the melee hitbox based on the boss's facing direction
            transform.localScale = new Vector3(bossTransform.localScale.x > 0 ? 1 : -1, 1, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats.playerStats.DealDamage(damage);
        }
    }
}
