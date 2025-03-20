using System.Collections;
using TMPro;
using UnityEngine;

public class DialougeUIScript : MonoBehaviour
{
    [SerializeField] private GameObject dialougeBox;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private TypeWriterEffect typeWriterEffect;
    private void Start()
    {
        typeWriterEffect = GetComponent<TypeWriterEffect>();
        CloseDialouge();
    }

    // Update is called once per frame
    public void ShowDialouge(DialougeObject dialougeObject)
    {
        dialougeBox.SetActive(true);
        StartCoroutine(StepThroughDialouge(dialougeObject));
    }

    private IEnumerator StepThroughDialouge(DialougeObject dialougeObject)
    {
        foreach(string dialouge in dialougeObject.Dialouge)
        {
            yield return typeWriterEffect.Run(dialouge, textLabel);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }
        CloseDialouge();
    }

    private void CloseDialouge()
    {
        dialougeBox.SetActive(false);
        textLabel.text = string.Empty;
    }
}
