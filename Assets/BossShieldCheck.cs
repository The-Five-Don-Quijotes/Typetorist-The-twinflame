using UnityEngine;

public class BossShieldCheck : MonoBehaviour
{
    public Transform vorrakTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(vorrakTransform == null)
        {
            this.gameObject.SetActive(false);
        }
    }
}
