using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    
    private TextMeshProUGUI _txt_energy = null;
    private Image _image_Energy = null;

    public Transform healthBarsContainer;
    
    public void UpdateHealth(Slider healthBar, int health, int maxHealth = -1)
    {
        TextMeshProUGUI hpText = healthBar.GetComponentInChildren<TextMeshProUGUI>();
        if(maxHealth != -1) healthBar.maxValue = maxHealth;
        healthBar.value = health;
        hpText.text = health + "/" + maxHealth;
        if (maxHealth == -1) hpText.text = health + "/20";
    }

    public void UpdateEnergy(int energy, int maxEnergy)
    {
        CheckUIElements();
        _txt_energy.text = energy.ToString();
        _image_Energy.fillAmount = (float)energy / maxEnergy;
    }

    public Slider SpawnHealthBar(GameObject enemyInstance,Enemy enemy)
    {
        enemy.HealthBar = Instantiate(enemy.HealthBar, healthBarsContainer);
        enemy.HealthBar.name = $"{enemy.name} HealthBar";

        HealthBarHoverHandler hoverScript = enemy.HealthBar.GetComponent<HealthBarHoverHandler>();
        if (hoverScript == null)hoverScript = enemy.HealthBar.gameObject.AddComponent<HealthBarHoverHandler>();
        hoverScript.Initialize(enemyInstance);
        return enemy.HealthBar;
    }
    public void TransformHealthBars(Slider healthbar, int offSet)
    {
        healthbar.transform.localPosition += new Vector3(0, -offSet, 0);
    }
    private void CheckUIElements()
    {
        if(_txt_energy == null)
        {
            _txt_energy = gameObject.transform.Find("Combat UI/EnergyCircle/EnergyText").GetComponent<TextMeshProUGUI>();
        }
        if(_image_Energy == null)
        {
            _image_Energy = gameObject.transform.Find("Combat UI/EnergyCircle/Fill").GetComponent<Image>();
        } 
    }
}