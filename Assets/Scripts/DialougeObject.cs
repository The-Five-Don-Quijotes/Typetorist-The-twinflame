using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/DialogueObject")]
public class DialougeObject : ScriptableObject
{
    [SerializeField][TextArea] private string[] dialouge;

    public string[] Dialouge => dialouge;
}
