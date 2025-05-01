using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class HealthBarHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private HUDManager hudManager;
    
    public float fadeDuration = 0.3f;

    private GameObject enemy; // Referência ao inimigo associado
    private SpriteRenderer glowRenderer;
    private Coroutine fadeCoroutine;
    public void Initialize(GameObject enemy)
    {
        glowRenderer = enemy.transform.Find("Glow")?.GetComponent<SpriteRenderer>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (glowRenderer != null)
        {
            StartFade(1f); // fade in
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (glowRenderer != null)
        {
            StartFade(0f); // fade out
        }
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = glowRenderer.color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            SetAlpha(alpha);
            time += Time.deltaTime;
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        Color color = glowRenderer.color;
        color.a = alpha;
        glowRenderer.color = color;
    }
}
