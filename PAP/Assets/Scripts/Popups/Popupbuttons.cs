using UnityEngine;
using UnityEngine.EventSystems;

public class Popupbuttons : MonoBehaviour
{
    PopupPerks popup;
    void Start()
    {
        popup = transform.parent.GetComponent<PopupPerks>();
        popup.NotifyButtonSpawned(popup.Selectable);

        AddEvent();
    }
    private void AddEvent()
    {
        GameObject buttonObj = gameObject;
        string perkName = buttonObj.name;

        Perk perk = popup.gameScript.AllPerks.Find(p => p.name == perkName);
        if (perk == null)
        {
            Debug.LogWarning($"Perk '{perkName}' não encontrado!");
            return;
        }

        EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = buttonObj.AddComponent<EventTrigger>();

        // Limpa eventos antigos
        trigger.triggers.Clear();

        //Hover-mostrar nome do perk
        EventTrigger.Entry hoverEntry = new()
        {
            eventID = EventTriggerType.PointerEnter
        };
        hoverEntry.callback.AddListener((data) => popup.GetPerkDescription(perk));
        trigger.triggers.Add(hoverEntry);

        // Click-selecionar perk
        if (popup.Selectable)
        {
            EventTrigger.Entry clickEntry = new()
            {
                eventID = EventTriggerType.PointerClick
            };

            clickEntry.callback.AddListener((data) => popup.Upgrade.SelectAtribute(perk));
            trigger.triggers.Add(clickEntry);
        }
    }
}