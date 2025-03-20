using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTeleport : MonoBehaviour
{
    public string sceneName; // Assign the scene in the Inspector

    void Start()
    {
        gameObject.SetActive(false);
        string currentScene = SceneManager.GetActiveScene().name;
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
