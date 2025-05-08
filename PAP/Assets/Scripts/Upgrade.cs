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
        if (Upside) popup = Instantiate(_popupUpgrade);
        else popup = Instantiate(_popupDowngrade);
        hitBox.GetComponent<Button>().onClick.AddListener(() => upgradeScreen.GetComponent<Upgrade>().ClosePopup());
    }
    public void SelectAtribute(Perk perk)
    {
        Debug.Log("Entrou");

        GameObject upgrade = GameObject.Find("UpgradeScreen");
        Transform area;
        area = upgrade.transform.Find("Upgrade Area");
        if (perk.type == PerkType.Debuff) area = upgrade.transform.Find("Down Side Area");

        Transform atribute = area.transform.Find("Atribute");
        Transform description = atribute.transform.Find("Description");

        atribute.GetComponent<Image>().sprite = perk.icon;
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
        Debug.Log("Hitbox");
        if (popup != null)
        {
            Destroy(popup);
            Destroy(hitBox);
            return;
        }
    }
}