using UnityEngine;

public class PauseManager : MonoBehaviour
{
    // A static instance to make it easy to access from other scripts if needed
    public static bool isPaused = false;

    // Assign your PauseMenuPanel's Animator in the Inspector
    public Animator pauseMenuAnimator;

    void Update()
    {
        // Listen for the Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        // Stop the game time
        Time.timeScale = 0f;

        // Activate the pause menu panel to make it visible
        pauseMenuAnimator.gameObject.SetActive(true);

        // Trigger the opening animation
        pauseMenuAnimator.SetBool("isOpen", true);
    }

    public void ResumeGame()
    {
        isPaused = false;

        // Resume the game time
        Time.timeScale = 1f;

        // Trigger the closing animation
        pauseMenuAnimator.SetBool("isOpen", false);

        // Note: We don't disable the GameObject immediately.
        // We let the animation play out. A small delay or an animation event
        // could be used to disable it after it's hidden, but for most cases,
        // just leaving it active but invisible (alpha=0) is fine.
    }

    public void QuitGame()
    {
        // For testing in the editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        // For a built game
        Application.Quit();
    }
}