using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoProgressSaver : MonoBehaviour
{
    private void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int newLevelValue = GetSceneValue(currentSceneName);
        int currentSavedLevel = PlayerPrefs.GetInt("MaxLevelValue", -1);

        // Check if the current scene is valid (0 to 3) and strictly greater than saved level
        if (newLevelValue > currentSavedLevel && newLevelValue <= 3)
        {
            PlayerPrefs.SetInt("MaxLevelValue", newLevelValue);
            PlayerPrefs.SetString("MaxSceneName", currentSceneName);
            PlayerPrefs.Save();
            Debug.Log("[Save System] Progress saved at: " + currentSceneName);
        }
    }

    private int GetSceneValue(string sceneName)
    {
        // Normalize string to prevent syntax mismatch due to accidental spaces
        string normalizedName = sceneName.Replace(" ", "").ToLower();

        switch (normalizedName)
        {
            case "scene0": return 0;
            case "scene1": return 1;
            case "scene2": return 2;
            case "scene3": return 3;
            default: return -1;
        }
    }
}