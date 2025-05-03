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
}