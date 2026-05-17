using UnityEngine;

public class PortalController : MonoBehaviour
{
    [SerializeField] private GameObject boss;
    public GameObject portal;

    void Start()
    {
        if (portal != null)
            portal.SetActive(false);
    }

    void Update()
    {
        //if (boss == null || boss.GetComponent<Animator>().GetBool("isDeath"))
        if (boss == null)
        {
            if (portal != null && !portal.activeSelf)
            {
                portal.SetActive(true);
            }
        }
    }
}
