using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Actors
{
    private class Node
    {
        public Vector2Int Position;
        public float GCost; // Custo de chegada (do início até aqui)
        public float HCost; // Heurística (distância até o destino)
        public float FCost => GCost + HCost; // Custo total
        public Node Parent; // Para reconstruir o caminho

        public Node(Vector2Int position)
        {
            Position = position;
        }
    }

    public Slider HealthBar;
    void Start()
    {
        gameScript = GameObject.Find("GameManager").GetComponent<GameScript>();
        actors = GameObject.Find("BoardManager").GetComponent<Actors>();
        gameScript._ratInstance = gameObject;

        HealthBar = Instantiate(HealthBar, GameObject.Find("Canvas").GetComponent<HUDManager>().healthBarsContainer);
        HealthBar.name = $"{gameObject.name} HealthBar";

        HealthBarHoverHandler hoverScript = HealthBar.GetComponent<HealthBarHoverHandler>();
        if (hoverScript == null)
        {
            hoverScript = HealthBar.gameObject.AddComponent<HealthBarHoverHandler>();
        }
        hoverScript.Initialize(gameObject);

        GameObject.Find("Canvas").GetComponent<HUDManager>().OffsetHealthBar(HealthBar, gameObject);

        Energy = MaxEnergy;
        Health = MaxHealth;

        ChangeHealth(HealthBar, MaxHealth, MaxHealth);
    }
    void Update()
    {
        if (Health == 0)
        {
            gameScript.NumberOfEnemies--;
            actors.ClearAttackableTiles(false);

            actors.ActorsCord.TryGetValue(gameObject, out Vector2Int enemyPos);

            string tileName = $"Tile {enemyPos.x} {enemyPos.y}";
            GameObject tileObj = GameObject.Find(tileName);

            Tile tile = tileObj.GetComponent<Tile>();
            tile.IsOccupied = false;
            actors.ActorsCord.Remove(gameObject);

            Destroy(gameObject);
            if (gameScript.NumberOfEnemies == 0)
            {
                gameScript.Gamestate = GameScript.GameState.WonEncounter;
                Canvas instance = Instantiate(gameScript.UpdateScreen);
                instance.name = "UpdateScreen";
                return;
            }
            RecalculateEnemyAttacks();
        }

        if (gameScript.Gamestate != GameScript.GameState.Combat || actors.isPlayersTurn) return;
        Energy = MaxEnergy;
        EnemyMove();
        SetAttackableTiles(false);
    }
    private void EnemyMove()
    {
        Vector2Int direction;
        while (Energy > 0)
        {
            if (!actors.ActorsCord.TryGetValue(gameScript._playerInstance, out var playerPos) ||
                !actors.ActorsCord.TryGetValue(gameObject, out var enemyPos))
            {
                Debug.Log("Movimento inimigo: Dicionário não encontrou a localização dos personagens");
                return;
            }

            direction = AStarPathfinding(enemyPos, playerPos);
            if (direction == Vector2Int.zero)
            {
                // Sem caminho ou já ao lado do jogador
                break;
            }

            actors.MoveCharacter(gameObject, direction);
            Energy--; // Diminui energia sempre (ou, se preferires, só quando move)
        }
    }

    private Vector2Int AStarPathfinding(Vector2Int startPos, Vector2Int targetPos)
    {
        List<Node> openList = new(); // Lista de nós a serem explorados
        HashSet<Vector2Int> closedList = new(); // Lista de nós já explorados

        Node startNode = new(startPos);
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Ordena a lista pela menor FCost
            openList.Sort((nodeA, nodeB) => nodeA.FCost.CompareTo(nodeB.FCost));
            Node currentNode = openList[0];
            openList.RemoveAt(0);

            // Se chegarmos ao destino, reconstruímos o caminho
            if (currentNode.Position == targetPos)
            {
                return RetracePath(currentNode, startPos); // Retorna a direção do próximo passo
            }

            closedList.Add(currentNode.Position);

            // Verifica os vizinhos
            foreach (var direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighborPos = currentNode.Position + direction;

                if (!actors.IsValidPosition(neighborPos))
                    continue;

                if (actors.IsSpaceOccupied(neighborPos) && neighborPos != targetPos)  // Ignora a tile do jogador
                    continue;

                if (closedList.Contains(neighborPos))
                    continue;

                float newGCost = currentNode.GCost + 1;
                Node neighborNode = openList.FirstOrDefault(node => node.Position == neighborPos);

                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos);
                    openList.Add(neighborNode);
                }

                if (newGCost < neighborNode.GCost || neighborNode.Parent == null)
                {
                    neighborNode.GCost = newGCost;
                    neighborNode.HCost = Vector2Int.Distance(neighborNode.Position, targetPos);
                    neighborNode.Parent = currentNode;
                }
            }
        }

        Debug.LogWarning("[A*] Nenhum caminho encontrado.");
        return Vector2Int.zero; // Retorna zero se não encontrar caminho
    }
    private Vector2Int RetracePath(Node targetNode, Vector2Int startPos)
    {
        List<Vector2Int> path = new();
        Node currentNode = targetNode;

        // Reconstruindo o caminho de trás para frente
        while (currentNode.Parent != null)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        path.Reverse();

        // Agora retorna a direção do primeiro passo
        if (path.Count > 0)
        {
            Vector2Int nextStep = path[0] - startPos;  // Calculando a direção em relação ao ponto de início
            return nextStep;
        }

        return Vector2Int.zero;
    }

    public void RecalculateEnemyAttacks()
    {
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