using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

// Added LoadSavedGame to handle reading from PlayerPrefs
public enum MenuActionType { LoadScene, QuitApplication, Continue, TransitionPanel, LoadSavedGame }

[System.Serializable]
public class MenuOption
{
    [Tooltip("The text that will be displayed on screen (e.g., START GAME)")]
    public TextMeshProUGUI displayText;

    [Tooltip("The command the player must type (e.g., 'start')")]
    public string command;

    [Tooltip("The action to perform when the command is completed")]
    public MenuActionType actionType;

    [Tooltip("The name of the scene to load (ignored if action is Quit, Continue, LoadSavedGame, or TransitionPanel)")]
    public string sceneToLoad;

    [Header("Panel Transition Settings")]
    [Tooltip("The CanvasGroup to fade IN when this command is typed")]
    public CanvasGroup panelToFadeIn;

    [Tooltip("The CanvasGroup to fade OUT when this command is typed")]
    public CanvasGroup panelToFadeOut;

    [Tooltip("Duration of the fade transition in seconds")]
    public float fadeDuration = 0.5f;

    [HideInInspector]
    public string originalText;

    [HideInInspector]
    public int currentProgress = 0;
}

public class TypingMenuController : MonoBehaviour
{
    [Tooltip("List of all menu options available on this screen")]
    public MenuOption[] menuOptions;

    [Tooltip("The color for text that has been correctly typed")]
    public Color typedColor = Color.green;

    [Tooltip("The color for text that is yet to be typed")]
    public Color defaultColor = Color.white;

    private SceneTransition sceneTransition;

    void Start()
    {
        sceneTransition = FindFirstObjectByType<SceneTransition>();
        if (sceneTransition == null)
        {
            Debug.LogError("SceneTransition component not found in the scene! Scene fading will not work.");
        }

        InitializeAllDisplays();
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                HandleBackspace();
                return;
            }

            string typedChars = Input.inputString;

            foreach (char c in typedChars)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                {
                    HandleCharacterInput(char.ToLower(c));
                }
            }
        }
    }

    void HandleCharacterInput(char typedChar)
    {
        foreach (var option in menuOptions)
        {
            if (option.currentProgress < option.command.Length)
            {
                if (typedChar == option.command[option.currentProgress])
                {
                    option.currentProgress++;
                }
                else
                {
                    if (typedChar == option.command[0])
                    {
                        option.currentProgress = 1;
                    }
                    else
                    {
                        option.currentProgress = 0;
                    }
                }
            }
            UpdateTextDisplay(option);
        }
    }

    void HandleBackspace()
    {
        foreach (var option in menuOptions)
        {
            if (option.currentProgress > 0)
            {
                option.currentProgress--;
                UpdateTextDisplay(option);
            }
        }
    }

    void UpdateTextDisplay(MenuOption option)
    {
        string commandToShow = option.originalText;
        int progress = Mathf.Min(option.currentProgress, commandToShow.Length);

        string typedPart = commandToShow.Substring(0, progress);
        string untypedPart = commandToShow.Substring(progress);

        string typedHexColor = ColorUtility.ToHtmlStringRGB(typedColor);
        string defaultHexColor = ColorUtility.ToHtmlStringRGB(defaultColor);

        option.displayText.text = $"<color=#{typedHexColor}>{typedPart}</color><color=#{defaultHexColor}>{untypedPart}</color>";

        if (option.currentProgress == option.command.Length)
        {
            // Disable input while executing action
            this.enabled = false;
            PerformMenuAction(option);
        }
    }

    void PerformMenuAction(MenuOption option)
    {
        switch (option.actionType)
        {
            case MenuActionType.LoadScene:
                if (sceneTransition != null) sceneTransition.LoadSceneWithFade(option.sceneToLoad);
                break;

            case MenuActionType.QuitApplication:
                Application.Quit();
                break;

            case MenuActionType.Continue:
                string previousScene = SceneTransition.GetPreviousScene();
                if (!string.IsNullOrEmpty(previousScene) && sceneTransition != null)
                {
                    sceneTransition.LoadSceneWithFade(previousScene);
                }
                else
                {
                    Debug.LogWarning("No previous scene found!");
                    this.enabled = true;
                }
                break;

            case MenuActionType.LoadSavedGame:
                // Changed default fallback to Scene0
                string savedScene = PlayerPrefs.GetString("MaxSceneName", "Scene0");
                if (sceneTransition != null)
                {
                    sceneTransition.LoadSceneWithFade(savedScene);
                }
                else
                {
                    SceneManager.LoadScene(savedScene);
                }
                break;

            case MenuActionType.TransitionPanel:
                // Start the coroutine to fade panels smoothly
                StartCoroutine(FadePanelsCoroutine(option));
                break;
        }
    }

    private IEnumerator FadePanelsCoroutine(MenuOption option)
    {
        float elapsedTime = 0f;

        // Pre-setup: ensure the incoming panel is active but transparent
        if (option.panelToFadeIn != null)
        {
            option.panelToFadeIn.gameObject.SetActive(true);
            option.panelToFadeIn.alpha = 0f;
            option.panelToFadeIn.blocksRaycasts = true;
            option.panelToFadeIn.interactable = true;
        }

        // Pre-setup: disable interactions on the outgoing panel immediately
        if (option.panelToFadeOut != null)
        {
            option.panelToFadeOut.blocksRaycasts = false;
            option.panelToFadeOut.interactable = false;
        }

        // Execute crossfade
        while (elapsedTime < option.fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / option.fadeDuration;

            if (option.panelToFadeIn != null)
                option.panelToFadeIn.alpha = Mathf.Lerp(0f, 1f, t);

            if (option.panelToFadeOut != null)
                option.panelToFadeOut.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        // Post-setup: finalize states
        if (option.panelToFadeIn != null)
            option.panelToFadeIn.alpha = 1f;

        if (option.panelToFadeOut != null)
        {
            option.panelToFadeOut.alpha = 0f;
            option.panelToFadeOut.gameObject.SetActive(false);
        }

        // Reset all typing variables so incomplete commands do not persist between menus
        ResetAllProgress();

        // Restore input
        this.enabled = true;
    }

    void InitializeAllDisplays()
    {
        foreach (var option in menuOptions)
        {
            option.originalText = option.displayText.text;
            option.command = option.command.ToLower();
        }
        ResetAllProgress();
    }

    void ResetAllProgress()
    {
        foreach (var option in menuOptions)
        {
            option.currentProgress = 0;
            string defaultHexColor = ColorUtility.ToHtmlStringRGB(defaultColor);
            option.displayText.text = $"<color=#{defaultHexColor}>{option.originalText}</color>";
        }
    }
}