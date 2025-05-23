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

        // Procura por todos os Transforms na hierarquia
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            Button button = child.GetComponent<Button>();
            if (button == null) continue;

            string perkName = child.name;

            if (gameScript.ActivePerks.Any(p => p.name == perkName))
            {
                button.interactable = false;
            }
        }
    }
}
