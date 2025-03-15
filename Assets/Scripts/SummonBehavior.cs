using System.Collections;
using TMPro;
using UnityEngine;

public class SummonBehavior : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
