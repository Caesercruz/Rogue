using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{
    GameScript gameScript;
    Actors actors;
    [SerializeField] private GameObject GroundPrefab;
    [SerializeField] private int numberOfDestroyedTiles = 1;
    [SerializeField] private GameObject winScreen;
    void Start()
    {
        actors = transform.parent.GetComponent<Actors>();
        gameScript = actors.transform.parent.GetComponent<GameScript>();
    }
    public void DamageGround()
    {
        for (int i = 0; i < numberOfDestroyedTiles; i++)
        {
            Vector2Int bossPos = actors.ActorsCord[gameObject];
            GameObject player = gameScript.transform.GetComponentInChildren<Player>().gameObject;
            Vector2Int playerPos = actors.ActorsCord[player];

            Vector2Int direction = new(
                Mathf.Clamp(playerPos.x - bossPos.x, -1, 1),
                Mathf.Clamp(playerPos.y - bossPos.y, -1, 1)
            );

            List<Vector2Int> attackPattern = gameObject.GetComponent<Enemy>().AttackPattern;
            Vector2Int currentPos = bossPos + direction;

            Tile targetTile = null;

            int tries = 5;
            while (currentPos != playerPos)
            {
                if (attackPattern.Contains(currentPos - bossPos) &&
                    actors.GridTiles.TryGetValue(currentPos, out Tile tile) && (tile.IsOccupied == false))
                {
                    targetTile = tile;
                    break; // Para na primeira tile válida
                }

                currentPos += direction;
                tries--;
                if (tries == 0) break;
            }

            if (targetTile != null)
            {
                DestroyTile(targetTile, currentPos);
            }
        }
    }

    private void DestroyTile(Tile tile, Vector2Int position)
    {
        tile.IsOccupied = true;
        GameObject ground = Instantiate(GroundPrefab, actors.transform);
        ground.transform.position = new(position.x, position.y, 0);
    }
    public void Win()
    {
        GameObject winscreen = Instantiate(winScreen, gameScript.transform.parent);
        winscreen.transform.Find("XP").GetComponent<TextMeshProUGUI>().text += gameScript.Score;
        Destroy(gameScript.gameObject);
        gameScript.GameControls.Disable();
    }
}