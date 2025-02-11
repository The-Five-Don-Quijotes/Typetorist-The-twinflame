using System.Collections.Generic;
using UnityEngine;

public class WordBank : MonoBehaviour
{
    private List<string> originalLines = new List<string>()
    {
        "witness the cross of the lord flee away ye hostiles",
        "may thy mercy o lord rest upon us for we have hope",
        "the lion of the tribe of judah the root of david hath conquered",
        "we cast you out every unclean spirit every satanic power and onslaught",
        "cursed dragon we adjure you may the lord be with you and with thy spirit amen"
    };

    private Queue<(string word, bool isFirstWord)> wordQueue = new Queue<(string, bool)>();
    public int currentLineIndex = -1;

    private void Awake()
    {
        PopulateWordQueue();
    }

    private void PopulateWordQueue()
    {
        wordQueue.Clear();
        foreach (string line in originalLines)
        {
            string[] words = line.Split(' '); // Split the line into words
            for (int i = 0; i < words.Length; i++)
            {
                // Add each word to the queue and mark the first word of each line
                wordQueue.Enqueue((words[i].ToLower(), i == 0));
            }
        }
    }

    public string GetWord()
    {
        if (wordQueue.Count > 0)
        {
            var (nextWord, isFirstWord) = wordQueue.Dequeue();

            // Increment the line index only if this is the first word of the next line
            if (isFirstWord)
            {
                currentLineIndex++;
            }

            Debug.Log($"Returning word: {nextWord}, IsFirstWord: {isFirstWord}");
            return nextWord;
        }
        else
        {
            Debug.LogWarning("No more words available.");
            return string.Empty;
        }
    }

    public string GetLine()
    {
        if (currentLineIndex < originalLines.Count)
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
