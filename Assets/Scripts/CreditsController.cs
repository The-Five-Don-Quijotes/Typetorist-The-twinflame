using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign the CreditsContent RectTransform here.")]
    [SerializeField] private RectTransform creditsContent;

    [Header("Scroll Settings")]
    [SerializeField] private float normalScrollSpeed = 75f;
    [SerializeField] private float fastScrollSpeed = 250f;
    [Tooltip("The Y anchored position where the credits sequence ends.")]
    [SerializeField] private float targetYPosition = 3000f;

    [Header("Scene Transition")]
    [SerializeField] private string targetSceneName = "HomeScreen";

    private SceneTransition sceneTransition;
    private bool isTransitioning = false;

    private void Start()
    {
        sceneTransition = FindFirstObjectByType<SceneTransition>();

        // Ensure starting position is below the screen
        if (creditsContent != null)
        {
            creditsContent.anchoredPosition = new Vector2(creditsContent.anchoredPosition.x, -Screen.height);
        }
    }

    private void Update()
    {
        if (isTransitioning || creditsContent == null) return;

        HandleScrolling();
        HandleInput();
    }

    private void HandleScrolling()
    {
        float currentSpeed = Input.GetKey(KeyCode.Space) ? fastScrollSpeed : normalScrollSpeed;
        creditsContent.anchoredPosition += Vector2.up * currentSpeed * Time.deltaTime;

        if (creditsContent.anchoredPosition.y >= targetYPosition)
        {
            EndCreditsSequence();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndCreditsSequence();
        }
    }

    private void EndCreditsSequence()
    {
        isTransitioning = true;

        if (sceneTransition != null)
        {
            sceneTransition.LoadSceneWithFade(targetSceneName);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}