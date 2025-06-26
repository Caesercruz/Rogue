using UnityEngine;

public class Popupbuttons : MonoBehaviour
{
    void Start()
    {
        PopupPerks popup = transform.parent.GetComponent<PopupPerks>();
         popup.NotifyButtonSpawned(popup.Selectable);
    }
}