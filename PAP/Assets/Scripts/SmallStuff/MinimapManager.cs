using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    public GameObject openMapPrefab; // Prefab com filhos Rooms/Intersections
    public GameObject RoomPrefab;
    public GameObject IntersectionPrefab;
    public GameScript gameScript; // onde está a grid
    public GameObject Canvas;

    private GameObject openMapInstance;

    public void ShowMap()
    {
        if (openMapInstance != null) return; // já está aberto

        openMapInstance = Instantiate(openMapPrefab, Canvas.transform); // ou outro container UI

        Transform roomsParent = openMapInstance.transform.Find("Rooms");
        Transform intersectionsParent = openMapInstance.transform.Find("Intersections");

        var grid = gameScript.grid;
        int width = gameScript.width;
        int height = gameScript.height;

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
    }

    public void HideMinimap()
    {
        if (openMapInstance != null)
        {
            Destroy(openMapInstance);
            openMapInstance = null;
        }
    }
}
