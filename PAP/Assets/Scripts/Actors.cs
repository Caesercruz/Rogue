using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameScript;
using static UnityEngine.EventSystems.EventTrigger;

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

    public int Strength;

    private Vector2Int gridPosition;
    private float nextMoveTime = 0f;
    private readonly int _spawnRangeWidth = 2, _spawnRangeHeight = 6, _enemySpawnRangeWidth = 5, _enemySpawnRangeHeight = 6;

    void Start()
    {
        gameScript = GameObject.Find("GameManager").GetComponent<GameScript>();
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
    
    public void SpawnCharacter(GameObject character, string name, bool isPlayer)
    {
        do
        {
            if (isPlayer)
                gridPosition = new Vector2Int(Random.Range(0, _spawnRangeWidth), Random.Range(0, _spawnRangeHeight));
            else
                gridPosition = new Vector2Int(Random.Range(_enemySpawnRangeWidth, 8), Random.Range(0, _enemySpawnRangeHeight));
        }
        while (IsSpaceOccupied(gridPosition));

        Vector3 worldPosition = new(gridPosition.x, gridPosition.y, 0);
        character = Instantiate(character, worldPosition, Quaternion.identity);
        character.name = name;

            ActorsCord.Add(character, gridPosition);

        if (GridTiles.TryGetValue(gridPosition, out Tile tile))
        {
            tile.IsOccupied = true;
        }
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
                    tile.UnderAtack += Strength;
                    tile.damage.text = tile.UnderAtack.ToString();
                    if (tile.UnderAtack == 0) tile.damage.gameObject.SetActive(false);
                    else tile.damage.gameObject.SetActive(true);
                }
            }
        }
    }

    public void ClearAttackableTiles(bool isPlayer)
    {
        foreach (var tile in GridTiles.Values)
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
