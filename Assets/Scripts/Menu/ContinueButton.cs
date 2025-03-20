using UnityEngine;

public class ContinueButton : MonoBehaviour
{
    public void OnYesButtonPressed()
    {
        string previousScene = SceneTransition.GetPreviousScene();

        if (!string.IsNullOrEmpty(previousScene))
        {
            FindFirstObjectByType<SceneTransition>().LoadSceneWithFade(previousScene);
        }
    }
}
