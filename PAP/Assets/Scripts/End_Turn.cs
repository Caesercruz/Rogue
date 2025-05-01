using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class End_Turn : MonoBehaviour
{
    
    private Actors actor;
    public void OnButtonClick()
    {
        actor = FindAnyObjectByType<Actors>();
        if (actor == null)
        {
            Debug.Log("game é null");
            return;
        }
        actor.isPlayersTurn = false;
    }
}