using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[System.Serializable]
public class SaveData
{
    public List<SerializableRoomData> allRooms;
    public Vector2Int playerRoomPos;
    public Vector2Int bossRoomPos;
}

[System.Serializable]
public class SerializableRoomData
{
    public Vector2Int position;
    public RoomData.Type type;
    public bool[] connections;
    public bool explored;
    public int infectedStatus;

    public SerializableRoomData(RoomData room)
    {
        position = room.position;
        type = room.type;
        connections = room.connections;
        explored = room.Explored;
        infectedStatus = room.infectedStatus;
    }

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
