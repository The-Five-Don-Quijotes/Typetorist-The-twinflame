using System.Collections;
using UnityEngine;

public class VorrakMovement : MonoBehaviour
{
    public GameObject melee;
    [Header("Attack Settings")]
    public float cooldown = 0.1f;
    public float hitboxDuration = 0.2f;
    public void ActivateMeleeHitbox()
    {
        StartCoroutine(EnableHitbox(melee));
    }

    IEnumerator EnableHitbox(GameObject hitbox)
    {
        hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxDuration);
        hitbox.SetActive(false);
    }
}
