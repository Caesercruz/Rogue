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
    private void Start()
    {
        gameScript = GameObject.Find("GameManager").GetComponent<GameScript>();
        player = GameObject.Find("Player").GetComponent<Player>();
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

            animationSpawner.PerksSelectedAnimation(SelectedBuff == null, SelectedByproduct == null);
            return;
        }

        SelectedBuff.active = true;
        SelectedByproduct.active = true;

        gameScript.ActivePerks.Add(SelectedBuff);
        gameScript.ActivePerks.Add(SelectedByproduct);

        ApplyPerks();

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
        if (SelectedBuff == null || SelectedByproduct == null) return false;
        else return true;
    }
    public void ApplyPerks()
    {
        switch (SelectedBuff.perkName)
        {
            case "Energetic":
                player.ChangeEnergy(player.MaxEnergy, player.MaxEnergy += 2);
                break;
            case "Reinforced Plates":
                player.ChangeHealth(player.HealthBar, Mathf.CeilToInt(player.Health * 1.5f), Mathf.CeilToInt(player.MaxHealth * 1.5f));
                break;
            case "Increased Reach":
                player.AttackPattern.Add(new Vector2Int(-2, -2));
                player.AttackPattern.Add(new Vector2Int(-2, -1));
                player.AttackPattern.Add(new Vector2Int(-2, -0));
                player.AttackPattern.Add(new Vector2Int(-2, 1));
                player.AttackPattern.Add(new Vector2Int(-2, 2));

                player.AttackPattern.Add(new Vector2Int(-1, -2));
                player.AttackPattern.Add(new Vector2Int(-1, 2));

                player.AttackPattern.Add(new Vector2Int(0, -2));
                player.AttackPattern.Add(new Vector2Int(0, 2));

                player.AttackPattern.Add(new Vector2Int(1, -2));
                player.AttackPattern.Add(new Vector2Int(1, 2));

                player.AttackPattern.Add(new Vector2Int(2, -2));
                player.AttackPattern.Add(new Vector2Int(2, -1));
                player.AttackPattern.Add(new Vector2Int(2, -0));
                player.AttackPattern.Add(new Vector2Int(2, 1));
                player.AttackPattern.Add(new Vector2Int(2, 2));

                break;
        }

        switch (SelectedByproduct.perkName)
        {
            case "Weak":
                player.Strength = Mathf.CeilToInt(player.Strength * .7f);
                break;
            case "Rusty Plates":
                player.ChangeHealth(player.HealthBar, player.Health, Mathf.CeilToInt(player.MaxHealth * .7f));
                break;
            case "Low Energy":
                player.ChangeEnergy(player.Energy, player.MaxEnergy -= 1);
                break;
        }
    }
}