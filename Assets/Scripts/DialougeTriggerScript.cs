using UnityEngine;

public class DialougeTriggerScript : MonoBehaviour
{
    [SerializeField] private DialougeObject DialougeObject;
    [SerializeField] private DialougeUIScript dialouge;

    void Start()
    { 
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Make sure we're only triggering on the player
        {
            Debug.Log("Player entered trigger");

                dialouge.ShowDialouge(DialougeObject);
                gameObject.SetActive(false);
        }
    }
}
