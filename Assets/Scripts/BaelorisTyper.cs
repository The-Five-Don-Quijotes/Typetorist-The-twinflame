using Assets.Interface;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaelorisTyper : MonoBehaviour, ITyper
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

    private void Start()
    {
        SetCurrentWord();
        SetCurrentLine();
    }

    public void SetCurrentLine()
    {
        string line = wordBank.GetLine();

        if (!string.IsNullOrEmpty(line) && line != currentLine)
        {
            // Execute damage only if transitioning away from an already established line.
            // This safely bypasses the initial startup while correctly triggering on loops.
            if (!string.IsNullOrEmpty(currentLine) && Enemy != null)
            {
                Enemy.GetComponent<EnemyReceiveDamage>().DealDamage(damage);
            }

            currentLine = line;
            outputLine.text = currentLine;
        }
    }

    public void SetCurrentWord()
    {
        currentWord = wordBank.GetWord();

        // Warning: If BaelorisWordBank loops indefinitely, currentWord will never be empty.
        // Phase transitions must be handled by observing boss health thresholds elsewhere,
        // or the WordBank loop logic must be adjusted to pause between phases.
        if (currentWord == string.Empty)
        {
            HandlePhaseTransition();
        }

        SetRemainingWord(currentWord);
    }

    private void HandlePhaseTransition()
    {
        if (Enemy != null)
        {
            Enemy.GetComponent<EnemyReceiveDamage>().DealDamage(damage);
        }

        if (!wordBank.isPhase2)
        {
            wordBank.SetNewLines(wordBank.phase2Lines);
            currentWord = wordBank.GetWord();
        }
    }

    private void SetRemainingWord(string remainWord)
    {
        remainingWord = remainWord;
        wordOutput.text = remainingWord;
    }

    private void Update()
    {
        CheckInput();
        HideText();
    }

    private void HideText()
    {
        if (Enemy == null)
        {
            wordOutput.gameObject.SetActive(false);
            outputLine.gameObject.SetActive(false);
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

            // Execute the Coroutine to initialize dot movement
            StartCoroutine(MoveDotToPlayer(dot));
        }
    }

    private IEnumerator MoveDotToPlayer(GameObject dot)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3 startPosition = dot.transform.position;

        Vector3 targetPosition = player.transform.position + (Vector3)(Random.insideUnitCircle * 1.5f);

        while (elapsedTime < duration)
        {
            // Failsafe in case the dot is destroyed prematurely
            if (dot == null) yield break;

            dot.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (dot != null)
        {
            dot.transform.position = targetPosition;
            Destroy(dot, 0.5f);
        }
    }
}