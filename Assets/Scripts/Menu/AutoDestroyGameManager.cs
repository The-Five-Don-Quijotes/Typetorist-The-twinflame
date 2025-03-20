using UnityEngine;

public class AutoDestroyGameManager : MonoBehaviour
{
    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (playerStats == null)
        {
            Destroy(gameObject);
        }
    }
}
