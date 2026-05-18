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

    public void OpenPortal()
    {
        if (portal != null && !portal.activeSelf)
        {
            portal.SetActive(true);
        }
    }
}
