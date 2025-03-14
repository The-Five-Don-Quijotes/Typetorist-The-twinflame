using UnityEngine;

public class ActiveSpawner : MonoBehaviour
{
    public GameObject Spawner;
    public GameObject Spawner1;
    public GameObject Spawner2;
    public GameObject Spawner3;
    void Start()
    {
        Spawner.SetActive(false);
        Spawner1.SetActive(false);
        Spawner2.SetActive(false);
        Spawner3.SetActive(false);
    }

    void Update()
    {
        GameObject boss = GameObject.Find("Baeloris");
        if (boss != null)
        {
            float bossHealth = boss.GetComponent<EnemyReceiveDamage>().health;
            if (bossHealth <= 75 && bossHealth > 50)
            {
                Spawner.SetActive(true);
                Spawner1.SetActive(true);
                Spawner2.SetActive(true);
                Spawner3.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Baeloris not found in the scene.");
        }
    }
}
