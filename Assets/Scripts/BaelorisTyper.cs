using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaelorisTyper : MonoBehaviour
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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SetCurrentWord();
        SetCurrentLine();
    }

    private void SetCurrentLine()
    {
        string line = wordBank.GetLine();
        if (line != string.Empty)
        {
            if (line.CompareTo(currentLine) != 0)
            {
                currentLine = line;
                outputLine.text = currentLine;
                if (Enemy != null && wordBank.currentLineIndex != 0) //if enemy is not death and the line is not the first line
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
        HideText();
    }

    private void HideText()
    {
        if(Enemy == null)
        {
            wordOutput.gameObject.SetActive(false);
            outputLine.gameObject.SetActive(false);
        }
    }

    private void CheckInput()
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
                SetCurrentLine();
                currentIndex = 0;
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

    public void ResetLine()
    {
        wordBank.ResetToFirstWordOfCurrentLine();
        currentWord = wordBank.GetWord();
        SetRemainingWord(currentWord);
    }

    private void SpawnDotsEffect()
    {
        if (dotPrefab == null || player == null) return;

        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, wordOutput.transform.position, Quaternion.identity);
        }
    }


    private IEnumerator MoveDotToPlayer(GameObject dot)
    {
        float duration = 0.5f; // Duration of movement
        float elapsedTime = 0f;
        Vector3 startPosition = dot.transform.position;

        // Generate random position around player
        Vector3 targetPosition = player.transform.position + (Vector3)(Random.insideUnitCircle * 1.5f);

        while (elapsedTime < duration)
        {
            dot.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        dot.transform.position = targetPosition;
        Destroy(dot, 0.5f); // Destroy after reaching the target
    }

}
