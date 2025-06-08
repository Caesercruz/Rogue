using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    public List<Perk> allPerks;
    GameScript gameScript;
    Player player;
    public GameObject upgradeScreen;
    public GameObject hitBoxPrefab;
    public GameObject hitBox;
    [SerializeField] private GameObject _popupUpgrade;
    [SerializeField] private GameObject _popupDowngrade;
    [HideInInspector] public GameObject Popup;

    public Perk SelectedBuff, SelectedByproduct;

    private void Update()
    {
        if (Popup != null)
        {
            if (gameScript.GameControls.Actions.Back.triggered) ClosePopup();
        }
    }
    private void Start()
    {
        gameScript = GameObject.Find("GameManager").GetComponent<GameScript>();
        player = GameObject.Find("Player").GetComponent<Player>();
        upgradeScreen = gameObject;
        gameScript.GameControls.PlayerControls.Disable();
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

                //Hover-mostrar nome do perk
                EventTrigger.Entry hoverEntry = new()
                {
                    eventID = EventTriggerType.PointerEnter
                };
                hoverEntry.callback.AddListener((data) => GetPerkName(perk));
                trigger.triggers.Add(hoverEntry);

                // Click-selecionar perk
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
        GameObject upgrade = gameObject;
        Transform area = upgrade.transform.Find("Upgrade Area");
        if (perk.type == PerkType.Debuff) area = upgrade.transform.Find("Down Side Area");

        Transform atribute = area.transform.Find("Atribute");

        Transform icon = atribute.transform.Find("Icon");
        Transform description = atribute.transform.Find("Description");

        icon.GetComponent<Image>().sprite = perk.icon;
        description.GetComponent<TextMeshProUGUI>().text = perk.description;

        if (perk.type == PerkType.Buff) SelectedBuff = perk;
        if (perk.type == PerkType.Debuff) SelectedByproduct = perk;
        ClosePopup();
    }
    public void Close()
    {
        if (!PerksSelectedValidation())
        {
            AnimationManager animationSpawner = FindAnyObjectByType<AnimationManager>();

            animationSpawner.PerksSelectedAnimation(upgradeScreen, SelectedBuff == null, SelectedByproduct == null);
            return;
        }

        SelectedBuff.active = true;
        SelectedByproduct.active = true;

        gameScript.ActivePerks.Add(SelectedBuff);
        gameScript.ActivePerks.Add(SelectedByproduct);

        gameScript.GetComponent<PerkEffects>().ApplyPerks(player, SelectedBuff, SelectedByproduct);
        gameScript.MapManager.ShowMap(true);
        if (upgradeScreen != null)
        {
            Destroy(upgradeScreen);
        }
        return;
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
    private bool PerksSelectedValidation()
    {

        if (SelectedBuff == null && SelectedByproduct == null)
        {
            WarningPerks();
        }
        if (SelectedBuff == null || SelectedByproduct == null) return false;
        return true;
    }
    public void WarningPerks()
    {
        //No perk selected
    }
}