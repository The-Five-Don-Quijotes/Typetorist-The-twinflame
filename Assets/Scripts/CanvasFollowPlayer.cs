using TMPro;
using UnityEngine;

public class TextFollowPlayer : MonoBehaviour
{
    public Transform player; //Transform of the player
    public Vector3 offset; //Offset relative to the player

    void Update()
    {
        if (player != null)
        {
            //Update the canvas position to follow the player
            transform.position = player.position + offset;
        }
    }
}
