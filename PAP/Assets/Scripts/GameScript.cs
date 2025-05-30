using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameScript : MonoBehaviour
{
    private Actors actors;

    public enum GameState
    {
        Combat,
        Menu,
        LostRun,
        WonEncounter,
    }
    public enum Encounter
    {
        Rats_2,
        Rats_3,
    }
    public List<Perk> ActivePerks = new();

    [SerializeField] private TextMeshProUGUI _txt_energy;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _cam;
    public Canvas UpdateScreen;
    public GameObject _playerInstance;
    public GameObject _ratInstance;
    [HideInInspector] public int NumberOfEnemies = 0;

    public readonly int Width = 8, Height = 6;

    public GameState Gamestate;

    public GameObject RoomPrefab;
    public GameObject IntersectionPrefab;
    public Transform MapContainer;

    public readonly int width = 7;
    public readonly int height = 7;
    public RoomData[,] grid;
    private readonly int maxRooms = 35;

    private void Start()
    {
        Gamestate = GameState.Combat;
        actors = GameObject.Find("BoardManager").GetComponent<Actors>();

        Gameplay();
    }

    private void Gameplay()
    {
        GenerateMap();
        GenerateMapVisuals();
        GenerateGrid();

        actors.SpawnCharacter(_playerInstance, "Player", true);

        Encounter currentEncounter = (Encounter)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Encounter)).Length);
        Debug.Log("Encounter: " + currentEncounter);

        switch (currentEncounter)
        {
            case Encounter.Rats_2:
                for (NumberOfEnemies = 0; NumberOfEnemies < 2; NumberOfEnemies++)
                {
                    actors.SpawnCharacter(_ratInstance, $"Rat {NumberOfEnemies}", false);
                }
                TransformHealthBars();
                break;
            case Encounter.Rats_3:
                for (NumberOfEnemies = 0; NumberOfEnemies < 3; NumberOfEnemies++)
                {
                    actors.SpawnCharacter(_ratInstance, $"Rat {NumberOfEnemies}", false);
                }
                TransformHealthBars();
                break;
            default:
                actors.SpawnCharacter(_ratInstance, $"Rat {NumberOfEnemies}", false);
                break;
        }
        TransformHealthBars();
    }

    public void TransformHealthBars()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy.HealthBar != null)
            {
                int offsetY = 0 * i;

                RectTransform rectTransform = enemy.HealthBar.transform as RectTransform;
                Vector3 originalPos = rectTransform.localPosition;
                rectTransform.localPosition = new Vector3(originalPos.x, originalPos.y + offsetY, originalPos.z);
            }
        }
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                float posX = x;
                float posY = y;
                Vector3 position = new(posX, posY, 0);
                var spawnedTile = Instantiate(_tilePrefab, position, Quaternion.identity, actors.transform);
                spawnedTile.name = $"Tile {x} {y}";
                actors.GridTiles[new Vector2Int(x, y)] = spawnedTile;
                bool isOffSet = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffSet);
            }
        }
    }

    void GenerateMap()
    {
        grid = new RoomData[width, height];

        Vector2Int startPos = new(width / 2, height / 2);
        int roomsCreated = 0;
        CreateRoomRecursive(startPos, ref roomsCreated);
    }
    void CreateRoomRecursive(Vector2Int pos, ref int roomsCreated)
    {
        if (roomsCreated >= maxRooms || grid[pos.x, pos.y] != null)
            return;

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

        foreach (var (i, dir) in directions)
        {
            Vector2Int nextPos = pos + dir;
            if (!IsInsideBounds(nextPos)) continue;

            if (grid[nextPos.x, nextPos.y] == null && Random.value < 0.7f)
            {
                room.connections[i] = true;
                CreateRoomRecursive(nextPos, ref roomsCreated);

                if (grid[nextPos.x, nextPos.y] != null)
                {
                    int opposite = (i + 2) % 4;
                    grid[nextPos.x, nextPos.y].connections[opposite] = true;
                }
                else
                {
                    room.connections[i] = false;
                }
            }
        }
    }

    bool IsInsideBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    void GenerateMapVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                RoomData room = grid[x, y];
                if (room == null) continue;

                Vector3 pos = new(x, y, 0);
                GameObject roomGO = Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, MapContainer.Find("Rooms"));
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
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height || grid[nx, ny] == null)
                        continue;

                    Vector3 interPos = new(x + dir.x * 0.5f, y + dir.y * 0.5f, 0);
                    GameObject interGO = Instantiate(IntersectionPrefab, Vector3.zero, Quaternion.identity, MapContainer.Find("Intersections"));
                    interGO.transform.localPosition = interPos;
                    interGO.name = $"Intersection {x + dir.x * 0.5f};{y + dir.y * 0.5f}";

                    if (i == 0 || i == 2)
                        interGO.transform.rotation = Quaternion.Euler(0, 0, 90);
                }

            }
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
