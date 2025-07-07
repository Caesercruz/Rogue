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
        Destroy(gameObject);
    }
    public void OpenMenu()
    {
        Save();
        Instantiate(menu);
        Destroy(transform.parent.gameObject);
    }
    public void Save()
    {
        GameScript gameScript = transform.parent.GetComponent<GameScript>();
        MinimapManager minimapManager = gameScript.transform.GetComponentInChildren<MinimapManager>();

    }
    public void OpenSettings()
    {
        Instantiate(settings);
        Destroy(gameObject);
    }
}
