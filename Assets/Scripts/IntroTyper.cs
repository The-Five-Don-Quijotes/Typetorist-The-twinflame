using Assets.Interface;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroTyper : MonoBehaviour, ITyper
{
    public BaelorisWordBank wordBank = null;
    public TextMeshProUGUI wordOutput = null;
    public TextMeshProUGUI outputLine = null;
    public GameObject Enemy;
    public float damage;

    private string remainingWord = string.Empty;
    private string currentWord = string.Empty;
    private string currentLine = string.Empty;
    private int currentIndex = 0;

    public GameObject dotPrefab;
    public GameObject player;
    public int dotCount = 15;

    // --- NEW: Flag to track when all lines are completely typed ---
    public bool IsTypingSequenceComplete { get; private set; } = false;

    private void Start()
    {
        SetCurrentWord();
        SetCurrentLine();
    }

    public void SetCurrentLine()
    {
        string line = wordBank.GetLine();
        if (line != string.Empty)
        {
            if (line.CompareTo(currentLine) != 0)
            {
                currentLine = line;
                outputLine.text = currentLine;
                if (Enemy != null && wordBank.currentLineIndex != 0)
                {
                    Enemy.GetComponent<EnemyReceiveDamage>().DealDamage(damage);
                }
            }
        }
    }

    public void SetCurrentWord()
    {
        currentWord = wordBank.GetWord();

        // --- NEW: Detect if there are no more words ---
        if (string.IsNullOrEmpty(currentWord))
        {
            IsTypingSequenceComplete = true;
            wordOutput.text = "";
            outputLine.text = "";
            return;
        }

        SetRemainingWord(currentWord);
    }

    private void SetRemainingWord(string remainWord)
    {
        remainingWord = remainWord;
        wordOutput.text = remainingWord;
    }

    private void Update()
    {
        if (IsTypingSequenceComplete) return; // Stop processing if done
        CheckInput();
        HideText();
    }

    private void HideText()
    {
        if (Enemy == null)
        {
            // Note: In a tutorial without an enemy, you may want to comment this out 
            // or assign a dummy enemy so the text doesn't hide immediately.
            // wordOutput.gameObject.SetActive(false);
            // outputLine.gameObject.SetActive(false);
        }
    }

    public void CheckInput()
    {
        if (Input.anyKeyDown)
        {
            string keyPressed = Input.inputString;
            if (keyPressed.Length == 1)
            {
                EnterLetter(keyPressed);
            }
        }
    }

    private void EnterLetter(string typedLetter)
    {
        if (currentIndex < currentWord.Length && typedLetter == currentWord[currentIndex].ToString())
        {
            currentIndex++;
            UpdateRemainingWord();

            if (currentIndex == currentWord.Length)
            {
                SpawnDotsEffect();
                SetCurrentWord();

                if (!IsTypingSequenceComplete)
                {
                    SetCurrentLine();
                    currentIndex = 0;
                }
            }
        }
        else if (currentIndex > 0)
        {
            currentIndex--;
            UpdateRemainingWord();
        }
    }

    private void UpdateRemainingWord()
    {
        remainingWord = currentWord.Substring(currentIndex);
        wordOutput.text = remainingWord;
    }

    public void ResetLine()
    {
        wordBank.ResetToFirstWordOfCurrentLine();
        currentWord = wordBank.GetWord();
        SetRemainingWord(currentWord);
        IsTypingSequenceComplete = false;
    }

    private void SpawnDotsEffect()
    {
        if (dotPrefab == null || player == null) return;
        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, wordOutput.transform.position, Quaternion.identity);
            // Assuming you have a script on the dot that handles the movement, 
            // otherwise StartCoroutine(MoveDotToPlayer(dot)) should be called here.
        }
    }
}