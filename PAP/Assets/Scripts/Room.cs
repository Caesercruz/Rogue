using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int x;
    public int y;
    public List<Vector2Int> connections = new();
    public bool Explored = false;
    public Room(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}