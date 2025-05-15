using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public GameObject slashAnimationPrefab;

    public void SpawnSlashAnimation(Transform canvasTransform)
    {
        Instantiate(slashAnimationPrefab, canvasTransform);
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
    private IEnumerator AnimateIcon(Transform iconTransform)
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
}