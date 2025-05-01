using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Transform canvasTransform;

    [SerializeField] private Sprite _baseSprite, _alterSprite;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject darkOverlay;
    public TextMeshProUGUI damage;

    public bool IsOccupied = false;
    public bool InAtackRange = false;
    public int UnderAtack = 0;

    public void Init(bool IsoffSet)
    {
        _renderer.sprite = IsoffSet ? _alterSprite : _baseSprite;
    }

    void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        _highlight.SetActive(false);
    }
    public void SetDarkOverlay(bool active)
    {
        darkOverlay.SetActive(active);
    }

    public Transform GetCanvasTransform()
    {
        return canvasTransform;
    }
}
