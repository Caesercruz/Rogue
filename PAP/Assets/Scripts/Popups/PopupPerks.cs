using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupPerks : MonoBehaviour
{
    public bool Selectable;

    public int ButtonCounter = 0;
    public int TotalButtonsToSpawn = 9;
    public Upgrade Upgrade;
    public GameScript gameScript;

    void Awake()
    {
        Upgrade = transform.parent.GetComponent<Upgrade>();
        gameScript = Upgrade.gameScript;
    }

    public void NotifyButtonSpawned(bool disableActivePerks)
    {
        ButtonCounter++;
        if (ButtonCounter == TotalButtonsToSpawn)
        {
            if (disableActivePerks) DisableActivePerkButtons();
            else EnableActivePerkButtons(); 
        }
    }
    public void GetPerkDescription(Perk perk)
    {
        Transform label = transform.Find("Perk Name");
        if (label != null)
        {
            label.GetComponent<TextMeshProUGUI>().text = perk.Description;
        }
        else
        {
            Debug.LogWarning("Label não encontrada!");
        }
    }
    public void DisableActivePerkButtons()
    {
        foreach (Transform child in transform)
        {
            Button button = child.GetComponent<Button>();
            if (button == null) continue;
            string perkName = button.name;
            if (gameScript.ActivePerks.Any(p => p.name == perkName))
            {
                button.interactable = false;
                button.GetComponent<EventTrigger>().enabled = false;
            }
        }
    }
    public void EnableActivePerkButtons()
    {
        foreach (Transform child in transform)
        {
            Button button = child.GetComponent<Button>();
            if (button == null) continue;
            string perkName = button.name;
            if (!gameScript.ActivePerks.Any(p => p.name == perkName))
            {
                button.interactable = false;
                button.GetComponent<EventTrigger>().enabled = false;
            }
        }
    }
}