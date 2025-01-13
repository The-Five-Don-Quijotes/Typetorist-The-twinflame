using System.Collections.Generic;
using UnityEngine;

public class WordBank : MonoBehaviour
{
    private List<string> originalLines = new List<string>()
    {
        "Darkness beyond blackest pitch",
        "Deeper than the deepest night",
        "Lord as vast as the largest ocean",
        "Colder than the coldest ice",
        "King of Darkness who shines like gold upon the Sea of Chaos",
        "I call upon thee and swear myself to thee",
        "I stand ready to bear the strength you give me",
        "Let the fools who stand before me be destroyed",
        "By the power you and I possess"
    };

    private Queue<(string word, bool isFirstWord)> wordQueue = new Queue<(string, bool)>();
    private int currentLineIndex = 0;

    private void Awake()
    {
        PopulateWordQueue();
    }

    private void PopulateWordQueue()
    {
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

            // Update currentLineIndex only when the first word of the next line is dequeued
            if (isFirstWord && wordQueue.Count > 0)
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
}
