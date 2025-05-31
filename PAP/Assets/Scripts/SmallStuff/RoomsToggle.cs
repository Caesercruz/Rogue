using UnityEngine;

public class RoomsToggle : MonoBehaviour
{
    public void ChangeState()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new(.676f, .827f, .38f, 1);
    }
}