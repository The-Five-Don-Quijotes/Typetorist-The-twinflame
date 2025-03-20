using UnityEngine;
using UnityEngine.Tilemaps;

public class GateController : MonoBehaviour
{
    private GameObject gateTilemap;
    private GameObject boss;
    private Transform player;

    public float activationY = -12f; // Y position threshold for activation

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

    void Update()
    {
        // Activate gate if player is above a certain Y position
        if (player != null && gateTilemap != null)
        {
            if (player.position.y > activationY)
            {
                gateTilemap.SetActive(true);
            }
        }

        // Deactivate gate when boss dies
        if (boss != null)
        {
            float bossHealth = boss.GetComponent<EnemyReceiveDamage>().health;
            if (bossHealth == 0 && gateTilemap != null)
            {
                gateTilemap.SetActive(false);
            }
        }
    }
}
