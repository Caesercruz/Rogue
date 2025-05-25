using UnityEngine;

public class Popupbuttons : MonoBehaviour
{
    void Start()
    {
        PopupPerks popup = GameObject.Find("Popup").GetComponent<PopupPerks>();
        popup.NotifyButtonSpawned();
    }
}