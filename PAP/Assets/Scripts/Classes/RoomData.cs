using System;
using UnityEngine;

public class RoomData
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
}