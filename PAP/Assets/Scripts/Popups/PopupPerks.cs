using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupPerks : MonoBehaviour
{
    public int ButtonCounter = 0;
    public int TotalButtonsToSpawn = 9; // Define isto conforme necessário
    private GameScript gameScript;

    void Awake()
    {
        gameScript = GameObject.Find("GameManager").GetComponent<GameScript>();
    }

    public void NotifyButtonSpawned()
    {
        ButtonCounter++;
        if (ButtonCounter == TotalButtonsToSpawn)
        {
            DisableActivePerkButtons();
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
            }
        }
    }
}
