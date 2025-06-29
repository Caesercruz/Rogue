using System;
using System.Linq;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class MinimapManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject openMapPrefab;
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private GameObject RoomPrefab;
    [SerializeField] private GameObject RoomPrefabVisual;
    [SerializeField] private GameObject IntersectionPrefab;
    [SerializeField] private GameObject ArrowPrefab;
    public GameObject PlayerIconPrefab;
    public GameObject BossIconPrefab;

    [Header("Instances")]
    [SerializeField] private GameObject openMapInstance;
    [SerializeField] private GameObject hitboxInstance;
    public GameObject PlayerIconInstance;
    public GameObject BossIconInstance;

    [Header("Variables")]
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;
    [SerializeField] private float spawnIntersectionChance = .8f;
    [SerializeField] private float spawnRoomChance = 0.8f;

    [SerializeField] private int maxRooms = 20;
    [SerializeField] private GameScript gameScript;

    private GameObject playersRoomGO = null;
    private GameObject bossRoomGO = null;
    private Vector2Int lastInfectedPos;

    public RoomData[,] grid;
    private void Update()
    {
        if (gameScript.GameControls.Actions.OpenMap.triggered) { if (openMapInstance == null) ShowMap(); else CloseMap(); };
        
        if (openMapInstance == null) { return; };

        if (gameScript.GameControls.Actions.Back.triggered) { CloseMap(); };
        if (!_movement) return;

        if (gameScript.GameControls.MapMovement.Up.triggered) Move(0);
        if (gameScript.GameControls.MapMovement.Right.triggered) Move(1);
        if (gameScript.GameControls.MapMovement.Down.triggered) Move(2);
        if (gameScript.GameControls.MapMovement.Left.triggered) Move(3);
    }
    private void Move(int dir)
    {
        playersRoomGO.GetComponent<RoomData>().type = RoomData.Type.Nothing;
        if (!playersRoomGO.GetComponent<RoomData>().connections[dir])
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

        Vector2Int newPos = playersRoomGO.GetComponent<RoomData>().position + direction;
        playersRoomGO = gameObject.transform.Find($"Rooms/Room {newPos.x};{newPos.y}").gameObject;
        playersRoomGO.GetComponent<RoomData>().Explored = true;
        Destroy(gameObject.transform.Find("ArrowContainer").gameObject);
        _movement = false;
        gameScript.CleanScene();

        InfectRoom();
        UpdateMapVisual();

        CloseMap();

        if (playersRoomGO.GetComponent<RoomData>().type == RoomData.Type.Fight) gameScript.Combat(RoomData.Type.Fight);
        else if (playersRoomGO.GetComponent<RoomData>().type == RoomData.Type.Infected) gameScript.Combat(RoomData.Type.Infected);
        else if (playersRoomGO.GetComponent<RoomData>().type == RoomData.Type.Bossfight) gameScript.Combat(RoomData.Type.Bossfight);
        else if (playersRoomGO.GetComponent<RoomData>().type == RoomData.Type.Nothing) Empty();
    }
    public void Empty()
    {
        _movement = true;
        ShowMap(true);
    }
    bool _movement = false;
    public void ShowMap(bool movement = false)
    {
        if (movement == true) _movement = true;
        if (openMapInstance != null) return;

        hitboxInstance = Instantiate(hitboxPrefab, transform.parent);
        openMapInstance = Instantiate(openMapPrefab, transform.parent);
        openMapInstance.name = "Map";
        hitboxInstance.GetComponent<Button>().onClick.AddListener(() => GetComponent<MinimapManager>().CloseMap());

        Transform roomsParent = openMapInstance.transform.Find("Rooms");
        Transform intersectionsParent = openMapInstance.transform.Find("Intersections");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RoomData room = grid[x, y];
                if (room == null) continue;

                Vector3 pos = new(x, y, 0);
                GameObject roomGO = Instantiate(RoomPrefabVisual, Vector3.zero, Quaternion.identity, roomsParent);
                roomGO.transform.localPosition = pos;

                roomGO.name = $"Room {x};{y}";
                if (room.GetComponent<RoomData>().type == RoomData.Type.Bossfight ||
                    room.GetComponent<RoomData>().type == RoomData.Type.Infected)
                    roomGO.GetComponent<SpriteRenderer>().color = new(.876f, .327f, .38f, 1);
                if (room.Explored) roomGO.GetComponent<SpriteRenderer>().color = new(.676f, .827f, .38f, 1);

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

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height || grid[nx, ny] == null)
                        continue;

                    Vector3 interPos = new(x + dir.x * 0.5f, y + dir.y * 0.5f, 0);
                    GameObject interGO = Instantiate(IntersectionPrefab, Vector3.zero, Quaternion.identity, intersectionsParent);
                    interGO.transform.localPosition = interPos;
                    interGO.name = $"Intersection {interPos.x};{interPos.y}";

                    if (i == 0 || i == 2)
                        interGO.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
            }
        }
        Instantiate(PlayerIconPrefab, openMapInstance.transform.Find($"Rooms/{playersRoomGO.name}"));
        Instantiate(BossIconPrefab, openMapInstance.transform.Find($"Rooms/{bossRoomGO.name}"));

        if (movement || _movement) EnableMovement(playersRoomGO.GetComponent<RoomData>());

        gameScript.GameControls.PlayerControls.Disable();
    }
    public void CloseMap()
    {
        if (openMapInstance != null)
        {
            if (gameScript.Gamestate == GameScript.GameState.Combat) gameScript.GameControls.PlayerControls.Enable();
            Destroy(openMapInstance);
            openMapInstance = null;
            Destroy(hitboxInstance);
        }
    }
    public void GenerateMap()
    {
        grid = new RoomData[width, height];

        Vector2Int startPos = new(width / 2, height / 2);
        int roomsCreated = 0;
        CreateRoomRecursive(startPos, ref roomsCreated);
    }
    bool IsInsideBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
    public void ShowMiniMapIntersections()
    {
        Transform intersectionsParent = transform.Find("Intersections");

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
                    inter.name = $"Intersection {interPos.x};{interPos.y}";

                    if (i == 0 || i == 2)
                        inter.transform.rotation = Quaternion.Euler(0, 0, 90);
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
                if (room.type == RoomData.Type.Infected) gameObject.transform.Find($"Rooms/Room {x};{y}").GetComponent<SpriteRenderer>()
                        .color = new(.876f, .327f, .38f, 1);

                if (room.Explored) gameObject.transform.Find($"Rooms/Room {x};{y}").GetComponent<SpriteRenderer>()
                        .color = new(.676f, .827f, .38f, 1);
            }
        }
        Destroy(PlayerIconInstance);
        PlayerIconInstance = Instantiate(PlayerIconPrefab, gameObject.transform);
        PlayerIconInstance.transform.position = playersRoomGO.transform.position;
        PlayerIconInstance.transform.localScale = new(.45f, .5f, 1);
    }
    public void SpawnIcons()
    {
        RoomData mostBottomLeft = null;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RoomData room = grid[x, y];
                if (room == null) continue;

                if (mostBottomLeft == null ||
                    room.position.x < mostBottomLeft.position.x ||
                    (room.position.x == mostBottomLeft.position.x && room.position.y < mostBottomLeft.position.y))
                {
                    mostBottomLeft = room;
                }
            }
        }
        RoomData bossRoom = null;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RoomData room = grid[x, y];
                if (room == null) continue;

                if (bossRoom == null ||
                    room.position.x > bossRoom.position.x ||
                    (room.position.x == bossRoom.position.x && room.position.y > bossRoom.position.y))
                {
                    bossRoom = room;
                }
            }
        }

        if (mostBottomLeft != null)
        {
            playersRoomGO = GameObject.Find($"Room {mostBottomLeft.position.x};{mostBottomLeft.position.y}");
            PlayerIconInstance = Instantiate(PlayerIconPrefab, gameObject.transform);
            PlayerIconInstance.transform.position = playersRoomGO.transform.position;
            PlayerIconInstance.transform.localScale = new Vector3(.25f, .3f, 1);

            playersRoomGO.GetComponent<RoomData>().Explored = true;
            playersRoomGO.GetComponent<RoomData>().type = RoomData.Type.Fight;
            playersRoomGO.GetComponent<SpriteRenderer>().color = new(.676f, .827f, .38f, 1);
        }

        if (bossRoom != null)
        {
            bossRoomGO = GameObject.Find($"Room {bossRoom.position.x};{bossRoom.position.y}");
            lastInfectedPos = bossRoom.position;
            BossIconInstance = Instantiate(BossIconPrefab, gameObject.transform);
            BossIconInstance.transform.position = bossRoomGO.transform.position;
            BossIconInstance.transform.localScale = new Vector3(.25f, .3f, 1);

            bossRoomGO.GetComponent<RoomData>().type = RoomData.Type.Bossfight;
            bossRoomGO.GetComponent<SpriteRenderer>().color = new(.876f, .327f, .38f, 1);
        }
    }
    private void EnableMovement(RoomData room)
    {
        gameScript.GameControls.MapMovement.Enable();

        Transform minimapGO = gameScript.transform.Find("Canvas/Minimap");
        Transform mapGO = gameScript.transform.Find("Canvas/Map");

        CreateArrow(room, minimapGO);
        CreateArrow(room, mapGO);

    }
    private void CreateArrow(RoomData room, Transform map)
    {
        if (map.Find("ArrowContainer") != null) Destroy(map.Find("ArrowContainer").gameObject);

        // Criar novo container
        GameObject arrowContainerObj = new("ArrowContainer");
        arrowContainerObj.transform.parent = map;
        arrowContainerObj.transform.localPosition = (Vector3.zero);
        arrowContainerObj.transform.localScale = Vector3.one;
        arrowContainerObj.name = "ArrowContainer";

        // Direções: Cima, Direita, Baixo, Esquerda
        Vector2Int[] offsets = {
        new(0, 1),  // Cima
        new(1, 0),  // Direita
        new(0, -1), // Baixo
        new(-1, 0)  // Esquerda
    };
        float[] rotations = { 0f, 270f, 180f, 90f };

        for (int i = 0; i < 4; i++)
        {
            if (!room.connections[i]) continue;

            //Vector2Int targetPos = room.position + offsets[i];

            GameObject arrow = Instantiate(ArrowPrefab, arrowContainerObj.transform);
            arrow.name = $"Arrow_{i}";

            Transform arrowRoomPosition = arrowContainerObj.transform.Find("../Rooms");

            Transform RoomTransform = arrowRoomPosition.Find($"Room {room.position.x + offsets[i].x};{room.position.y + offsets[i].y}");

            Vector3 targetPos = RoomTransform.position;
            arrow.transform.SetPositionAndRotation(new(targetPos.x, targetPos.y, 0), Quaternion.Euler(0, 0, rotations[i]));
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

        if (UnityEngine.Random.value <= spawnRoomChance)
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
        GameObject roomGO = Instantiate(RoomPrefab, transform.Find("Rooms"));
        roomGO.name = $"Room {pos.x};{pos.y}";
        roomGO.transform.localPosition = new Vector3(pos.x, pos.y, 0);

        RoomData room = roomGO.GetComponent<RoomData>();
        room.position = pos;
        room.type = GetRandomRoomType();

        grid[pos.x, pos.y] = room;
        return room;
    }
    private void TryConnectRooms(RoomData room, int direction, RoomData neighbor)
    {
        if (UnityEngine.Random.value < spawnIntersectionChance)
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
    private RoomData.Type GetRandomRoomType()
    {
        float roomRandom = UnityEngine.Random.value;
        if (roomRandom <= 0.4f) return RoomData.Type.Fight;
        return RoomData.Type.Nothing;
    }
    private void InfectRoom()
    {
        Debug.Log($"Tentando infectar da sala {lastInfectedPos} até {playersRoomGO.GetComponent<RoomData>().position}");

        Vector2Int targetPos = playersRoomGO.GetComponent<RoomData>().position;

        Vector2Int direction = PathfindingUtility.AStarPathfinding(
            lastInfectedPos,
            targetPos,
            pos => IsInsideBounds(pos) && grid[pos.x, pos.y] != null,
            pos => false,
            out var path,
            CanMoveBetween // função que valida se há conexão
        );

        if (path == null || path.Count < 2)
        {
            Debug.LogWarning("Nenhum caminho válido de infecção encontrado.");
            return;
        }

        // Pula o primeiro (lastInfectedPos), percorre os próximos
        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int nextPos = path[i];
            RoomData room = grid[nextPos.x, nextPos.y];

            if (room == null)
            {
                Debug.LogWarning($"Erro: posição {nextPos} sem sala.");
                continue;
            }

            if (room.type != RoomData.Type.Infected)
            {
                room.type = RoomData.Type.Infected;
                lastInfectedPos = nextPos; // Atualiza a última infectada para próxima chamada
                Debug.Log($"Sala infectada em {nextPos}");
                return;
            }
            else
            {
                Debug.Log($"Sala em {nextPos} já estava infectada. Procurando próxima...");
            }
        }

        Debug.Log("Todas as salas no caminho já estavam infectadas.");
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
}