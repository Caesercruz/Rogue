using System;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    public Vector2Int position;
    public bool[] connections = new bool[4];
    public bool Explored = false;
    public int infectedStatus = 0;

    public enum Type
    {
        Fight,
        Event,
        Infected,
        Bossfight,
        Nothing,
    }
    public Type type;
    public bool IsConnectedTo(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return connections[0];
        if (direction == Vector2Int.right) return connections[1];
        if (direction == Vector2Int.down) return connections[2];
        if (direction == Vector2Int.left) return connections[3];
        return false;
    }
}