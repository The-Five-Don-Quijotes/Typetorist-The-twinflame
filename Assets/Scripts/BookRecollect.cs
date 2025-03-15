using TMPro;
using UnityEngine;

public class BookRecollect : MonoBehaviour
{
    public bool isTaken = false;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats.playerStats.ShowTyper(); //Show the typer again
            Destroy(gameObject); //Collect the book when it hit the player
        }    
    }
}
