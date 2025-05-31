using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using System.Linq;

public class MinimapManager : MonoBehaviour
{
    [SerializeField] private GameObject openMapPrefab;
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private GameObject RoomPrefab;
    [SerializeField] private GameObject IntersectionPrefab;
    [SerializeField] private GameScript gameScript;
    public GameObject Canvas;

    [SerializeField] private GameObject openMapInstance;
    [SerializeField] private GameObject hitboxInstance;

    public readonly int width = 7;
    public readonly int height = 7;
    public RoomData[,] grid;
    private readonly int maxRooms = 35;

    public GameObject PlayerIconPrefab;
    public GameObject BossIconPrefab;
    public void ShowMap()
    {
        if (openMapInstance != null) return;

        hitboxInstance = Instantiate(hitboxPrefab, Canvas.transform);
        openMapInstance = Instantiate(openMapPrefab, Canvas.transform);
        hitboxInstance.GetComponent<Button>().onClick.AddListener(() => GetComponent<MinimapManager>().HideMap());

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
                
                if(room.Explored) roomGO.GetComponent<SpriteRenderer>().color = new(.676f, .827f, .38f, 1);
                if(room.PlayerInside) Instantiate(PlayerIconPrefab, roomGO.transform);
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
        
        gameScript.playerInstance.GetComponent<Player>().controls.Actions.Disable();
    }

    public void HideMap()
    {
        if (openMapInstance != null)
        {
            gameScript.playerInstance.GetComponent<Player>().controls.Actions.Enable();
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
    void CreateRoomRecursive(Vector2Int pos, ref int roomsCreated)
    {
        if (roomsCreated >= maxRooms || grid[pos.x, pos.y] != null) return;

        RoomData room = new() { position = pos };
        grid[pos.x, pos.y] = room;
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

    bool IsInsideBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public void GenerateMapVisuals()
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
}

public class RoomData
{
    public Vector2Int position;
    public bool[] connections = new bool[4];
    public bool Explored = false;
    public bool PlayerInside = false;
}
