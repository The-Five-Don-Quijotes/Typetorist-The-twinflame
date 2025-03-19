using Unity.VisualScripting;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    private BoxCollider2D laserCollider;
    public GameObject LaserBody;
    public GameObject LaserBody1;
    public GameObject LaserBody2;
    public GameObject LaserBody3;
    public GameObject LaserBody4;

    private GameObject boss;

    void Awake()
    {
        // Get the BoxCollider on the parent Laser object
        laserCollider = GetComponent<BoxCollider2D>();
        boss = GameObject.FindGameObjectWithTag("Boss");
    }

    public void EnableLaser()
    {
        if (laserCollider != null)
            laserCollider.enabled = true;

        LaserBody.SetActive(true);
        LaserBody1.SetActive(true);
        LaserBody2.SetActive(true);
        LaserBody3.SetActive(true);
        LaserBody4.SetActive(true);
    }

    public void DisableLaser()
    {
        if (laserCollider != null)
            laserCollider.enabled = false;

        LaserBody.SetActive(false);
        LaserBody1.SetActive(false);
        LaserBody2.SetActive(false);
        LaserBody3.SetActive(false);
        LaserBody4.SetActive(false);
    }

    public void DestroyLaser()
    {
        if(boss != null)
        {
            boss.GetComponent<Animator>().SetBool("isShootingLaser", false);
        }
        Destroy(gameObject);
    }
}
