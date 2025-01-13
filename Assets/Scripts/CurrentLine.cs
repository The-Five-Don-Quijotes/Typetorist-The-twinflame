using TMPro;
using UnityEngine;

public class CurrentLine : MonoBehaviour
{
    public WordBank wordBank = null;
    public TextMeshProUGUI outputLine = null;

    private string currentLine = string.Empty;
    private string currentWord = string.Empty;
    private string lastWord = string.Empty;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetCurrentLine();
    }
    private void SetCurrentLine()
    {
        currentLine = wordBank.GetLine();
        outputLine.text = currentLine;
    }

    // Update is called once per frame
    void Update()
    {
        SetCurrentLine();
    }
}
