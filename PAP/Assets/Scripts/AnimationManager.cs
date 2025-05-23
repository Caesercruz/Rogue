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

    public void PerksSelectedAnimation(bool buffMissing, bool byproductMissing) //Error perk Animation handler
    {
        GameObject upgrade = GameObject.Find("UpdateScreen");

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
    public IEnumerator AnimateIcon(Transform iconTransform) //Error perk not selected
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
    public IEnumerator AnimatePopupSpawn(Transform target, float duration = .1f)
    {
        Debug.Log("Iniciando animação");
        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = new(0.73f, .6f,1);
        float elapsedTime = 0f;

        target.localScale = initialScale;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            target.localScale = Vector3.Lerp(initialScale, finalScale, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        target.localScale = finalScale;
        Debug.Log("Animação concluída");
    }

}