using UnityEngine;

public class GroundHealth : MonoBehaviour
{
    int health = 2;
    private void Start()
    {
        GameScript gameScript = FindAnyObjectByType<GameScript>();
        gameScript.timedGrounds.Add(this);
    }
    public void DecreaseHealth()
    {
        health--;
        GetComponent<SpriteRenderer>().color = new(1, 1, 1, .4f);
        if (health == 0)
        {
            GameScript gameScript = FindAnyObjectByType<GameScript>();
            gameScript.timedGrounds.Remove(this);
            Tile occupiedTile = transform.parent.Find($"Tile {transform.localPosition.x} {transform.localPosition.y}").GetComponent<Tile>();
            occupiedTile.IsOccupied = false;
            Destroy(gameObject);
        }
    }
}