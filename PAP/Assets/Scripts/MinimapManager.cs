using System.Linq;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class MinimapManager : MonoBehaviour
{
    [SerializeField] private GameObject openMapPrefab;
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private GameObject RoomPrefab;
    [SerializeField] private GameObject RoomPrefabVisual;
    [SerializeField] private GameObject IntersectionPrefab;
    [SerializeField] private GameObject ArrowPrefab;
    [SerializeField] private GameScript gameScript;
    //public GameObject Canvas;

    [SerializeField] private GameObject openMapInstance;
    [SerializeField] private GameObject hitboxInstance;

    public readonly int width = 7;
    public readonly int height = 7;
    public RoomData[,] grid;
    private readonly int maxRooms = 35;

    public GameObject PlayerIconPrefab;
    private GameObject playersRoomGO = null;

    public GameObject BossIconPrefab;
    private GameObject bossRoomGO = null;

    public GameObject PlayerIconInstance;
    public GameObject BossIconInstance;
    private void Update()
    {
        if (openMapInstance != null)
        {
            if (gameScript.GameControls.Actions.Back.triggered) CloseMap();
            if (!_movement) return;
            if (gameScript.GameControls.MapMovement.Up.triggered) Move(0);
            if (gameScript.GameControls.MapMovement.Right.triggered) Move(1);
            if (gameScript.GameControls.MapMovement.Down.triggered) Move(2);
            if (gameScript.GameControls.MapMovement.Left.triggered) Move(3);
        }
    }
    private void Move(int dir)
    {
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
        CloseMap();
        UpdateMapVisual();
        gameScript.CleanScene();
        if (playersRoomGO.GetComponent<RoomData>().type == RoomData.Type.Fight) gameScript.Combat(false);
        if (playersRoomGO.GetComponent<RoomData>().type == RoomData.Type.Infected) gameScript.Combat(true);
        else gameScript.Combat(false);
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

                if (room.Explored) roomGO.GetComponent<SpriteRenderer>().color = new(.676f, .827f, .38f, 1);
                if (room.GetComponent<RoomData>().type == RoomData.Type.Bossfight || room.GetComponent<RoomData>().type == RoomData.Type.Infected) roomGO.GetComponent<SpriteRenderer>().color = new(.876f, .327f, .38f, 1);
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
        if (movement || _movement) EnableMovement(playersRoomGO.GetComponent<RoomData>());
        Instantiate(PlayerIconPrefab, openMapInstance.transform.Find($"Rooms/{playersRoomGO.name}"));
        Instantiate(BossIconPrefab, openMapInstance.transform.Find($"Rooms/{bossRoomGO.name}"));
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
    public void GenerateMiniMapIntersections()
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
                if (room.Explored) gameObject.transform.Find($"Rooms/Room {x};{y}").GetComponent<SpriteRenderer>()
                        .color = new(.676f, .827f, .38f, 1);
            }
        }
        Destroy(PlayerIconInstance);
        PlayerIconInstance = Instantiate(PlayerIconPrefab, gameObject.transform);
        PlayerIconInstance.transform.position = playersRoomGO.transform.position;
        PlayerIconInstance.transform.localScale = new(.25f, .3f, 1);
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
                    mostBottomRight = room;
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

        if (mostBottomRight != null)
        {
            playersRoomGO = GameObject.Find($"Room {mostBottomRight.position.x};{mostBottomRight.position.y}");
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

            Transform RoomTransform = arrowRoomPosition.Find($"Room {room.position.x + offsets[i].x};{room.position.y + offsets[i].y}");

            Vector3 targetPos = RoomTransform.position;
            arrow.transform.SetPositionAndRotation(new(targetPos.x, targetPos.y, 0), Quaternion.Euler(0, 0, rotations[i]));
        }
    }
    private void CreateRoomRecursive(Vector2Int pos, ref int roomsCreated)
    {
        if (roomsCreated >= maxRooms || grid[pos.x, pos.y] != null) return;

        GameObject roomGO = Instantiate(RoomPrefab, transform.Find("Rooms"));
        roomGO.name = $"Room {pos.x};{pos.y}";
        roomGO.transform.localPosition = new Vector3(pos.x, pos.y, 0);

        //Change RoomData atributes

        RoomData room = roomGO.GetComponent<RoomData>();
        room.position = pos;

        float roomRandom = Random.value;
        if (roomRandom <= .4f) room.type = RoomData.Type.Fight;
        else if (roomRandom <= .7f) room.type = RoomData.Type.Event;
        else room.type = RoomData.Type.Nothing;

        grid[pos.x, pos.y] = room;
        roomsCreated++;

        var directions = new (int index, Vector2Int dir)[]
        {
        (0, Vector2Int.up),
        (1, Vector2Int.right),
        (2, Vector2Int.down),
        (3, Vector2Int.left)
        }.OrderBy(_ => Random.value).ToArray();

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