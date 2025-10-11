using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum MenuActionType { LoadScene, QuitApplication }

[System.Serializable]
public class MenuOption
{
    [Tooltip("The text that will be displayed on screen (e.g., START GAME)")]
    public TextMeshProUGUI displayText;

    [Tooltip("The command the player must type (e.g., 'start')")]
    public string command;

    [Tooltip("The name of the scene to load when the command is completed")]
    public string sceneToLoad;

    [Tooltip("The action to perform when the command is completed")]
    public MenuActionType actionType;

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

    void Start()
    {
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
                // Ignore special keys like 'enter'
                if (char.IsLetterOrDigit(c))
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
        string commandToShow = option.originalText; // Use the display text as base
        string typedPart = commandToShow.Substring(0, option.currentProgress);
        string untypedPart = commandToShow.Substring(option.currentProgress);

        // Convert colors to hex codes for TextMeshPro rich text
        string typedHexColor = ColorUtility.ToHtmlStringRGB(typedColor);
        string defaultHexColor = ColorUtility.ToHtmlStringRGB(defaultColor);

        // Build the rich text string 
        option.displayText.text = $"<color=#{typedHexColor}>{typedPart}</color><color=#{defaultHexColor}>{untypedPart}</color>";

        // Check if the command is complete
        if (option.currentProgress == option.command.Length)
{
    // Perform the action defined for this option
    switch (option.actionType)
    {
        case MenuActionType.LoadScene:
            Debug.Log($"Command '{option.command}' complete! Loading scene: {option.sceneToLoad}");
            SceneManager.LoadScene(option.sceneToLoad);
            break;

        case MenuActionType.QuitApplication:
            Debug.Log("Command 'quit' complete! Closing application.");
            Application.Quit();
            break;
    }
}
    }

    void InitializeAllDisplays()
    {
        foreach (var option in menuOptions)
        {
            // Reset progress and update display to default color
            option.currentProgress = 0;
            option.originalText = option.displayText.text;
            // The display text can be different from the command, so we use its length
            string commandToShow = option.displayText.text;
            string defaultHexColor = ColorUtility.ToHtmlStringRGB(defaultColor);
            option.displayText.text = $"<color=#{defaultHexColor}>{commandToShow}</color>";
        }
    }
}