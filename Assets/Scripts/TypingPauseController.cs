using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// MODIFIED: Renamed the enum to be specific to the pause menu.
public enum PauseMenuActionType { ResumeGame, LoadScene, QuitApplication }

// The MenuOption class is mostly the same, just uses the new enum.
[System.Serializable]
public class PauseMenuOption
{
    [Tooltip("The text that will be displayed on screen (e.g., RESUME)")]
    public TextMeshProUGUI displayText;

    [Tooltip("The command the player must type (e.g., 'resume' or 'main menu'). Spaces will be ignored during typing.")]
    public string command;

    [Tooltip("The name of the scene to load (if action is LoadScene)")]
    public string sceneToLoad;

    [Tooltip("The action to perform when the command is completed")]
    public PauseMenuActionType actionType;

    [HideInInspector]
    public string originalText;

    // NEW: We'll store the command without spaces for easier comparison.
    [HideInInspector]
    public string commandWithoutSpaces;

    [HideInInspector]
    public int currentProgress = 0; // This will now track progress on the spaceless command.
}

public class TypingPauseController : MonoBehaviour
{
    // NEW: A static bool to let other scripts know if the game is paused.
    public static bool isPaused = false;

    [Tooltip("Assign your Pause Menu UI Panel's Animator here.")]
    public Animator pauseMenuAnimator;

    [Tooltip("List of all menu options available on this screen")]
    public PauseMenuOption[] menuOptions;

    [Tooltip("The color for text that has been correctly typed")]
    public Color typedColor = Color.green;

    [Tooltip("The color for text that is yet to be typed")]
    public Color defaultColor = Color.white;

    void Start()
    {
        // Make sure the menu is initialized and hidden on start.
        InitializeAllDisplays();
        pauseMenuAnimator.gameObject.SetActive(false);
    }

    void Update()
    {
        // NEW: This is the master toggle for the pause menu.
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

        // NEW: Only process typing input if the game is paused.
        if (!isPaused)
        {
            return;
        }

        // Your existing input handling logic, now running only when paused.
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
                if (char.IsLetterOrDigit(c))
                {
                    HandleCharacterInput(char.ToLower(c));
                }
            }
        }
    }

    // NEW: Function to pause the game and show the menu.
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // This freezes the game!
        pauseMenuAnimator.gameObject.SetActive(true);
        pauseMenuAnimator.SetBool("isOpen", true);
        InitializeAllDisplays(); // Reset typing progress every time we open the menu.
    }

    // NEW: Function to resume the game and hide the menu.
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // This unfreezes the game!
        pauseMenuAnimator.SetBool("isOpen", false);
    }

    void HandleCharacterInput(char typedChar)
    {
        // NEW: We'll check for a completed option but won't act on it until the loop is finished.
        PauseMenuOption completedOption = null;

        foreach (var option in menuOptions)
        {
            // MODIFIED: We compare against the command *without* spaces.
            if (option.currentProgress < option.commandWithoutSpaces.Length)
            {
                if (typedChar == option.commandWithoutSpaces[option.currentProgress])
                {
                    option.currentProgress++;
                }
                else
                {
                    // Reset logic is slightly simpler now.
                    option.currentProgress = (typedChar == option.commandWithoutSpaces[0]) ? 1 : 0;
                }
            }
            UpdateTextDisplay(option);

            // NEW: If this option is now complete, remember it.
            if (option.currentProgress == option.commandWithoutSpaces.Length)
            {
                completedOption = option;
            }
        }

        // NEW: Now that the loop is done, perform the action if one was completed.
        if (completedOption != null)
        {
            PerformAction(completedOption);
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

    void UpdateTextDisplay(PauseMenuOption option)
    {
        string commandToShow = option.originalText;
        string typedHexColor = ColorUtility.ToHtmlStringRGB(typedColor);
        string defaultHexColor = ColorUtility.ToHtmlStringRGB(defaultColor);

        // MODIFIED: This is the new logic to handle spaces correctly in the display.
        int splitIndex = 0;
        int nonSpaceCharsCounted = 0;
        for (int i = 0; i < commandToShow.Length && nonSpaceCharsCounted < option.currentProgress; i++)
        {
            if (commandToShow[i] != ' ' && commandToShow[i] != '\n' && commandToShow[i] != '\r')
            {
                nonSpaceCharsCounted++;
            }
            splitIndex = i + 1;
        }
        if (option.currentProgress == 0) { splitIndex = 0; }

        string typedPart = commandToShow.Substring(0, splitIndex);
        string untypedPart = commandToShow.Substring(splitIndex);

        option.displayText.text = $"<color=#{typedHexColor}>{typedPart}</color><color=#{defaultHexColor}>{untypedPart}</color>";

        // MODIFIED: The action-triggering logic has been REMOVED from this function.
    }

    // NEW: A dedicated function to perform the action.
    void PerformAction(PauseMenuOption option)
    {
        // We must un-pause before loading a new scene or quitting.
        Time.timeScale = 1f;

        switch (option.actionType)
        {
            case PauseMenuActionType.ResumeGame:
                // We call ResumeGame directly. It handles the Time.timeScale and animations.
                ResumeGame();
                break;
            case PauseMenuActionType.LoadScene:
                SceneManager.LoadScene(option.sceneToLoad);
                break;
            case PauseMenuActionType.QuitApplication:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    void InitializeAllDisplays()
    {
        foreach (var option in menuOptions)
        {
            option.currentProgress = 0;

            // BUG FIX: Only store the original text if we haven't already.
            // This prevents it from being corrupted with rich text tags on subsequent pauses.
            if (string.IsNullOrEmpty(option.originalText))
            {
                option.originalText = option.displayText.text;
            }

            // NEW: Prepare the spaceless version of the command.
            option.commandWithoutSpaces = option.command.Replace(" ", "").ToLower();

            string defaultHexColor = ColorUtility.ToHtmlStringRGB(defaultColor);
            option.displayText.text = $"<color=#{defaultHexColor}>{option.originalText}</color>";
        }
    }
}