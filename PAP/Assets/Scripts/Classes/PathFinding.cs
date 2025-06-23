using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PathfindingUtility
{
    public class Node
    {
        public Vector2Int Position;
        public float GCost;
        public float HCost;
        public float FCost => GCost + HCost;
        public Node Parent;

        public Node(Vector2Int position) => Position = position;
    }

    public static Vector2Int AStarPathfinding(
    Vector2Int startPos,
    Vector2Int targetPos,
    System.Func<Vector2Int, bool> isValidPosition,
    System.Func<Vector2Int, bool> isBlocked,
    out List<Vector2Int> fullPath,
    System.Func<Vector2Int, Vector2Int, bool> canMove = null // novo parâmetro opcional
)

    {
        List<Node> openList = new();
        HashSet<Vector2Int> closedList = new();
        Node startNode = new(startPos);
        openList.Add(startNode);
        fullPath = new();

        while (openList.Count > 0)
        {
            openList.Sort((a, b) => a.FCost.CompareTo(b.FCost));
            Node current = openList[0];
            openList.RemoveAt(0);

            if (current.Position == targetPos)
            {
                fullPath = RetracePath(current);
                return fullPath.Count > 1 ? fullPath[1] - startPos : Vector2Int.zero;
            }

            closedList.Add(current.Position);

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighborPos = current.Position + dir;

                if (!isValidPosition(neighborPos) ||
    (isBlocked(neighborPos) && neighborPos != targetPos) ||
    closedList.Contains(neighborPos) ||
    (canMove != null && !canMove(current.Position, neighborPos)))
                {
                    continue;
                }


                float newG = current.GCost + 1;
                Node neighbor = openList.FirstOrDefault(n => n.Position == neighborPos);

                if (neighbor == null)
                {
                    neighbor = new Node(neighborPos);
                    openList.Add(neighbor);
                }

                if (newG < neighbor.GCost || neighbor.Parent == null)
                {
                    neighbor.GCost = newG;
                    neighbor.HCost = Vector2Int.Distance(neighborPos, targetPos);
                    neighbor.Parent = current;
                }
            }
        }

        Debug.LogWarning("[A*] Caminho não encontrado.");
        return Vector2Int.zero;

    }

    private static List<Vector2Int> RetracePath(Node endNode)
    {
        List<Vector2Int> path = new();
        Node current = endNode;

        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }
}
