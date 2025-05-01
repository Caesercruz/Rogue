using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public GameObject slashAnimationPrefab;

    public void SpawnSlashAnimation(Transform canvasTransform)
    {
        Instantiate(slashAnimationPrefab, canvasTransform);
    }

    public void OnAnimationEnd()
    {
        Destroy(gameObject);
    }
}