using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    public GameScript gameScript;
    public GameObject upgradeScreen;
    public GameObject hitBoxPrefab;
    public GameObject hitBox;
    [SerializeField] private GameObject _popupUpgrade;
    [SerializeField] private GameObject _popupDowngrade;
    [HideInInspector] public GameObject Popup;

    public Perk SelectedBuff, SelectedByproduct;
    private void Awake()
    {
        upgradeScreen = gameObject;
        gameScript = transform.parent.GetComponent<GameScript>();
    }
    private void Start()
    {
        gameScript.GameControls.PlayerControls.Disable();
    }
    private void Update()
    {
        if (Popup != null)
        {
            if (gameScript.GameControls.Actions.Back.triggered) ClosePopup();
        }
    }
    public void ShowPerks(bool buff)
    {
        if (Popup != null) return;
        hitBox = Instantiate(hitBoxPrefab, upgradeScreen.transform);
        if (buff) Popup = Instantiate(_popupUpgrade, upgradeScreen.transform);
        else Popup = Instantiate(_popupDowngrade, upgradeScreen.transform);
        Popup.name = "Popup";
        hitBox.GetComponent<Button>().onClick.AddListener(() => upgradeScreen.GetComponent<Upgrade>().ClosePopup());
    }
    public void SelectAtribute(Perk perk)
    {
        GameObject upgrade = gameObject;
        Transform area = upgrade.transform.Find("Upgrade Area");
        if (perk.Type == PerkType.Debuff) area = upgrade.transform.Find("Down Side Area");

        Transform atribute = area.transform.Find("Atribute");

        Transform icon = atribute.transform.Find("Icon");
        Transform description = atribute.transform.Find("Description");

        icon.GetComponent<Image>().sprite = perk.Icon;
        description.GetComponent<TextMeshProUGUI>().text = perk.name;

        if (perk.Type == PerkType.Buff) SelectedBuff = perk;
        if (perk.Type == PerkType.Debuff) SelectedByproduct = perk;
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

        gameScript.ActivePerks.Add(SelectedBuff);
        gameScript.ActivePerks.Add(SelectedByproduct);

        gameScript.MapManager.OpenMap(true);
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
        Debug.Log("Popup � null");
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
            Debug.LogWarning("Label n�o encontrada!");
        }
    }
    
    private bool PerksSelectedValidation()
    {
        if (SelectedBuff == null && SelectedByproduct == null) return true;
        if (SelectedBuff == null || SelectedByproduct == null) return false;
        return true;
    }
}