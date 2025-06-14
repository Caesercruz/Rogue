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

    public List<Perk> ActivePerks = new();
    private GameObject combatUIInstance;
    private GameObject boardManagerInstance;
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
        Combat(false);
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

    public void Combat(bool infected)
    {
        GameControls.PlayerControls.Enable();
        combatUIInstance = Instantiate(combatUIPrefab,gameObject.transform.Find("Canvas"));
        combatUIInstance.name = "Combat UI";

        HUDManager hudManager = transform.Find("Canvas").GetComponent<HUDManager>();
        hudManager.healthBarsContainer = combatUIInstance.transform.Find("HealthBarsContainer");

        boardManagerInstance = Instantiate(boardManager,gameObject.transform);
        boardManagerInstance.name = "BoardManager";
        actors = boardManagerInstance.GetComponent<Actors>();
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
        playerInstance.GetComponent<Player>().SetPlayer();
        GameControls.Enable();
        StartEncounter(infected);
        Gamestate = GameState.Combat;
    }

    [SerializeField] private List<EncounterData> allEncounters;
    public void StartEncounter(bool infected)
    {
        NumberOfEnemies = 0;
        var validEncounters = allEncounters.FindAll(e => e.isInfected == infected);

        if (validEncounters.Count == 0)
        {
            Debug.LogWarning("Nenhum encontro válido.");
            return;
        }

        EncounterData selected = validEncounters[UnityEngine.Random.Range(0, validEncounters.Count)];
        Debug.Log("Encontro selecionado: " + selected.name);

        foreach (EnemySpawnInfo enemy in selected.DiferentEnemies)
        {
            for (int i = 0; i < enemy.amount; i++)
            {
                enemy.enemyPrefab.GetComponent<Enemy>().SpawnCharacter(enemy.enemyPrefab, $"{enemy.Name} {i}");
                TransformHealthBars();
            }
        }
    }
    public void CleanScene()
    {
        Destroy(combatUIInstance);
        Destroy(boardManagerInstance);
    }
}