using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI _txt_energy;
    [SerializeField] private Image _image_Energy;

    public Transform healthBarsContainer;

    public void UpdateHealth(Slider healthBar, int health, int maxHealth)
    {
        TextMeshProUGUI hpText = healthBar.GetComponentInChildren<TextMeshProUGUI>();
        healthBar.maxValue = maxHealth;
        healthBar.value = health;
        hpText.text = health + "/" + maxHealth;
    }

    public void UpdateEnergy(int energy, int maxEnergy)
    {
        _txt_energy.text = energy.ToString();
        _image_Energy.fillAmount = (float)energy / maxEnergy;
    }

    public void OffsetHealthBar(Slider healthbar, GameObject enemy)
    {
        string enemyName = enemy.name;
        System.Text.RegularExpressions.Regex regex = new(@"(\d+)$");
        var match = regex.Match(enemyName);

        int enemyNumber = 0;
        if (match.Success)
            enemyNumber = int.Parse(match.Value);

        int offsetY = -1 * enemyNumber;

        RectTransform rectTransform = healthbar.GetComponent<RectTransform>();
        Vector3 newPos = new(0, offsetY, 0);

        rectTransform.localPosition += newPos;
    }
}