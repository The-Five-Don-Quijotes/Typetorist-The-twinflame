using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    public event Action OnDialogueEnded;

    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Animator dialogueBoxAnimator;

    public bool isDialogueActive = false;

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentSentence = "";
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        sentences = new Queue<string>();
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) && isDialogueActive)
        {
            if (isTyping)
            {
                CompleteSentence();
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        dialogueBoxAnimator.SetBool("IsOpen", true);
        nameText.text = dialogue.npcName;
        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    private void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentences.Dequeue();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }

        isTyping = false;
    }

    private void CompleteSentence()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        dialogueText.text = currentSentence;
        isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueBoxAnimator.SetBool("IsOpen", false);
        isDialogueActive = false;
        OnDialogueEnded?.Invoke();
    }
}