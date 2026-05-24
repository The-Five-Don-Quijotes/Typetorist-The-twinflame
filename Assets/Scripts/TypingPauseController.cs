using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public enum PauseMenuActionType { ResumeGame, LoadScene, QuitApplication, OpenOptions, CloseOptions }

[System.Serializable]
public class PauseMenuOption
{
    public TextMeshProUGUI displayText;
    public string command;
    public PauseMenuActionType actionType;
    public bool isOptionsCommand = false;
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

    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Secret God Mode Integration")]
    [SerializeField] private string secretGodModeCode = "baoaxid";
    [SerializeField] private GameObject godModeUIContainer;
    [SerializeField] private UnityEngine.UI.Toggle godModeToggle;

    private bool _actionIsQueued = false;
    private PauseMenuOption _queuedOption = null;

    private bool isInOptionsView = false;
    private int secretCodeProgress = 0;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        InitializeAllDisplays();
        pauseMenuAnimator.gameObject.SetActive(false);

        if (godModeUIContainer != null)
        {
            godModeUIContainer.SetActive(false);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainMenuSceneName)
        {
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
        secretCodeProgress = 0;

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
        // Process UI Commands
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

        // Process Secret God Mode Input isolated to the Options view
        if (isInOptionsView && !string.IsNullOrEmpty(secretGodModeCode))
        {
            if (typedChar == secretGodModeCode[secretCodeProgress])
            {
                secretCodeProgress++;
                if (secretCodeProgress == secretGodModeCode.Length)
                {
                    ToggleGodModeState();
                    secretCodeProgress = 0;
                }
            }
            else
            {
                secretCodeProgress = (typedChar == secretGodModeCode[0]) ? 1 : 0;
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

        if (isInOptionsView && secretCodeProgress > 0)
        {
            secretCodeProgress--;
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
                secretCodeProgress = 0;
                EventSystem.current.SetSelectedGameObject(null);
                InitializeAllDisplays();
                break;
            case PauseMenuActionType.CloseOptions:
                pauseMenuAnimator.SetTrigger("HideOptions");
                isInOptionsView = false;
                secretCodeProgress = 0;
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

    private void ToggleGodModeState()
    {
        if (godModeUIContainer == null) return;

        bool targetState = !godModeUIContainer.activeSelf;

        // Toggle UI visibility
        godModeUIContainer.SetActive(targetState);

        // Sync toggle component
        if (godModeToggle != null)
        {
            godModeToggle.SetIsOnWithoutNotify(targetState);
        }

        // Apply God Mode to PlayerStats
        if (PlayerStats.playerStats != null)
        {
            PlayerStats.playerStats.isGodMode = targetState;
        }
    }

    // Assign this method to the God Mode UI Toggle's OnValueChanged event in the Inspector
    public void OnGodModeToggleChanged(bool isOn)
    {
        if (PlayerStats.playerStats != null)
        {
            PlayerStats.playerStats.isGodMode = isOn;
        }
    }
}