using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Typer : MonoBehaviour
{
    public WordBank wordBank = null;
    public TextMeshProUGUI wordOutput = null;
    public TextMeshProUGUI outputLine = null;
    public GameObject Enemy;
    public float damage;

    private string remainingWord = string.Empty;
    private string currentWord = string.Empty;
    private string currentLine = string.Empty;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SetCurrentWord();
        SetCurrentLine();
    }

    private void SetCurrentLine()
    {
        string line = wordBank.GetLine();
        if(line != string.Empty)
        {
            if(line.CompareTo(currentLine) != 0 )
            {
                currentLine = line;
                outputLine.text = currentLine;
                if(Enemy != null && wordBank.currentLineIndex != 0) //if enemy is not death and the line is not the first line
                {
                    Enemy.GetComponent<EnemyReceiveDamage>().DealDamage(damage); //deal damage when finish a senetence
                }
            }
        }
    }

    private void SetCurrentWord()
    {
        currentWord = wordBank.GetWord();
        SetRemainingWord(currentWord);
    }

    private void SetRemainingWord(string remainWord)
    {
        remainingWord = remainWord;
        wordOutput.text = remainingWord;
    }

    // Update is called once per frame
    private void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        if (Input.anyKeyDown)
        {
            string keyPressed = Input.inputString;
            if(keyPressed.Length == 1)
            {
                EnterLetter(keyPressed);
            }
        }
    }

    private void EnterLetter(string typedLetter)
    {
        if (IsCorrectLetter(typedLetter)) 
        {
            RemoveLetter();

            if (IsWordComplete())
            {
                SetCurrentWord();
                SetCurrentLine();
            }
        }
    }

    private bool IsCorrectLetter(string letter)
    {
        return remainingWord.IndexOf(letter) == 0;
    }

    private void RemoveLetter()
    {
        string newString = remainingWord.Remove(0, 1);
        SetRemainingWord(newString);
    }

    private bool IsWordComplete()
    {
        return remainingWord.Length == 0;
    }
}
