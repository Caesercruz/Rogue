using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<SerializableRoomData> allRooms;
    public Vector2Int playerRoomPos;
    public Vector2Int bossRoomPos;

    public List<SerializablePerk> activePerks;
    public int Health;
}

[System.Serializable]
public class SerializableRoomData
{
    public Vector2Int position;
    public RoomData.Type type;
    public bool[] connections;
    public bool explored;
    public int infectedStatus;

    //Convert to Serializable
    public SerializableRoomData(RoomData room)
    {
        position = room.position;
        type = room.type;
        connections = room.connections;
        explored = room.Explored;
        infectedStatus = room.infectedStatus;
    }
    //Convert to RoomData
    public RoomData ToRoomData()
    {
        RoomData room = new();
        room.position = position;
        room.type = type;
        room.connections = connections;
        room.Explored = explored;
        room.infectedStatus = infectedStatus;
        return room;
    }
}

[System.Serializable]
public class SerializablePerk
{
    public string Name;
    public string PerkType;

    public SerializablePerk(Perk perk)
    {
        Name = perk.name;
        PerkType = perk.type.ToString();
    }

    public PerkType GetPerkType()
    {
        return (PerkType)System.Enum.Parse(typeof(PerkType), PerkType);
    }
}