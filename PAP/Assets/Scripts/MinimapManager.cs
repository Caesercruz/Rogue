using System.Linq;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class MinimapManager : MonoBehaviour
{
    [SerializeField] private GameObject openMapPrefab;
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private GameObject RoomPrefab;
    [SerializeField] private GameObject IntersectionPrefab;
    [SerializeField] private GameObject ArrowPrefab;
    [SerializeField] private GameScript gameScript;
    public GameObject Canvas;

    [SerializeField] private GameObject openMapInstance;
    [SerializeField] private GameObject hitboxInstance;

    public readonly int width = 7;
    public readonly int height = 7;
    public RoomData[,] grid;
    private readonly int maxRooms = 35;
    private RoomData playersRoom = null;

    public GameObject PlayerIconPrefab;
    public GameObject BossIconPrefab;

    private void Update()
    {
        if (openMapInstance != null)
        {
            if (gameScript.GameControls.Actions.Back.triggered) CloseMap();
            if (gameScript.GameControls.MapMovement.Up.triggered) Move(0);
            if (gameScript.GameControls.MapMovement.Right.triggered) Move(1);
            if (gameScript.GameControls.MapMovement.Down.triggered) Move(2);
            if (gameScript.GameControls.MapMovement.Left.triggered) Move(3);
        }
    }
    private void Move(int dir)
    {
        if (!playersRoom.connections[dir])
        {
            //Error Animation(Player goes to the direction "hits" the room wall, glows red and vibrates)
            return;
        }
        Vector2Int direction = dir switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.right,
            2 => Vector2Int.left,
            3 => Vector2Int.down,
            _ => Vector2Int.zero
        };

        Vector2Int newPos = playersRoom.position + direction;

        RoomData newRoom = grid[newPos.x, newPos.y];

        playersRoom = newRoom;

    }
    public void ShowMap(bool movement = false)
    {
        if (openMapInstance != null) return;

        hitboxInstance = Instantiate(hitboxPrefab, Canvas.transform);
        openMapInstance = Instantiate(openMapPrefab, Canvas.transform);
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
                GameObject roomGO = Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, roomsParent);
                roomGO.transform.localPosition = pos;

                roomGO.name = $"Room {x};{y}";

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
                if (room.PlayerInside)
                {
                    Instantiate(PlayerIconPrefab, roomGO.transform);
                    playersRoom = room;
                }
            }
        }
        if (movement) EnableMovement(playersRoom);

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

    public void GenerateMiniMapVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RoomData room = grid[x, y];
                if (room == null) continue;

                Vector3 pos = new(x, y, 0);
                GameObject roomGO = Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, gameObject.transform.Find("Rooms"));
                roomGO.transform.localPosition = pos;

                roomGO.name = $"Room {x};{y}";
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

                    // Verifica se a sala vizinha existe antes de instanciar a interseção

                    if (!IsInsideBounds(new Vector2Int(nx, ny)) || grid[nx, ny] == null) continue;

                    Vector3 interPos = new(x + dir.x * 0.5f, y + dir.y * 0.5f, 0);
                    GameObject interGO = Instantiate(IntersectionPrefab, Vector3.zero, Quaternion.identity, gameObject.transform.Find("Intersections"));
                    interGO.transform.localPosition = interPos;
                    interGO.name = $"Intersection {x + dir.x * 0.5f};{y + dir.y * 0.5f}";

                    if (i == 0 || i == 2)
                        interGO.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
            }
        }
    }

    public void SpawnIcons()
    {
        RoomData mostBottomRight = null;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RoomData room = grid[x, y];
                if (room == null) continue;

                if (mostBottomRight == null ||
                    room.position.x < mostBottomRight.position.x ||
                    (room.position.x == mostBottomRight.position.x && room.position.y < mostBottomRight.position.y))
                {
                    room.PlayerInside = true;
                    mostBottomRight = room;
                }
            }
        }

        if (mostBottomRight != null)
        {
            GameObject roomGO = GameObject.Find($"Room {mostBottomRight.position.x};{mostBottomRight.position.y}");
            Instantiate(PlayerIconPrefab, roomGO.transform);
            mostBottomRight.PlayerInside = true;
            mostBottomRight.Explored = true;
            roomGO.GetComponent<Renderer>().enabled = true;
            roomGO.GetComponent<SpriteRenderer>().color = new(.676f, .827f, .38f, 1);//Fafas
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
        if (map.Find("ArrowContainer") != null) return;
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
            Debug.Log($"Rooms name: {arrowRoomPosition.name}");
            Debug.Log($"Room {room.position.x + offsets[i].x};{room.position.y + offsets[i].y}");

            Transform RoomTransform = arrowRoomPosition.Find($"Room {room.position.x + offsets[i].x};{room.position.y + offsets[i].y}");

            Debug.Log(RoomTransform.name);
            Vector3 targetPos = RoomTransform.position;
            arrow.transform.SetPositionAndRotation(new(targetPos.x, targetPos.y, 0), Quaternion.Euler(0, 0, rotations[i]));
        }
    }
    void CreateRoomRecursive(Vector2Int pos, ref int roomsCreated)
    {
        if (roomsCreated >= maxRooms || grid[pos.x, pos.y] != null) return;

        RoomData room = new() { position = pos };
        grid[pos.x, pos.y] = room;
        room.Type = RoomData.Type.Fight;
        roomsCreated++;

        var directions = new (int index, Vector2Int dir)[]
        {
        (0, Vector2Int.up),
        (1, Vector2Int.right),
        (2, Vector2Int.down),
        (3, Vector2Int.left)
        }.OrderBy(_ => Random.value).ToArray();

        //Se a sala adjacente existe e o random passa com 70% cria a interceção. Faz isso para cada direção
        foreach (var (i, dir) in directions)
        {
            Vector2Int nextPos = pos + dir;
            if (!IsInsideBounds(nextPos)) continue;
            if (grid[nextPos.x, nextPos.y] != null && Random.value >= 0.7f) return;

            room.connections[i] = true;
            CreateRoomRecursive(nextPos, ref roomsCreated);

            if (grid[nextPos.x, nextPos.y] == null)
            {
                room.connections[i] = false;
                return;
            }
            int opposite = (i + 2) % 4;
            grid[nextPos.x, nextPos.y].connections[opposite] = true;
        }
    }
}