using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTeleport : MonoBehaviour
{
    [SerializeField] private string sceneName; // Assign the scene in the Inspector

    void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
