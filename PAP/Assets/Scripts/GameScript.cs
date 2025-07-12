using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

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

    [Header("Debugging")]
    public List<Perk> AllPerks;
    public List<Perk> ActivePerks = new();
    [SerializeField] private List<EncounterData> allEncounters;
    public int NumberOfEnemies = 0;
    public readonly int Width = 8, Height = 6;
    public GameState Gamestate;
    public bool firstTurn = true;
    public int Score = 0;

    [Header("Others")]
    public bool NewGame = false;
    public List<GroundHealth> timedGrounds = new();
    [SerializeField] private EncounterData bossFight;
    private GameObject combatUIInstance;
    private GameObject boardManagerInstance;
    [HideInInspector] public GameObject playerInstance;
    [SerializeField] public GameObject MenuInstance;
    [SerializeField] public GameObject hitboxInstance;
    public MinimapManager MapManager;

    public int SavedHealth;
    public bool RenforcedPlates = false;
    public bool RustyPlates = false;

    [Header("Prefabs")]
    [SerializeField] private GameObject combatUIPrefab;
    [SerializeField] public GameObject boardManager;
    [SerializeField] private GameObject UpdateScreenPrefab;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject tutorial;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private GameObject showPerksPrefab;
    [SerializeField] private GameObject hitboxPrefab;

    private void Start()
    {
        Gameplay();
    }
    private void Update()
    {
        if (GameControls.Actions.Pause.triggered) Pause();
        if (MenuInstance != null && GameControls.Actions.Back.triggered) CloseAction();
    }
    public void ClearWarningData()
    {
        string path = Application.persistentDataPath + "/Info.json";
        if (File.Exists(path))
            File.Delete(path);
    }
    public void Gameplay()
    {
        GameControls = new();
        GameControls.Enable();

        if (!NewGame) MapManager.Load();
        else
            MapManager.GenerateMap();
        
        MapManager.LoadMap(MapManager.gameObject);
        MapManager.SpawnIcons();
        if (NewGame) MapManager.Save();

        if (MapManager.playersRoom.type == RoomData.Type.Nothing) MapManager.Empty();
        else Combat(MapManager.playersRoom.type);
    }
    public void Combat(RoomData.Type roomType)
    {
        GameControls.PlayerControls.Enable();
        combatUIInstance = Instantiate(combatUIPrefab, gameObject.transform.Find("Canvas"));
        combatUIInstance.name = "Combat UI";

        boardManagerInstance = Instantiate(boardManager, gameObject.transform);
        boardManagerInstance.name = "BoardManager";
        actors = boardManagerInstance.GetComponent<Actors>();
        transform.Find("Canvas").GetComponent<HUDManager>().healthBarsContainer = transform.Find("Canvas/Combat UI/EnemyHealthBars");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                SpawnTile(x, y);
            }
        }
        playerInstance.GetComponent<Player>().SetPlayer();
        StartEncounter(roomType);
        Gamestate = GameState.Combat;

        void SpawnTile(int x, int y)
        {
            Vector3Int position = new(x, y, 0);
            Tile spawnedTile = Instantiate(_tilePrefab, actors.transform);
            spawnedTile.name = $"Tile {x} {y}";
            spawnedTile.transform.localPosition = position;
            spawnedTile.GetComponent<Tile>().Position = new(x,y);
            actors.GridTiles[new Vector2Int(x, y)] = spawnedTile;
            bool isOffSet = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
            spawnedTile.Init(isOffSet);
        }
    }
    public void ShowUpdateScreen()
    {
        Gamestate = GameState.WonEncounter;

        GameObject instance = Instantiate(UpdateScreenPrefab, transform);
        instance.name = "UpdateScreen";
        instance.GetComponent<Upgrade>().gameScript = this;
        CleanScene();
        // Inicia a animação
        AnimationManager animationSpawner = FindAnyObjectByType<AnimationManager>();

        //StartCoroutine(animationSpawner.AnimatePopupSpawn(instance.transform));
    }
    public void StartEncounter(RoomData.Type fightType)
    {
        NumberOfEnemies = 0;
        List<EncounterData> validEncounters = new();
        if (fightType == RoomData.Type.Fight) validEncounters = allEncounters.FindAll(e => e.isInfected == false);
        if (fightType == RoomData.Type.Infected) validEncounters = allEncounters.FindAll(e => e.isInfected == true);
        if (fightType == RoomData.Type.Bossfight) validEncounters.Add(bossFight);
        if (validEncounters.Count == 0)
        {
            Debug.LogError("Nenhum encontro válido.");
            return;
        }

        EncounterData selected = validEncounters[UnityEngine.Random.Range(0, validEncounters.Count)];
        Debug.Log("Encontro selecionado: " + selected.name);

        foreach (EnemySpawnInfo enemy in selected.DiferentEnemies)
        {
            for (int spawnedEnemies = 0; spawnedEnemies < enemy.amount; spawnedEnemies++)
            {
                GameObject enemyInstance = Instantiate(enemy.enemyPrefab, actors.transform);
                Enemy enemyScript = enemyInstance.GetComponent<Enemy>();
                enemyScript.SetCharacter(enemyInstance, $"{enemy.Name} {spawnedEnemies}");

                HUDManager hudManager = transform.Find("Canvas").GetComponent<HUDManager>();

                Slider healthbar = hudManager.SpawnHealthBar(enemyInstance, enemyScript);
                hudManager.TransformHealthBars(healthbar, NumberOfEnemies);

                NumberOfEnemies++;
            }
        }
    }
    public void ShowEquipedPerks()
    {
        if (MenuInstance != null) return;
        MenuInstance = Instantiate(showPerksPrefab, transform);
        hitboxInstance = Instantiate(hitboxPrefab, transform.Find("Canvas"));
        hitboxInstance.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => CloseAction());
        GameControls.PlayerControls.Disable();
    }
    public void CleanScene()
    {
        Destroy(combatUIInstance);
        Destroy(boardManagerInstance);
    }
    public void GameOver()
    {
        Destroy(gameObject);
        Instantiate(gameOver);
        GameControls.PlayerControls.Disable();
    }
    public void OpenTutorial()
    {
        if (MenuInstance != null) return;
        hitboxInstance = Instantiate(hitboxPrefab,transform.Find("Canvas"));
        MenuInstance = Instantiate(tutorial, transform);
        hitboxInstance.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => CloseAction());
    }
    public void CloseAction()
    {
        if (Gamestate == GameState.Combat) GameControls.PlayerControls.Enable();
        Destroy(MenuInstance);
        MenuInstance = null;
        Destroy(hitboxInstance);
    }
    public void Pause()
    {
        if (MenuInstance != null)
        {
            Destroy(MenuInstance);
            MenuInstance = null;
            Destroy(hitboxInstance);
            hitboxInstance = null;
            if (Gamestate == GameState.Combat) GameControls.PlayerControls.Enable();
            return;
        }
        MenuInstance = Instantiate(pauseMenu, transform);
        hitboxInstance = Instantiate(hitboxPrefab, transform.Find("Canvas"));
        hitboxInstance.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => CloseAction());
        GameControls.PlayerControls.Disable();
    }
}