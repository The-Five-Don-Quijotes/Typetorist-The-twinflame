using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat playerStat;

    //public GameObject player;
    public TextMeshProUGUI zoneText;
    public Slider zoneSlider;
    public TextMeshProUGUI healthText;
    public Slider healthSlider;

    public float zoneValue;
    public float maxZone;
    public float healthValue;
    public float maxHealth;
    public float zoneLost;
    public float healthLost;
    public float zoneGain;
    public float healthGain;

    void Awake()
    {
        if (playerStat != null)
        {
            Destroy(playerStat);
        }
        else
        {
            playerStat = this;
        }
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        zoneValue = maxZone / 2;
        zoneSlider.value = 0.5f;
        zoneText.text = Mathf.Ceil(zoneValue).ToString() + " / " + Mathf.Ceil(maxZone).ToString();

        healthValue = maxHealth;
        healthSlider.value = 1f;
        healthText.text = Mathf.Ceil(healthValue).ToString() + " / " + Mathf.Ceil(maxHealth).ToString();
    }

    private void Update() //for testing
    {
        if (Input.GetMouseButtonDown(0)) { //left click
            HitEnemy();
        }
        else if(Input.GetMouseButtonDown(1)) { //right click
            BeingDamaged(); 
        }
    }

    public void BeingDamaged()
    {
        zoneValue -= zoneLost;
        SetZoneUI();

        healthValue -= healthLost;
        SetHealthUI();
    }

    public void HitEnemy()
    {
        zoneValue += zoneGain;
        SetZoneUI();

        healthValue += healthGain;
        SetHealthUI();
    }

    private void SetZoneUI()
    {
        CheckOverZone();
        UpdateZoneProgress(); //Update color of the zone bar
        zoneSlider.value = CalculateZonePercentage();
        zoneText.text = Mathf.Ceil(zoneValue).ToString() + " / " + Mathf.Ceil(maxZone).ToString();
    }

    private void SetHealthUI()
    {
        CheckOverHeal();
        healthSlider.value = CalculateHealthPercentage();
        healthText.text = Mathf.Ceil(healthValue).ToString() + " / " + Mathf.Ceil(maxHealth).ToString();
    }

    private void CheckOverZone()
    {
        if(zoneValue > maxZone)
        {
            zoneValue = maxZone;
            zoneSlider.value = 1f;
        }
        else if(zoneValue < 0)
        {
            zoneValue = 0;
            zoneSlider.value = 0f;
        }
    }

    private void CheckOverHeal()
    {
        if (healthValue > maxHealth)
        {
            healthValue = maxHealth;
            healthSlider.value = 1f;
        }
        else if (healthValue < 0)
        {
            healthValue = 0;
            healthSlider.value = 0f;
        }
    }

    private void UpdateZoneProgress()
    {
        Image fillColor = zoneSlider.fillRect.GetComponent<Image>();
        if (zoneValue >= 90)
        {
            fillColor.color = Color.red;
        }
        else if (zoneValue >= 70)
        {
            fillColor.color = Color.yellow;
        }
        else if (zoneValue >= 30)
        {
            fillColor.color = Color.green;
        }
        else
        {
            fillColor.color = new Color(0, 0, 0.5f);
        }
    }

    private float CalculateZonePercentage()
    {
        return zoneValue / maxZone;
    }

    private float CalculateHealthPercentage()
    {
        return healthValue / maxHealth;
    }
}
