using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject Game;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject Settings;
    [SerializeField] private float fadeDuration = 1f;

    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        if(newGameButton!=null)
        newGameButton.onClick.AddListener(NewGame);
    }

    public void NewGame()
    {
        transform.Find("New Game").GetComponent<Button>().onClick.RemoveAllListeners();
        StartCoroutine(FadeOutAndStartGame(true));
    }
    public void Continue()
    {
        transform.Find("New Game").GetComponent<Button>().onClick.RemoveAllListeners();
        StartCoroutine(FadeOutAndStartGame(false));
    }
    private System.Collections.IEnumerator FadeOutAndStartGame(bool newGame)
    {
        Game.GetComponent<GameScript>().NewGame = newGame;
        Instantiate(Game);
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = 1f - (timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        
        Destroy(gameObject);
    }

    public void CloseGame()
    {
        Application.Quit();
    }
    public void OpenMenu()
    {
        Instantiate(menu);
        Destroy(transform.parent.gameObject);
    }
}