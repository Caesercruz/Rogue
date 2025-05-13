using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    public List<Perk> allPerks;

    public GameObject upgradeScreen;
    public GameObject hitBoxPrefab;
    public GameObject hitBox;
    [SerializeField] private GameObject _popupUpgrade;
    [SerializeField] private GameObject _popupDowngrade;
    [HideInInspector] public GameObject Popup;
    private void Start()
    {
        upgradeScreen = gameObject;
    }
    public void ShowAtributes(bool Upside)
    {
        hitBox = Instantiate(hitBoxPrefab, upgradeScreen.transform);
        if (Upside) Popup = Instantiate(_popupUpgrade, upgradeScreen.transform);
        else Popup = Instantiate(_popupDowngrade, upgradeScreen.transform);
        Popup.name = "Popup";
        hitBox.GetComponent<Button>().onClick.AddListener(() => upgradeScreen.GetComponent<Upgrade>().ClosePopup());
        foreach (Transform child in Popup.transform)
        {
            if (child.name != "Perk Name")
            {
                GameObject buttonObj = child.gameObject;
                string perkName = buttonObj.name;

                Perk perk = allPerks.Find(p => p.name == perkName);
                if (perk == null)
                {
                    Debug.LogWarning($"Perk '{perkName}' não encontrado!");
                    continue;
                }

                EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
                if (trigger == null)
                    trigger = buttonObj.AddComponent<EventTrigger>();

                // Limpa eventos antigos
                trigger.triggers.Clear();

                // Evento: Hover (mostrar nome do perk)
                EventTrigger.Entry hoverEntry = new()
                {
                    eventID = EventTriggerType.PointerEnter
                };
                hoverEntry.callback.AddListener((data) => GetPerkName(perk));
                trigger.triggers.Add(hoverEntry);

                // Evento: Clique (selecionar perk)
                EventTrigger.Entry clickEntry = new()
                {
                    eventID = EventTriggerType.PointerClick
                };
                clickEntry.callback.AddListener((data) => SelectAtribute(perk));
                trigger.triggers.Add(clickEntry);
            }
        }

    }
    public void SelectAtribute(Perk perk)
    {

        GameObject upgrade = GameObject.Find("UpgradeScreen");
        Transform area;
        area = upgrade.transform.Find("Upgrade Area");
        if (perk.type == PerkType.Debuff) area = upgrade.transform.Find("Down Side Area");

        Transform atribute = area.transform.Find("Atribute");

        Transform icon = atribute.transform.Find("Icon");
        Transform description = atribute.transform.Find("Description");

        icon.GetComponent<Image>().sprite = perk.icon;
        description.GetComponent<TextMeshProUGUI>().text = perk.description;
        ClosePopup();
    }
    public void Close()
    {
        if (upgradeScreen != null)
        {
            Destroy(upgradeScreen);
        }
    }
    public void ClosePopup()
    {
        if (Popup != null)
        {
            Destroy(Popup);
            Destroy(hitBox);
            return;
        }
        Debug.Log("Popup é null");
    }
    public void GetPerkName(Perk perk)
    {
        Transform label = Popup.transform.Find("Perk Name");
        if (label != null)
        {
            label.GetComponent<TextMeshProUGUI>().text = perk.name;
        }
        else
        {
            Debug.LogWarning("Label não encontrada!");
        }
    }
}