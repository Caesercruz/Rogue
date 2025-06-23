using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Actors
{
    public Slider HealthBar;
    private void Awake()
    {
        actors = transform.parent.GetComponent<Actors>();
        gameScript = actors.transform.parent.GetComponent<GameScript>();
    }
    void Start()
    {
        Energy = MaxEnergy;
        Health = MaxHealth;

        ChangeHealth(HealthBar, MaxHealth, MaxHealth);
    }
    void Update()
    {
        if (Health == 0)
        {
            gameScript.NumberOfEnemies--;
            ClearAttackableTiles(false);

            actors.ActorsCord.TryGetValue(gameObject, out Vector2Int enemyPos);

            string tileName = $"Tile {enemyPos.x} {enemyPos.y}";
            GameObject tileObj = GameObject.Find(tileName);

            Tile tile = tileObj.GetComponent<Tile>();
            tile.IsOccupied = false;
            actors.ActorsCord.Remove(gameObject);

            Destroy(gameObject);
            if (gameScript.NumberOfEnemies == 0)
            {
                
                gameScript.ShowUpdateScreen();
                
                return;
            }
            RecalculateEnemyAttacks();
        }

        if (gameScript.Gamestate != GameScript.GameState.Combat || actors.isPlayersTurn) return;

        Energy = MaxEnergy;
        Weakness = 0;
        EnemyMove();
        SetAttackableTiles(false);
    }
    private void EnemyMove()
    {
        Vector2Int direction;
        while (Energy > 0)
        {
            if (!actors.ActorsCord.TryGetValue(gameScript.playerInstance, out var playerPos) ||
                !actors.ActorsCord.TryGetValue(gameObject, out var enemyPos))
            {
                Debug.Log("Movimento inimigo: Dicionário não encontrou a localização dos personagens");
                return;
            }

            direction = PathfindingUtility.AStarPathfinding(
                enemyPos,
                playerPos,
                pos => actors.IsValidPosition(pos),
                pos => actors.IsSpaceOccupied(pos),
                out var _ // você pode ignorar o caminho completo se não precisar dele
            );

            if (direction == Vector2Int.zero)
            {
                // Sem caminho ou já ao lado do jogador
                break;
            }

            actors.MoveCharacter(gameObject, direction);
            Energy--;
        }
    }


    public void RecalculateEnemyAttacks()
    {
        ClearAttackableTiles(false);
        foreach (var kvp in actors.ActorsCord)
        {
            Enemy enemy = kvp.Key.GetComponent<Enemy>();
            if (enemy != null && enemy.Health > 0)
            {
                enemy.SetAttackableTiles(false);
            }
        }
    }
}