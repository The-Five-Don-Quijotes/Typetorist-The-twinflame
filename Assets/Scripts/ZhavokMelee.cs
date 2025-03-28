using UnityEngine;

public class ZhavokMelee : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats.playerStats.DealDamage(damage);
        }
    }
}
