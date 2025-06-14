using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Actors : MonoBehaviour
{
    protected GameScript gameScript;
    protected Actors actors;

    public Dictionary<GameObject, Vector2Int> ActorsCord = new();
    public Dictionary<Vector2Int, Tile> GridTiles = new();

    public bool isPlayersTurn = true;

    public int Energy;
    public int MaxEnergy;
    public int Health;
    public int MaxHealth;

    public List<Vector2Int> AttackPattern = new();

    public int Strength, Weakness = 0;

    private float nextMoveTime = 0f;

    public int _spawnRangeWidth = 2, _spawnRangeHeight = 6;

    private void Awake()
    {
        gameScript = transform.parent.GetComponent<GameScript>();
        actors = this;
    }
    public bool MoveCharacter(GameObject character, Vector2Int direction)
    {
        Vector2Int position = ActorsCord[character];
        Vector2Int newPosition = position + direction;

        if (!IsValidPosition(newPosition))
        {
            Debug.Log("Out of bounds");
            return false;
        }
        if (IsSpaceOccupied(newPosition))
        {
            Debug.Log("Space is Occupied");
            return false;
        }
        if (isPlayersTurn && Time.time < nextMoveTime) return false;

        // Liberta tile antiga
        if (GridTiles.TryGetValue(position, out Tile currentTile))
        {
            currentTile.IsOccupied = false;
        }

        if (!ActorsCord.ContainsKey(character))
        {
            Debug.Log("O dicionário não encontrou a chave. Personagem não encontrado: " + character.name);
            return false;
        }

        // Atualiza a posição lógica antes da animação
        ActorsCord[character] = newPosition;
        // Ocupa nova tile
        if (GridTiles.TryGetValue(newPosition, out Tile newTile))
        {
            newTile.IsOccupied = true;
        }

        // Inicia animação
        AnimationManager animationSpawner = FindAnyObjectByType<AnimationManager>();
        StartCoroutine(animationSpawner.MoveOverTime(character.transform, new Vector3(newPosition.x, newPosition.y, 0), 0.15f));

        nextMoveTime = Time.time + 0.1f;
        return true;
    }

    public GameObject SpawnCharacter(GameObject character, string name)
    {
        Debug.LogWarning(actors.name);
        character = Instantiate(character, actors.transform);
        character.name = name;

        Vector2Int gridPosition;
        do
        {
            gridPosition = new Vector2Int(Random.Range(_spawnRangeWidth, 8), Random.Range(0, _spawnRangeHeight));
        }
        while (IsSpaceOccupied(gridPosition));

        Debug.Log($"SpawnRange: {_spawnRangeWidth}, {_spawnRangeWidth}");

        Vector3 worldPosition = new(gridPosition.x, gridPosition.y, 0);
        character.transform.position = worldPosition;
        actors.ActorsCord.Add(character, gridPosition);

        if (GridTiles.TryGetValue(gridPosition, out Tile tile))
        {
            tile.IsOccupied = true;
        }
        gameScript.NumberOfEnemies++;
        return character;
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gameScript.Width && position.y >= 0 && position.y < gameScript.Height;
    }

    public bool IsSpaceOccupied(Vector2Int position)
    {
        if (GridTiles.TryGetValue(position, out Tile tile))
        {
            return tile.IsOccupied;
        }

        Debug.LogWarning($"Tile {position.x} {position.y} não foi encontrada no dicionário.");
        return false;
    }

    public void SetAttackableTiles(bool isPlayer)
    {
        Vector2Int position = actors.ActorsCord[gameObject];
        foreach (Vector2Int direction in AttackPattern)
        {
            Vector2Int adjacentPosition = position + direction;
            if (actors.IsValidPosition(adjacentPosition) && actors.GridTiles.TryGetValue(adjacentPosition, out Tile tile))
            {
                if (isPlayer) tile.InAtackRange = true;
                else
                {
                    tile.UnderAtack += (Strength - Weakness);
                    tile.damage.text = tile.UnderAtack.ToString();
                    if (tile.UnderAtack <= 0) tile.damage.gameObject.SetActive(false);
                    else tile.damage.gameObject.SetActive(true);
                }
            }
        }
    }

    public void ClearAttackableTiles(bool isPlayer)
    {
        foreach (var tile in actors.GridTiles.Values)
        {
            if (isPlayer) tile.InAtackRange = false;
            else
            {
                tile.UnderAtack = 0;
                tile.damage.gameObject.SetActive(false);
            }
        }
    }
    public void ChangeHealth(Slider healthBar, int health, int maxhealth)
    {
        MaxHealth = maxhealth;
        Health = Mathf.Clamp(health, 0, maxhealth);
        FindAnyObjectByType<HUDManager>().UpdateHealth(healthBar, Health, MaxHealth);
    }

    public void ChangeEnergy(int energy, int maxenergy)
    {
        MaxEnergy = maxenergy;
        Energy = Mathf.Clamp(energy, 0, maxenergy);
        FindAnyObjectByType<HUDManager>().UpdateEnergy(Energy, MaxEnergy);
    }
}