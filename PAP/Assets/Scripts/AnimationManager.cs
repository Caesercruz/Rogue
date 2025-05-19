using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public GameObject slashAnimationPrefab,DoubleSlashPrefab;
    public IEnumerator MoveOverTime(Transform objTransform, Vector3 targetPosition, float duration)
    {
        Vector3 startPos = objTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            objTransform.position = Vector3.Lerp(startPos, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        objTransform.position = targetPosition;
    }
    public enum StrikeType
    {
        Default,
        Double
    };
    public void SpawnSlashAnimation(StrikeType type,Transform canvasTransform)
    {
        if (type == StrikeType.Default)
        {
            Instantiate(slashAnimationPrefab, canvasTransform);
        }
        else if (type == StrikeType.Double)
        {
            Instantiate(DoubleSlashPrefab, canvasTransform);
        }
    }

    public void PerksSelectedAnimation(bool buffMissing, bool byproductMissing)
    {
        GameObject upgrade = GameObject.Find("UpgradeScreen");

        if (buffMissing)
        {
            Transform areaBuff = upgrade.transform.Find("Upgrade Area");
            Transform icon = areaBuff.transform.Find("Atribute/Icon");
            StartCoroutine(AnimateIcon(icon));
        }

        if (byproductMissing)
        {
            Transform areaByprod = upgrade.transform.Find("Down Side Area");
            Transform icon = areaByprod.transform.Find("Atribute/Icon");
            StartCoroutine(AnimateIcon(icon));
        }
    }
    public void OnAnimationEnd()
    {
        Destroy(gameObject);
    }
    public IEnumerator AnimateIcon(Transform iconTransform)
    {
        if (iconTransform == null) yield break;

        Image iconImage = iconTransform.GetComponent<Image>();
        if (iconImage == null) yield break;

        Vector3 originalPos = iconTransform.localPosition;
        Color originalColor = iconImage.color;
        Color alertColor = Color.red;

        float vibrationStrength = 5f;
        float duration = 0.1f;

        for (int i = 0; i < 3; i++)
        {
            // Vibração (move ligeiramente para os lados)
            iconTransform.localPosition = originalPos + (Vector3.right * vibrationStrength);
            iconImage.color = alertColor;
            yield return new WaitForSeconds(duration);

            iconTransform.localPosition = originalPos - (Vector3.right * vibrationStrength);
            yield return new WaitForSeconds(duration);

            iconTransform.localPosition = originalPos;
            iconImage.color = originalColor;
            yield return new WaitForSeconds(duration);
        }
    }

    public IEnumerator AnimatePopupSpawn(Transform target, float duration = 0.5f)
    {
        Debug.Log("Iniciando animação de escala...");
        Debug.Log("timeScale: " + Time.timeScale);

        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = Vector3.one;
        float elapsedTime = 0f;

        target.localScale = initialScale;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // EaseOut
            Vector3 newScale = Vector3.Lerp(initialScale, finalScale, t);
            target.localScale = newScale;

            Debug.Log($"[ANIMAÇÃO] t={t}, scale={newScale}, elapsed={elapsedTime}");

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }


        target.localScale = finalScale;
        Debug.Log("Animação concluída.");
    }
}