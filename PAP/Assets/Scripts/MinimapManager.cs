using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class MinimapManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject openMapPrefab;
    [SerializeField] private GameObject HitboxPrefab;
    [SerializeField] private GameObject RoomPrefab;
    [SerializeField] private GameObject RoomPrefabVisual;
    [SerializeField] private GameObject IntersectionPrefab;
    [SerializeField] private GameObject ArrowPrefab;
    public GameObject PlayerIconPrefab;
    public GameObject BossIconPrefab;

    [Header("Instances")]
    public GameObject PlayerIconInstance;
    public GameObject BossIconInstance;

    [Header("Variables")]
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;
    [SerializeField] Color ExploredColor = new(.676f, .827f, .38f, 1);
    [SerializeField] Color BossColor = new(.2f, .2f, .2f, 1);
    [SerializeField] Color Infected1Color = new(.876f, .327f, .38f, 1);
    [SerializeField] Color Infected2Color = new(.42f, .16f, .17f, 1);
    [SerializeField] private float spawnIntersectionChance = .8f;
    [SerializeField] private float spawnRoomChance = 0.8f;

    [SerializeField] private int maxRooms = 20;
    [SerializeField] private GameScript gameScript;
    public RoomData playersRoom;
    public RoomData bossRoom;

    private Vector2Int lastInfectedPos;

    public RoomData[,] grid;
    private void Update()
    {
        if (gameScript.GameControls.Actions.OpenMap.triggered) { if (gameScript.MenuInstance == null) OpenMap(); else gameScript.CloseAction(); };

        if (gameScript.MenuInstance == null) { return; };

        if (gameScript.GameControls.Actions.Back.triggered) { gameScript.CloseAction(); };
        if (!_movement) return;

        if (gameScript.GameControls.MapMovement.Up.triggered) Move(0);
        if (gameScript.GameControls.MapMovement.Right.triggered) Move(1);
        if (gameScript.GameControls.MapMovement.Down.triggered) Move(2);
        if (gameScript.GameControls.MapMovement.Left.triggered) Move(3);
    }
    public void Load()
    {
        string path = Application.persistentDataPath + "/save.json";
        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found. Creating new map instead.");
            GenerateMap(); // fallback se não tiver save
            return;
        }

        string json = File.ReadAllText(path);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        // Reconstrói a grid a partir dos dados salvos
        RebuildGridFromSave(saveData);

        // Reatribui as referências do jogador e do boss
        playersRoom = grid[saveData.playerRoomPos.x, saveData.playerRoomPos.y];
        bossRoom = grid[saveData.bossRoomPos.x, saveData.bossRoomPos.y];

        Debug.Log("Mapa carregado com sucesso.");
        
        void RebuildGridFromSave(SaveData saveData)
        {
            // Descobre tamanho da grid com base nas posições salvas
            int width = saveData.allRooms.Max(r => r.position.x) + 1;
            int height = saveData.allRooms.Max(r => r.position.y) + 1;

            grid = new RoomData[width, height];

            foreach (SerializableRoomData room in saveData.allRooms)
            {
                RoomData newRoom = room.ToRoomData();
                grid[newRoom.position.x, newRoom.position.y] = newRoom;
            }
        }
    }
    public void Save()
    {

        var allRooms = new List<SerializableRoomData>();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                RoomData room = grid[x, y];
                if (room != null)
                    allRooms.Add(new SerializableRoomData(room));
            }
        }

        SaveData saveData = new SaveData
        {
            allRooms = allRooms,
            playerRoomPos = playersRoom.position,
            bossRoomPos = bossRoom.position
        };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }
    private void Move(int dir)
    {
        playersRoom.type = RoomData.Type.Nothing;
        Save();
        if (!playersRoom.connections[dir])
        {
            //Error Animation(Player goes to the direction "hits" the room wall, glows red and vibrates)
            return;
        }
        Vector2Int direction = dir switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.right,
            2 => Vector2Int.down,
            3 => Vector2Int.left,
            _ => Vector2Int.zero
        };
        Vector2Int newPos = playersRoom.position + direction;

        // Atualizar para a nova sala real do mapa
        RoomData newRoom = grid[newPos.x, newPos.y];
        if (newRoom == null)
        {
            Debug.LogError($"Sala em {newPos} não encontrada!");
            return;
        }

        playersRoom = newRoom; // <-- Agora playersRoom aponta para a nova sala do mapa

        playersRoom.Explored = true;
        playersRoom.infectedStatus = 0;

        Destroy(transform.Find("ArrowContainer").gameObject);
        _movement = false;
        gameScript.CleanScene();

        InfectRoom();
        UpdateMapVisual();

        gameScript.CloseAction();

        if (playersRoom.type == RoomData.Type.Fight) gameScript.Combat(RoomData.Type.Fight);
        else if (playersRoom.type == RoomData.Type.Infected) gameScript.Combat(RoomData.Type.Infected);
        else if (playersRoom.type == RoomData.Type.Bossfight) gameScript.Combat(RoomData.Type.Bossfight);
        else if (playersRoom.type == RoomData.Type.Nothing) Empty();
    }
    public void Empty()
    {
        _movement = true;
        OpenMap(true);
    }
    bool _movement = false;
    public void OpenMap(bool movement = false)
    {
        if (movement == true) _movement = true;
        if (gameScript.MenuInstance != null) return;

        gameScript.hitboxInstance = Instantiate(HitboxPrefab, transform.parent);
        gameScript.hitboxInstance.GetComponent<Button>().onClick.AddListener(() => gameScript.CloseAction());

        gameScript.MenuInstance = Instantiate(openMapPrefab, transform.parent);
        gameScript.MenuInstance.name = "Map";

        LoadMap(gameScript.MenuInstance);

        Instantiate(PlayerIconPrefab, gameScript.MenuInstance.transform.Find($"Rooms/Room {playersRoom.position.x} {playersRoom.position.y}").transform);
        Instantiate(BossIconPrefab, gameScript.MenuInstance.transform.Find($"Rooms/Room {bossRoom.position.x} {bossRoom.position.y}").transform);

        if (movement || _movement) EnableMovement(playersRoom);

        gameScript.GameControls.PlayerControls.Disable();
    }
    public void GenerateMap()
    {
        grid = new RoomData[width, height];

        Vector2Int startPos = new(width / 2, height / 2);
        int roomsCreated = 0;
        CreateRoomRecursive(startPos, ref roomsCreated);
    }
    public void LoadMap(GameObject parent)
    {
        LoadRooms(parent);
        LoadIntersections(parent);

        void LoadRooms(GameObject parent)
        {
            Transform roomsParent = parent.transform.Find("Rooms");

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    RoomData room = grid[x, y];
                    if (room == null) continue;

                    GameObject roomInstance = Instantiate(RoomPrefab, roomsParent.transform);
                    roomInstance.name = $"Room {room.position.x} {room.position.y}";
                    roomInstance.transform.localPosition = new(x, y, 0);
                    if (room.Explored) roomInstance.GetComponent<Image>().color = ExploredColor;
                    if (room.infectedStatus == 1) roomInstance.GetComponent<Image>().color = Infected1Color;
                    if (room.infectedStatus == 2) roomInstance.GetComponent<Image>().color = Infected2Color;
                }
            }
        }
        void LoadIntersections(GameObject parent)
        {
            Transform intersectionsParent = parent.transform.Find("Intersections");

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    RoomData room = grid[x, y];
                    if (room == null) continue;

                    for (int i = 0; i < 4; i++)
                    {
                        if (!room.connections[i]) continue;

                        Vector2Int dir = i switch
                        {
                            0 => Vector2Int.up,
                            1 => Vector2Int.right,
                            2 => Vector2Int.down,
                            3 => Vector2Int.left,
                            _ => Vector2Int.zero
                        };

                        int nx = x + dir.x;
                        int ny = y + dir.y;

                        if (nx < x || ny < y) continue;

                        Vector3 interPos = new(x + dir.x * 0.5f, y + dir.y * 0.5f, 0);
                        GameObject inter = Instantiate(IntersectionPrefab, Vector3.zero, Quaternion.identity, intersectionsParent);
                        inter.transform.localPosition = interPos;
                        inter.name = $"Intersection {interPos.x} {interPos.y}";

                        if (i == 0 || i == 2)
                            inter.transform.rotation = Quaternion.Euler(0, 0, 90);
                    }
                }
            }
        }
    }
    public void UpdateMapVisual()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RoomData room = grid[x, y];
                if (room == null) continue;
                if (room.Explored) gameObject.transform.Find($"Rooms/Room {x} {y}").GetComponent<Image>()
                        .color = ExploredColor;
                if (room.infectedStatus == 1) gameObject.transform.Find($"Rooms/Room {x} {y}").GetComponent<Image>()
                        .color = Infected1Color;
                if (room.infectedStatus == 2) gameObject.transform.Find($"Rooms/Room {x} {y}").GetComponent<Image>()
                        .color = Infected2Color;
            }
        }
        Destroy(PlayerIconInstance);
        PlayerIconInstance = Instantiate(PlayerIconPrefab, transform.Find("IconsContainer").transform);
        PlayerIconInstance.transform.localPosition = new(playersRoom.position.x, playersRoom.position.y, 0);
        PlayerIconInstance.transform.localScale = new(.1f, .1f, 1);
    }
    public void SpawnIcons()
    {
        if (bossRoom != null || playersRoom != null)
        {
            LoadIcons();
            return;
        }
        RoomData mostBottomLeft = null;
        RoomData mostTopRight = null;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RoomData room = grid[x, y];
                if (room == null) continue;

                if (mostTopRight == null ||
                    room.position.x > mostTopRight.position.x ||
                    (room.position.x == mostTopRight.position.x && room.position.y > mostTopRight.position.y))
                {
                    mostTopRight = room;
                }

                if (mostBottomLeft == null ||
                    room.position.x < mostBottomLeft.position.x ||
                    (room.position.x == mostBottomLeft.position.x && room.position.y < mostBottomLeft.position.y))
                {
                    mostBottomLeft = room;
                }
            }
        }

        playersRoom = mostBottomLeft;
        bossRoom = mostTopRight;

        LoadIcons();
        playersRoom.type = RoomData.Type.Fight;
        bossRoom.type = RoomData.Type.Bossfight;

        void LoadIcons()
        {
            PlayerIconInstance = Instantiate(PlayerIconPrefab, gameObject.transform.Find("IconsContainer").transform);
            PlayerIconInstance.transform.localPosition = new(playersRoom.position.x, playersRoom.position.y, 0);
            PlayerIconInstance.transform.localScale = new Vector3(.1f, .1f, 1);

            playersRoom.Explored = true;
            transform.Find($"Rooms/Room {playersRoom.position.x} {playersRoom.position.y}").GetComponent<Image>().color = ExploredColor;

            lastInfectedPos = bossRoom.position;
            BossIconInstance = Instantiate(BossIconPrefab, gameObject.transform.Find("IconsContainer").transform);
            BossIconInstance.transform.localPosition = new(bossRoom.position.x, bossRoom.position.y, 0);
            BossIconInstance.transform.localScale = new Vector3(.08f, .09f, 1);
        }
    }
    private void EnableMovement(RoomData room)
    {
        gameScript.GameControls.MapMovement.Enable();

        Transform minimapGO = transform;
        Transform mapGO = transform.parent.Find("Map");

        CreateArrows(room, minimapGO);
        CreateArrows(room, mapGO);
    }
    private void CreateArrows(RoomData room, Transform map)
    {
        if (map.Find("ArrowContainer") != null && map.GetComponent<MinimapManager>() != null) Destroy(map.Find("ArrowContainer").gameObject);

        // Criar novo container
        GameObject arrowContainerObj = new("ArrowContainer");
        arrowContainerObj.transform.parent = map;
        arrowContainerObj.transform.localPosition = (Vector3.zero);
        arrowContainerObj.transform.localScale = Vector3.one;
        
        // Direções: Cima, Direita, Baixo, Esquerda
        Vector2Int[] offsets = {
        Vector2Int.up,  // Cima
        Vector2Int.right,  // Direita
        Vector2Int.down, // Baixo
        Vector2Int.left  // Esquerda
    };
        float[] rotations = { 0f, 270f, 180f, 90f };

        for (int i = 0; i < 4; i++)
        {
            if (!room.connections[i]) continue;

            int dir = i;

            GameObject arrow = Instantiate(ArrowPrefab, arrowContainerObj.transform);
            arrow.name = $"Arrow_{dir}";
            if(arrowContainerObj.transform.parent.GetComponents<MinimapManager>() ==  null) 
                arrow.AddComponent<Button>().onClick.AddListener(() => Move(dir));

            Transform RoomsContainer = map.transform.Find("Rooms");
            Transform RoomGlobalPosition = RoomsContainer.Find($"Room {room.position.x + offsets[dir].x} {room.position.y + offsets[dir].y}");

            Vector3 ArrowPosition = RoomGlobalPosition.position;
            arrow.transform.SetPositionAndRotation(new(ArrowPosition.x, ArrowPosition.y, 0), Quaternion.Euler(0, 0, rotations[dir]));
        }
    }
    private void CreateRoomRecursive(Vector2Int pos, ref int roomsCreated)
    {
        if (roomsCreated >= maxRooms || grid[pos.x, pos.y] != null) return;

        RoomData room = CreateRoomAt(pos);
        roomsCreated++;

        foreach (var (i, dir) in GetShuffledDirections())
        {
            Vector2Int nextPos = pos + dir;

            if (!IsInsideBounds(nextPos)) continue;

            RoomData neighbor = grid[nextPos.x, nextPos.y];

            if (neighbor != null)
            {
                TryConnectRooms(room, i, neighbor);
                continue;
            }

            if (Random.value <= spawnRoomChance)
            {
                room.connections[i] = true;
                CreateRoomRecursive(nextPos, ref roomsCreated);

                RoomData created = grid[nextPos.x, nextPos.y];
                if (created == null)
                {
                    room.connections[i] = false;
                }
                else
                {
                    int opp = (i + 2) % 4;
                    created.connections[opp] = true;
                }
            }
        }
    }
    private RoomData CreateRoomAt(Vector2Int pos)
    {
        RoomData roomData = new()
        {
            // Inicializa os dados da sala
            position = pos,
            connections = new bool[4], // Cima, Direita, Baixo, Esquerda
            type = GetRandomRoomType(),
            infectedStatus = 0,
            Explored = false
        };

        // Salva na grade
        grid[pos.x, pos.y] = roomData;

        return roomData;
    }
    private void TryConnectRooms(RoomData room, int direction, RoomData neighbor)
    {
        if (Random.value < spawnIntersectionChance)
        {
            room.connections[direction] = true;
            int opp = (direction + 2) % 4;
            neighbor.connections[opp] = true;
        }
    }
    private (int, Vector2Int)[] GetShuffledDirections()
    {
        return new (int, Vector2Int)[]
        {
        (0, Vector2Int.up),
        (1, Vector2Int.right),
        (2, Vector2Int.down),
        (3, Vector2Int.left)
        }.OrderBy(_ => UnityEngine.Random.value).ToArray();
    }
    private void InfectRoom()
    {
        Debug.Log($"A infectar a sala {lastInfectedPos} até {playersRoom.position}");

        //Room infection in tier 1
        foreach (var room in grid)
        {
            if (room != null && room.infectedStatus == 1)
            {
                room.infectedStatus = 2;
                room.type = RoomData.Type.Infected;
                lastInfectedPos = room.position;
                Debug.Log($"Sala em {room.position} foi infectada.");
                return;
            }
        }

        Vector2Int targetPos = playersRoom.position;

        Vector2Int direction = PathfindingUtility.AStarPathfinding(
            lastInfectedPos,
            targetPos,
            pos => IsInsideBounds(pos) && grid[pos.x, pos.y] != null,
            pos => false,
            out var path,
            CanMoveBetween
        );

        if (path == null || path.Count < 2)
        {
            Debug.LogWarning("Nenhum caminho válido de infecção encontrado.");
            return;
        }

        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int nextPos = path[i];
            RoomData room = grid[nextPos.x, nextPos.y];

            if (room == null)
            {
                Debug.LogWarning($"Erro: posição {nextPos} sem sala.");
                continue;
            }

            if (room.infectedStatus == 0 && room.type != RoomData.Type.Infected)
            {
                room.infectedStatus = 1;
                Debug.Log($"Sala marcada para infecção futura em {nextPos}");
                return;
            }
            else
            {
                Debug.Log($"Sala em {nextPos} já está infectada ou marcada.");
            }
        }

        Debug.Log("Todas as salas no caminho já estavam marcadas ou infectadas.");
    }
    private bool CanMoveBetween(Vector2Int from, Vector2Int to)
    {
        Vector2Int dir = to - from;

        int fromIndex = -1;
        if (dir == Vector2Int.up) fromIndex = 0;
        else if (dir == Vector2Int.right) fromIndex = 1;
        else if (dir == Vector2Int.down) fromIndex = 2;
        else if (dir == Vector2Int.left) fromIndex = 3;

        int toIndex = -1;
        if (dir == Vector2Int.down) toIndex = 0;
        else if (dir == Vector2Int.left) toIndex = 1;
        else if (dir == Vector2Int.up) toIndex = 2;
        else if (dir == Vector2Int.right) toIndex = 3;

        if (fromIndex == -1 || toIndex == -1)
            return false;

        RoomData fromRoom = grid[from.x, from.y];
        RoomData toRoom = grid[to.x, to.y];

        return fromRoom != null && toRoom != null &&
               fromRoom.connections[fromIndex] && toRoom.connections[toIndex];
    }
    private bool IsInsideBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
    private RoomData.Type GetRandomRoomType()
    {
        float roomRandom = UnityEngine.Random.value;
        if (roomRandom <= 0.75f) return RoomData.Type.Fight;
        return RoomData.Type.Nothing;
    }
}