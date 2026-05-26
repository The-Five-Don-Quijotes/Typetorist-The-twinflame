using System.Collections.Generic;
using UnityEngine;

public class BaelorisWordBank : MonoBehaviour
{
    public List<string> originalLines = new List<string>()
    {
        "witness the cross of the lord flee away ye hostiles may thy mercy o lord rest upon us for we have hope",
        "the lion of the tribe of judah the root of david hath conquered we cast you out every unclean spirit",
        "every satanic power and onslaught cursed dragon we adjure you by his might you shall not stand",
        "may the lord be with us our refuge and shield his light drives out the darkness",
        "let his name reign forever in him we find our salvation amen"
    };

    public List<string> phase2Lines = new List<string>()
    {
        "behold the ancient enemy the murderer raising his head yet the lords army shall fight his battles",
        "as once thou didst fight the first apostate so shall he prevail the forces of light shall not be overcome",
        "where the seat of peter is settled let truth shine for the nations darkness shall not consume it",
        "the behemoth is cast down the serpent deceives no more the lord rebukes the prince of lies",
        "pray to the god of peace to crush satans reign may he harm the innocent no more amen"
    };

    private Queue<(string word, bool isFirstWord)> wordQueue = new Queue<(string, bool)>();
    public int currentLineIndex = -1;

    // Add boolean lock for Phase state
    public bool isPhase2 = false;
    public bool isLooping = false;

    private void Awake()
    {
        isPhase2 = false; // Reset on start
        PopulateWordQueue();
    }

    public void PopulateWordQueue()
    {
        wordQueue.Clear();
        foreach (string line in originalLines)
        {
            // Trim to prevent trailing spaces from creating empty string artifacts
            string[] words = line.Trim().Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                wordQueue.Enqueue((words[i].ToLower(), i == 0));
            }
        }
    }

    public void SetNewLines(List<string> newLines)
    {
        originalLines = newLines;
        currentLineIndex = -1;
        isPhase2 = true; // Lock the state
        PopulateWordQueue();
    }

    public string GetWord()
    {
        // Loop back to the beginning if the queue is empty
        if (wordQueue.Count == 0)
        {
            // Halt execution and return empty if looping is restricted
            if (!isLooping)
            {
                return string.Empty;
            }

            currentLineIndex = -1;
            PopulateWordQueue();

            // Failsafe in case the list is completely empty
            if (wordQueue.Count == 0)
            {
                Debug.LogWarning("Original lines list is empty. Cannot fetch words.");
                return string.Empty;
            }
        }

        var (nextWord, isFirstWord) = wordQueue.Dequeue();

        if (isFirstWord)
        {
            currentLineIndex++;
        }

        return nextWord;
    }

    public string GetLine()
    {
        if (currentLineIndex >= 0 && currentLineIndex < originalLines.Count)
        {
            string line = originalLines[currentLineIndex];
            return line;
        }

        Debug.LogWarning("No more lines available.");
        return string.Empty;
    }

    public void ResetToFirstWordOfCurrentLine()
    {
        if (currentLineIndex >= 0 && currentLineIndex < originalLines.Count)
        {
            Queue<(string word, bool isFirstWord)> newQueue = new Queue<(string, bool)>();
            string currentLine = originalLines[currentLineIndex];
            string[] words = currentLine.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                newQueue.Enqueue((words[i].ToLower(), false));
            }

            for (int i = currentLineIndex + 1; i < originalLines.Count; i++)
            {
                string[] lineWords = originalLines[i].Split(' ');
                for (int j = 0; j < lineWords.Length; j++)
                {
                    newQueue.Enqueue((lineWords[j].ToLower(), j == 0));
                }
            }

            wordQueue = newQueue;
            Debug.Log("Reset to first word of current line: " + currentLine);
        }
        else
        {
            Debug.LogWarning("Cannot reset: currentLineIndex is invalid.");
        }
    }
}