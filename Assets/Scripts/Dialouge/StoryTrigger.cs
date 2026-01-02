using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    void Start()
    {
        DialogueManager.instance.StartDialogue(dialogue);
    }
}