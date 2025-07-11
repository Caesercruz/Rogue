using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject settings;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Continue()
    {
        transform.GetComponentInParent<GameScript>().CloseAction();
    }
    public void OpenMenu()
    {
        GameScript gameScript = transform.parent.GetComponent<GameScript>();
        MinimapManager minimapManager = gameScript.GetComponentInChildren<MinimapManager>();
        gameScript.GameControls.Disable();
        Instantiate(menu);
        Destroy(transform.parent.gameObject);
    }
    
    public void OpenSettings()
    {
        //Instantiate(settings);
        //Destroy(gameObject);
    }
}
