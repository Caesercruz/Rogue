using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    public GameObject upgradeScreen;
    public GameObject hitBoxPrefab;
    public GameObject hitBox;
    [SerializeField] private GameObject _popupUpgrade;
    [SerializeField] private GameObject _popupDowngrade;
    [HideInInspector] public GameObject popup;
    private void Start()
    {
        upgradeScreen = gameObject;
    }
    public void ShowAtributes(bool Upside)
    {
        hitBox = Instantiate(hitBoxPrefab, upgradeScreen.transform);
        if (Upside) popup = Instantiate(_popupUpgrade, upgradeScreen.transform);
        else popup = Instantiate(_popupDowngrade, upgradeScreen.transform);
        popup.name = "Popup";
        hitBox.GetComponent<Button>().onClick.AddListener(() => upgradeScreen.GetComponent<Upgrade>().ClosePopup());
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
        if (popup != null)
        {
            Destroy(popup);
            Destroy(hitBox);
            return;
        }
    }
    public void GetPerkName(Perk perk)
    {
        Transform popup = transform.Find("Popup");
        Transform label = popup.Find("Perk Name");
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