using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Animator dialogueBoxAnimator;

    [Header("UI To Toggle")]
    public GameObject worldGUI;    
    public GameObject typingText;  

    private Queue<string> sentences;
    public bool isDialogueActive = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        sentences = new Queue<string>();
    }

    void Start()
    {
        //sentences = new Queue<string>();
        isDialogueActive = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) && isDialogueActive)
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true; 

        if (worldGUI != null) worldGUI.SetActive(false);
        if (typingText != null) typingText.SetActive(false);

        dialogueBoxAnimator.SetBool("IsOpen", true);

        nameText.text = dialogue.npcName;
        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    void EndDialogue()
    {
        dialogueBoxAnimator.SetBool("IsOpen", false);
        isDialogueActive = false;

        if (worldGUI != null) worldGUI.SetActive(true);
        if (typingText != null) typingText.SetActive(true);
    }
}