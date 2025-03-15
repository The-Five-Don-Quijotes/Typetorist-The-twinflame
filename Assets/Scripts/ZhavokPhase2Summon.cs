using UnityEngine;
using UnityEngine.Tilemaps;

public class ZhavokPhase2Summon : MonoBehaviour
{
    private Animator animator;
    private float currentHealth;
    public GameObject summon;
    public Tilemap tilemap; // Reference to the tilemap

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = gameObject.GetComponent<EnemyReceiveDamage>().health;
    }

    void Update()
    {
        currentHealth = gameObject.GetComponent<EnemyReceiveDamage>().health;
        if (currentHealth < 75)
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Summon");
            if (gameObjects.Length < 4)
            {
                DoSummon(gameObjects.Length);
            }
        }
    }

    private void DoSummon(int number)
    {
        animator.SetTrigger("isSummoning");

        // Get tilemap bounds
        BoundsInt bounds = tilemap.cellBounds;

        // Calculate world positions for corners
        Vector3 bottomLeft = tilemap.GetCellCenterWorld(bounds.min);
        Vector3 topLeft = tilemap.GetCellCenterWorld(new Vector3Int(bounds.xMin, bounds.yMax, 0));
        Vector3 bottomRight = tilemap.GetCellCenterWorld(new Vector3Int(bounds.xMax, bounds.yMin, 0));
        Vector3 topRight = tilemap.GetCellCenterWorld(bounds.max);

        // Calculate offset amount (adjustable)
        float offsetX = (bounds.size.x * 0.1f); // 10% inward
        float offsetY = (bounds.size.y * 0.1f); // 10% inward

        // Apply inward offset
        bottomLeft += new Vector3(offsetX, offsetY, 0);
        topLeft += new Vector3(offsetX, -offsetY, 0);
        bottomRight += new Vector3(-offsetX, offsetY, 0);
        topRight += new Vector3(-offsetX, -offsetY, 0);

        // Spawn summons at adjusted positions
        if(number == 0)
        {
            Instantiate(summon, bottomLeft, Quaternion.identity);
        }
        else if(number == 1)
        {
            Instantiate(summon, topLeft, Quaternion.identity);
        }
        else if(number == 2)
        {
            Instantiate(summon, bottomRight, Quaternion.identity);
        }
        else if (number == 3) 
        {
                Instantiate(summon, topRight, Quaternion.identity);
        }
    }
}

