using System.Collections.Generic;
using UnityEngine;

public class ZhavokWordBank : MonoBehaviour
{
    public List<string> originalLines = new List<string>()
    {
        "in the name and by the power of our lord jesus christ we chant",
        "may you be snatched away from this fiend and redeemed by the divine",
        "transfiguratus in angelum lucis c-um tota malignorum spirituum caterva late circuit",
        "ecclesiam agni immaculati sponsam faverrimi hostes repleverunt amaritudinibus inebriarunt absinthio",
        "virus nequitiae suae tamquam flumen immundissimum draco maleficus transfundit in homines depravatos mente",
        "ut in ea deleat nomen dei ad aeternae gloriae coronam destinatas furetur mactet ac perdat ininteritum amen"
    };

    public List<string> phase2Lines = new List<string>()
    {
       "vade satana inventor et magister omnis fallacie hostis humanae",
        "da locum ecclesiae uni sanctae quam christus ipse acquisivit sanguine suo",
        "humiliare sub potenti manu dei contremisce a nobis sancto et terribili nomine jesu",
        "quem inferi tremunt cui virtutles subjectae sunt quem seraphim seraphim indefessis vocibus audant",
        "dicentes sanctus dominue deus sabaoth domine elxaudi orationem meam et camor meus ad te veniat",
        "gloriae tuae supplicamus ut ab omni infernalium spiritum potestate laqueo deceptione nos potenter liberare amen"
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
        }
        else
        {
        }
    }

}
