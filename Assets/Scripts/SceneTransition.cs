using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(Fade(1)); // Fade out
        SceneManager.LoadScene(sceneName);
        yield return StartCoroutine(Fade(0)); // Fade in
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
}
