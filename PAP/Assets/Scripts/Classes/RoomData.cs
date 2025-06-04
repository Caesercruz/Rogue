using UnityEngine;

public class RoomData : MonoBehaviour
{
    public Vector2Int position;
    public bool[] connections = new bool[4];
    public bool Explored = false;
    //public bool PlayerInside = false;

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