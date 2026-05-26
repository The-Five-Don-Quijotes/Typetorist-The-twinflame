using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class PortalTeleport : MonoBehaviour
{
    [Header("Portal Configuration")]
    public string sceneName;
    public float safeDistance = 2.0f;

    private bool isArmed = false;
    private Transform player;

    private void Awake()
    {
        // Cache the player reference on initialization
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void OnEnable()
    {
        // Reset armed state automatically when portal becomes active
        isArmed = false;
    }

    private void Update()
    {
        if (isArmed || player == null) return;

        // Arm the portal only when the player moves completely outside the safe distance
        if (Vector3.Distance(transform.position, player.position) > safeDistance)
        {
            isArmed = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore collision events entirely if the portal is not yet armed
        if (!isArmed) return;

        if (collision.CompareTag("Player"))
        {
            SceneTransition sceneTransition = FindFirstObjectByType<SceneTransition>();
            if (sceneTransition != null && !string.IsNullOrEmpty(sceneName))
            {
                sceneTransition.LoadSceneWithFade(sceneName);
            }
            else
            {
                Debug.LogWarning("SceneTransition is missing or sceneName is not set!");
            }
        }
    }
}