using System.Collections.Generic;
using UnityEngine;

public class BaelorisWordBank : MonoBehaviour
{
    public List<string> originalLines = new List<string>()
    {
        "witness the cross of the lord flee away ye hostiles may thy mercy o lord",
        "rest upon us for we have hope the lion of the tribe of judah",
        "the root of david hath conquered we cast you out every unclean spirit",
        "every satanic power and onslaught cursed dragon we adjure",
        "you may the lord be with you and with thy spirit amen"
    };


    public List<string> phase2Lines = new List<string>
{
    "behold the ancient enemy and murderer strongly raises his head fight the lord battles ", 
        "with the undefeated army of the angels as once thou didst fight the first apostate and he prevailed not",
    "where the sea of peter has been settled for the light of the gentiles",
    "where the behemoth was cast out the serpent called the devil seduced the world",
    "pray the god of peace to crush satan may he no more be able to harm innocents amen"
};


    private Queue<(string word, bool isFirstWord)> wordQueue = new Queue<(string, bool)>();
    public int currentLineIndex = -1;

    private void Awake()
    {
        PopulateWordQueue();
    }

    public void PopulateWordQueue()
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

    public void SetNewLines(List<string> newLines)
    {
        originalLines = newLines;
        currentLineIndex = -1;
        PopulateWordQueue();
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
