using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.WSA;
using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;

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
    public List<Perk> ActivePerks = new List<Perk>();

    [SerializeField] private TextMeshProUGUI _txt_energy;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _cam;
    public Canvas UpdateScreen;
    public GameObject _playerInstance;
    public GameObject _ratInstance;
    [HideInInspector] public int NumberOfEnemies = 0;

    public readonly int Width = 8, Height = 6;
    
    public GameState Gamestate;

    private void Start()
    {
        Gamestate = GameState.Combat;
        actors = GameObject.Find("BoardManager").GetComponent<Actors>();
        
        Gameplay();
    }

    private void Gameplay()
    {
        GenerateGrid();

        actors.SpawnCharacter(_playerInstance,"Player", true);

        Encounter currentEncounter = (Encounter)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Encounter)).Length);
        //Encounter currentEncounter = (Encounter.Test);
        Debug.Log("Encounter: " + currentEncounter);

        switch (currentEncounter)
        {
            case Encounter.Rats_2:
                for (NumberOfEnemies = 0; NumberOfEnemies < 2; NumberOfEnemies++)
                {
                    actors.SpawnCharacter(_ratInstance,$"Rat {NumberOfEnemies}", false);
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
                // Define o padrão de offset
                bool isOffSet = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffSet);
            }
        }
    }
}