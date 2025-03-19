using UnityEngine;

public class VorrakShieldCast : MonoBehaviour
{
    public CanvasGroup bossShield; // Assign in Inspector

    public void ShowShield()
    {
        if (bossShield != null)
        {
            bossShield.alpha = 1f; // Make shield fully visible
            bossShield.interactable = true;
            bossShield.blocksRaycasts = true;
        }
    }

    public void HideShield()
    {
        if (bossShield != null)
        {
            bossShield.alpha = 0f; // Make shield fully invisible
            bossShield.interactable = false;
            bossShield.blocksRaycasts = false;
        }
    }

    public bool isShieldOn()
    {
        if (bossShield != null)
        {
            return bossShield.alpha == 1f;
        }
        return false;
    }
}
