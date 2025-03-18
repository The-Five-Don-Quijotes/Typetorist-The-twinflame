using UnityEngine;
using UnityEngine.Tilemaps;

public class GateController : MonoBehaviour
{
    private GameObject gateTilemap;
    private GameObject boss;
    private Transform player;

    void Start()
    {
        gateTilemap = GameObject.Find("Gate"); // Find the tilemap GameObject
        boss = GameObject.FindWithTag("Boss"); // Find the boss
        player = GameObject.FindGameObjectWithTag("Player")?.transform; // Find player

        if (gateTilemap != null)
            gateTilemap.SetActive(false); // Deactivate Gate at start
        else
            Debug.LogError("Gate tilemap not found!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && gateTilemap != null)
        {
            gateTilemap.SetActive(true); // Activate Gate when player is on top
        }
    }

    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player") && gateTilemap != null)
    //    {
    //        gateTilemap.SetActive(false); // Optionally deactivate when player leaves
    //    }
    //}

    void Update()
    {
        if (boss != null)
        {
            float bossHealth = boss.GetComponent<EnemyReceiveDamage>().health;
            if (bossHealth == 0 && gateTilemap != null)
            {
                gateTilemap.SetActive(false); // Deactivate when boss dies
            }
        }
    }
}
