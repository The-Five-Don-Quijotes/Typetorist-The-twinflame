using UnityEngine;

public class PortalController : MonoBehaviour
{
    private GameObject boss;
    public GameObject portal;

    void Start()
    {
        boss = GameObject.FindWithTag("Boss"); // Find the boss
    }

    void Update()
    {
        //if (boss == null || boss.GetComponent<Animator>().GetBool("isDeath"))
        if(boss == null)
        {
            if(portal != null)
                portal.SetActive(true); // Activate Portal when boss dies
        }
    }
}
