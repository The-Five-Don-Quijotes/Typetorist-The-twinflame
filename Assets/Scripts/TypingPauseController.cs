using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public enum PauseMenuActionType { ResumeGame, LoadScene, QuitApplication, OpenOptions, CloseOptions }

[System.Serializable]
public class PauseMenuOption
{
    [Tooltip("The text that will be displayed on screen (e.g., RESUME)")]
    public TextMeshProUGUI displayText;

    [Tooltip("The command the player must type (e.g., 'resume' or 'main menu'). Spaces will be ignored during typing.")]
    public string command;

    [Tooltip("The action to perform when the command is completed")]
    public PauseMenuActionType actionType;

    [Tooltip("Is this command only available when in the Options sub-menu?")]
    public bool isOptionsCommand = false;

    [Tooltip("The name of the scene to load (if action is LoadScene)")]
    public string sceneToLoad;

    [HideInInspector] public string originalText;
    [HideInInspector] public string commandWithoutSpaces;
    [HideInInspector] public int currentProgress = 0;
}

public class TypingPauseController : MonoBehaviour
{
    public static bool isPaused = false;

    [SerializeField] private Animator pauseMenuAnimator;
    [SerializeField] private PauseMenuOption[] menuOptions;
    [SerializeField] private Color typedColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    // NEW: Add a field in the Inspector for your main menu scene's name.
    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // <-- IMPORTANT: Change this to your exact scene name!

    private bool _actionIsQueued = false;
    private PauseMenuOption _queuedOption = null;

    private bool isInOptionsView = false;

    // NEW: Subscribe to the sceneLoaded event when the object is enabled.
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // NEW: Unsubscribe when the object is disabled to prevent memory leaks.
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        InitializeAllDisplays();
        pauseMenuAnimator.gameObject.SetActive(false);
    }

    // NEW: This function runs every time a new scene is finished loading.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the newly loaded scene is the main menu.
        if (scene.name == mainMenuSceneName)
        {
            // If it is, this pause controller is a duplicate and should be destroyed.
            // Also ensure time is running normally.
            Time.timeScale = 1f;
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (_actionIsQueued)
        {
            _actionIsQueued = false;
            PerformAction(_queuedOption);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (isInOptionsView)
                {
                    PerformAction(new PauseMenuOption { actionType = PauseMenuActionType.CloseOptions });
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }

        if (!isPaused) return;

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

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        EventSystem.current.SetSelectedGameObject(null);

        pauseMenuAnimator.gameObject.SetActive(true);
        pauseMenuAnimator.SetBool("isOpen", true);

        isInOptionsView = false;
        if (pauseMenuAnimator.gameObject.activeInHierarchy)
        {
            pauseMenuAnimator.ResetTrigger("ShowOptions");
            pauseMenuAnimator.SetTrigger("HideOptions");
        }

        InitializeAllDisplays();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        EventSystem.current.SetSelectedGameObject(null);

        pauseMenuAnimator.SetBool("isOpen", false);
    }

    public void DisablePausePanel()
    {
        pauseMenuAnimator.gameObject.SetActive(false);
    }

    void HandleCharacterInput(char typedChar)
    {
        foreach (var option in menuOptions)
        {
            if (option.isOptionsCommand != isInOptionsView)
            {
                continue;
            }

            if (option.currentProgress < option.commandWithoutSpaces.Length)
            {
                if (typedChar == option.commandWithoutSpaces[option.currentProgress])
                {
                    option.currentProgress++;
                }
                else
                {
                    option.currentProgress = (typedChar == option.commandWithoutSpaces[0]) ? 1 : 0;
                }
            }
            UpdateTextDisplay(option);

            if (option.currentProgress == option.commandWithoutSpaces.Length && option.command.Length > 0)
            {
                _queuedOption = option;
                _actionIsQueued = true;
            }
        }
    }

    void HandleBackspace()
    {
        foreach (var option in menuOptions)
        {
            if (option.isOptionsCommand != isInOptionsView)
            {
                continue;
            }

            if (option.currentProgress > 0)
            {
                option.currentProgress--;
                UpdateTextDisplay(option);
            }
        }
    }

    void PerformAction(PauseMenuOption option)
    {
        switch (option.actionType)
        {
            case PauseMenuActionType.ResumeGame:
                ResumeGame();
                break;
            case PauseMenuActionType.LoadScene:
                Time.timeScale = 1f;
                SceneManager.LoadScene(option.sceneToLoad);
                break;
            case PauseMenuActionType.QuitApplication:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
            case PauseMenuActionType.OpenOptions:
                pauseMenuAnimator.SetTrigger("ShowOptions");
                isInOptionsView = true;
                EventSystem.current.SetSelectedGameObject(null);
                InitializeAllDisplays();
                break;
            case PauseMenuActionType.CloseOptions:
                pauseMenuAnimator.SetTrigger("HideOptions");
                isInOptionsView = false;
                EventSystem.current.SetSelectedGameObject(null);
                InitializeAllDisplays();
                break;
        }
    }

    void UpdateTextDisplay(PauseMenuOption option)
    {
        string commandToShow = option.originalText;
        string typedHexColor = ColorUtility.ToHtmlStringRGB(typedColor);
        string defaultHexColor = ColorUtility.ToHtmlStringRGB(defaultColor);

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
    }

    void InitializeAllDisplays()
    {
        foreach (var option in menuOptions)
        {
            option.currentProgress = 0;

            if (string.IsNullOrEmpty(option.originalText) && option.displayText != null)
            {
                option.originalText = option.displayText.text;
            }

            option.commandWithoutSpaces = option.command.Replace(" ", "").ToLower();

            if (option.displayText != null)
            {
                string defaultHexColor = ColorUtility.ToHtmlStringRGB(defaultColor);
                option.displayText.text = $"<color=#{defaultHexColor}>{option.originalText}</color>";
            }
        }
    }
}

