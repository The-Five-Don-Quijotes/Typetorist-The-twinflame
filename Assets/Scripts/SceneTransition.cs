using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;

    private static string previousScene; // Store previous scene name

    public void LoadSceneWithFade(string sceneName)
    {
        // Save the current scene before switching
        previousScene = SceneManager.GetActiveScene().name;
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        StopAllAudio();
        yield return StartCoroutine(Fade(1)); // Fade out
        SceneManager.LoadScene(sceneName);
        yield return StartCoroutine(Fade(0)); // Fade in
    }

    private void StopAllAudio()
    {
        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource audio in audioSources)
        {
            if (audio.gameObject.CompareTag("GameManager")) // Check if it's from GameManager
            {
                if (audio.clip != null && audio.clip.name == "PlayerDeath")
                    continue; // Skip stopping "PlayerDeath" sound
            }

            audio.Stop();
        }
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvas.alpha;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = targetAlpha;
    }

    public static string GetPreviousScene()
    {
        return previousScene;
    }
}
