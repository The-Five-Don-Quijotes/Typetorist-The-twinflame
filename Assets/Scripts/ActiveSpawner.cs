using System;
using UnityEngine;

public class ActiveSpawner : MonoBehaviour
{
    private GameObject Spawner;
    private GameObject Spawner1;
    private GameObject Spawner2;
    private GameObject Spawner3;
    private GameObject Spawner4;

    void Start()
    {
        // Find objects in the scene (even if inactive)
        Spawner = GameObject.Find("BulletSpawner");
        Spawner1 = GameObject.Find("BulletSpawner (1)");
        Spawner2 = GameObject.Find("BulletSpawner (2)");
        Spawner3 = GameObject.Find("BulletSpawner (3)");
        Spawner4 = GameObject.Find("BulletSpawner (4)");

        // Ensure they are found before setting active
        if (Spawner != null) Spawner.SetActive(false);
        if (Spawner1 != null) Spawner1.SetActive(false);
        if (Spawner2 != null) Spawner2.SetActive(false);
        if (Spawner3 != null) Spawner3.SetActive(false);
        if (Spawner4 != null) Spawner4.SetActive(false);
    }

    void Update()
    {
        GameObject boss = GameObject.Find("Baeloris");
        if (boss != null)
        {
            float bossHealth = boss.GetComponent<EnemyReceiveDamage>().health;

            bool shouldActivate = bossHealth == 75 || bossHealth == 25;
            bool shouldDeactivate = bossHealth == 50 || bossHealth == 0;

            // Activate or deactivate spawners based on health
            if (Spawner != null) Spawner.SetActive(shouldActivate && !shouldDeactivate);
            if (Spawner1 != null) Spawner1.SetActive(shouldActivate && !shouldDeactivate);
            if (Spawner2 != null) Spawner2.SetActive(shouldActivate && !shouldDeactivate);
            if (Spawner3 != null) Spawner3.SetActive(shouldActivate && !shouldDeactivate);
            if (Spawner4 != null) Spawner4.SetActive(shouldActivate && shouldDeactivate);
        }
    }
}
