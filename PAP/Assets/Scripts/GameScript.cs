using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameScript : MonoBehaviour
{
    public Controls GameControls;
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

    [SerializeField] private GameObject combatUIPrefab;
    [SerializeField] private GameObject boardManager;

    [SerializeField] private TextMeshProUGUI _txt_energy;
    [SerializeField] private Tile _tilePrefab;
    public Canvas UpdateScreen;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _ratPrefab;
    [HideInInspector] public GameObject playerInstance;
    [HideInInspector] public GameObject ratInstance;
    [HideInInspector] public int NumberOfEnemies = 0;

    public readonly int Width = 8, Height = 6;

    public GameState Gamestate;

    public MinimapManager MapManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ClearWarningData();
            Debug.Log("Warnings resetados.");
        }
    }
    public void ClearWarningData()
    {
        string path = Application.persistentDataPath + "/Info.json";
        if (File.Exists(path))
            File.Delete(path);
    }

    private void Start()
    {
        GameControls = new();
        GameControls.Enable();
        
        Gameplay();
    }
    
    private void Gameplay()
    {
        MapManager.GenerateMap();
        MapManager.GenerateMiniMapIntersections();
        MapManager.SpawnIcons();
        Combat();
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

    public void Combat()
    {
        //actors = GameObject.Find("BoardManager").GetComponent<Actors>();
        GameObject combatUI = Instantiate(combatUIPrefab,gameObject.transform.Find("Canvas"));
        combatUI.name = "Combat UI";
       GameObject boardManagerGO = Instantiate(boardManager,gameObject.transform);
        boardManagerGO.name = "BoardManager";
        actors = boardManagerGO.GetComponent<Actors>();
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
        playerInstance = actors.SpawnCharacter(_playerPrefab, "Player", true);
        StartEncounter();
    }
    private void StartEncounter()
    {
        Encounter currentEncounter = (Encounter)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Encounter)).Length);
        Debug.Log("Encounter: " + currentEncounter);

        switch (currentEncounter)
        {
            case Encounter.Rats_2:
                for (NumberOfEnemies = 0; NumberOfEnemies < 2; NumberOfEnemies++)
                {
                    actors.SpawnCharacter(_ratPrefab, $"Rat {NumberOfEnemies}", false);
                }
                TransformHealthBars();
                break;
            case Encounter.Rats_3:
                for (NumberOfEnemies = 0; NumberOfEnemies < 3; NumberOfEnemies++)
                {
                    actors.SpawnCharacter(_ratPrefab, $"Rat {NumberOfEnemies}", false);
                }
                TransformHealthBars();
                break;
            default:
                actors.SpawnCharacter(_ratPrefab, $"Rat {NumberOfEnemies}", false);
                break;
        }
        TransformHealthBars();
    }
    public void CleanScene()
    {

    } 
}