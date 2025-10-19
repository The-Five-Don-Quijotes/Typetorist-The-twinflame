using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// 1. Added "Continue" to the list of possible actions.
public enum MenuActionType { LoadScene, QuitApplication, Continue }

[System.Serializable]
public class MenuOption
{
    [Tooltip("The text that will be displayed on screen (e.g., START GAME)")]
    public TextMeshProUGUI displayText;

    [Tooltip("The command the player must type (e.g., 'start')")]
    public string command;

    [Tooltip("The action to perform when the command is completed")]
    public MenuActionType actionType;

    [Tooltip("The name of the scene to load (ignored if action is Quit or Continue)")]
    public string sceneToLoad;

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

    // A reference to the SceneTransition component to avoid finding it repeatedly.
    private SceneTransition sceneTransition;

    void Start()
    {
        // Find the SceneTransition component once and store it.
        sceneTransition = FindFirstObjectByType<SceneTransition>();
        if (sceneTransition == null)
        {
            Debug.LogError("SceneTransition component not found in the scene! Fading will not work.");
        }

        // Initialize all text displays when the game starts
        InitializeAllDisplays();
    }

    void Update()
    {
        // Check for keyboard input every frame
        if (Input.anyKeyDown)
        {
            // Specifically handle backspace to undo progress
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                HandleBackspace();
                return;
            }

            // Get all characters typed this frame (handles different keyboard layouts)
            string typedChars = Input.inputString;

            // Process each character that was typed
            foreach (char c in typedChars)
            {
                // We check for letters, digits, or spaces to allow multi-word commands.
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                {
                    HandleCharacterInput(char.ToLower(c));
                }
            }
        }
    }

    void HandleCharacterInput(char typedChar)
    {
        // Check the typed character against every menu option
        foreach (var option in menuOptions)
        {
            // Is the player still typing this command?
            if (option.currentProgress < option.command.Length)
            {
                // Does the typed character match the *next* expected character?
                if (typedChar == option.command[option.currentProgress])
                {
                    // Correct character! Advance the progress.
                    option.currentProgress++;
                }
                else
                {
                    // Wrong character! Reset progress for this option.
                    // But check if the typed char is the START of the command again.
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
            // Update the visual display for this option
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
        // Use the display text (e.g., "START GAME") as the base for coloring
        string commandToShow = option.originalText;

        // Safety check to prevent errors if progress exceeds text length
        int progress = Mathf.Min(option.currentProgress, commandToShow.Length);

        string typedPart = commandToShow.Substring(0, progress);
        string untypedPart = commandToShow.Substring(progress);

        // Convert colors to hex codes for TextMeshPro rich text
        string typedHexColor = ColorUtility.ToHtmlStringRGB(typedColor);
        string defaultHexColor = ColorUtility.ToHtmlStringRGB(defaultColor);

        // Build the rich text string 
        option.displayText.text = $"<color=#{typedHexColor}>{typedPart}</color><color=#{defaultHexColor}>{untypedPart}</color>";

        // Check if the command is complete
        if (option.currentProgress == option.command.Length)
        {
            // 2. We disabled this component to prevent further typing after a command is completed.
            this.enabled = false;

            // Perform the action defined for this option
            PerformMenuAction(option);
        }
    }

    // 3. Moved the action logic into its own method for clarity.
    void PerformMenuAction(MenuOption option)
    {
        if (sceneTransition == null && option.actionType != MenuActionType.QuitApplication)
        {
            Debug.LogError("Cannot perform scene transition because the SceneTransition component is missing!");
            return;
        }

        switch (option.actionType)
        {
            case MenuActionType.LoadScene:
                Debug.Log($"Command '{option.command}' complete! Loading scene: {option.sceneToLoad}");
                // Use the SceneTransition component for a consistent fade effect
                sceneTransition.LoadSceneWithFade(option.sceneToLoad);
                break;

            case MenuActionType.QuitApplication:
                Debug.Log("Command 'quit' complete! Closing application.");
                Application.Quit();
                break;

            // 4. Added the logic for the new "Continue" action.
            case MenuActionType.Continue:
                Debug.Log($"Command '{option.command}' complete! Loading previous scene.");
                string previousScene = SceneTransition.GetPreviousScene();

                if (!string.IsNullOrEmpty(previousScene))
                {
                    sceneTransition.LoadSceneWithFade(previousScene);
                }
                else
                {
                    Debug.LogWarning("No previous scene found to continue from!");
                    // Re-enable the script if we can't continue, so the player isn't stuck.
                    this.enabled = true;
                }
                break;
        }
    }

    void InitializeAllDisplays()
    {
        foreach (var option in menuOptions)
        {
            // Store the original text (e.g., "NEW GAME") so we can always refer to it.
            option.originalText = option.displayText.text;

            // Make the command lowercase for reliable comparison.
            option.command = option.command.ToLower();

            // Reset progress and update display to default color
            option.currentProgress = 0;
            string defaultHexColor = ColorUtility.ToHtmlStringRGB(defaultColor);
            option.displayText.text = $"<color=#{defaultHexColor}>{option.originalText}</color>";
        }
    }
}
