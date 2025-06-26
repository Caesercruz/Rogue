using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupPerks : MonoBehaviour
{
    public bool Selectable;

    public int ButtonCounter = 0;
    public int TotalButtonsToSpawn = 9;
    private GameScript gameScript;

    void Awake()
    {
        gameScript = transform.parent.GetComponent<Upgrade>().gameScript;
    }

    public void NotifyButtonSpawned(bool disableActiveButtons)
    {
        ButtonCounter++;
        if (ButtonCounter == TotalButtonsToSpawn)
        {
            if(disableActiveButtons) DisableActivePerkButtons();
            else EnableActivePerkButtons();
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