using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject Game;
    [SerializeField] private GameObject Settings;
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void NewGame()
    {
        StartCoroutine(FadeOutAndStartGame());
        Game.GetComponent<GameScript>().Gamestate = GameScript.GameState.Combat;
    }

    private System.Collections.IEnumerator FadeOutAndStartGame()
    {
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
}
