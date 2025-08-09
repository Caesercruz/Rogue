using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : Actors
{
    public Slider HealthBar;
    private PerkEffects perkEffects;
    public bool leakedEnergy = false;
    private bool inAttackMode, weakened = false;
    private int StoredEnergy;

    private void Awake()
    {
        actors = transform.parent.GetComponent<Actors>();
        gameScript = actors.transform.parent.GetComponent<GameScript>();
        perkEffects = gameScript.GetComponent<PerkEffects>();
        gameScript.GameControls.PlayerControls.Enable();
        gameScript.playerInstance = gameObject;
        HealthBar = gameScript.transform.Find("Canvas/Player's HealthBar").GetComponent<Slider>();
        gameScript.playerInstance = gameObject;
    }
    private void Start()
    {
        Health = Convert.ToInt32(HealthBar.value);
        MaxHealth = Convert.ToInt32(HealthBar.maxValue);
        ChangeHealth(HealthBar, Convert.ToInt32(HealthBar.value), Convert.ToInt32(HealthBar.maxValue));
        perkEffects.ApplyPerks(gameScript.playerPrefab.GetComponent<Player>());
        ChangeEnergy(MaxEnergy, MaxEnergy);
    }

    private void Update()
    {
        if (Health == 0)
        {
            gameScript.Gamestate = GameScript.GameState.LostRun;
            gameScript.GameControls.PlayerControls.Disable();
            Destroy(gameObject);
            gameScript.GameOver();
        }
        else if (gameScript.Gamestate == GameScript.GameState.Combat && !actors.isPlayersTurn)
        {
            ChangeEnergy(MaxEnergy, MaxEnergy);
            perkEffects.BatteryLeak(this);

            actors.isPlayersTurn = true;
        }
        VerifyMove();
    }
    private void DamageTiles()
    {
        Vector2Int playerPos = actors.ActorsCord[gameObject]; // Posição do jogador
        if (actors.GridTiles.TryGetValue(playerPos, out Tile tile))
        {
            if (tile.UnderAtack > 0)
            {
                float damage = tile.UnderAtack;

                // Tirar vida
                if (perkEffects.ExposedCircuits()) Mathf.RoundToInt(damage *= 1.2f);
                ChangeHealth(HealthBar, Health - Mathf.RoundToInt(damage), MaxHealth);
                leakedEnergy = true;
                weakened = true;
            }
        }
    }

    private void VerifyMove()
    {
        if (!actors.isPlayersTurn) return;

        if (gameScript.GameControls.PlayerControls.Up.triggered || gameScript.GameControls.PlayerControls.Down.triggered
            || gameScript.GameControls.PlayerControls.Left.triggered || gameScript.GameControls.PlayerControls.Right.triggered)
        {
            if (Energy < 1)
            {
                Debug.Log("Insufficient energy ");
                return;
            }
        }

        bool moved = false;
        if (gameScript.GameControls.PlayerControls.Up.triggered) moved = actors.MoveCharacter(gameObject, Vector2Int.up);
        if (gameScript.GameControls.PlayerControls.Left.triggered) moved = actors.MoveCharacter(gameObject, Vector2Int.left);
        if (gameScript.GameControls.PlayerControls.Down.triggered) moved = actors.MoveCharacter(gameObject, Vector2Int.down);
        if (gameScript.GameControls.PlayerControls.Right.triggered) moved = actors.MoveCharacter(gameObject, Vector2Int.right);

        if (moved)
        {
            ChangeEnergy(--Energy, MaxEnergy);
            ExitAttackMode();
            inAttackMode = false;

            StoredEnergy += 3;
        }

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (gameScript.GameControls.PlayerControls.Atack.triggered)
            {
                if (Energy < 1)
                {
                    Debug.Log("Insufficient energy ");
                    return;
                }

                ClearAttackableTiles(true);
                SetAttackableTiles(true);
                if (inAttackMode)
                {
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                    Vector2 mouseWorld2D = new(mouseWorldPos.x, mouseWorldPos.y);
                    RaycastHit2D hit = Physics2D.Raycast(mouseWorld2D, Vector2.zero);

                    if (hit.collider != null)
                    {
                        Tile tile = hit.collider.GetComponent<Tile>();
                        if (tile != null)
                        {
                            TryAttackAt(tile.Position);
                        }
                    }
                }
                else
                {
                    EnterAttackMode();
                    inAttackMode = true;
                }
            }

            if (gameScript.GameControls.PlayerControls.Back.triggered)
            {
                ExitAttackMode();
                inAttackMode = false;
            }
        }
        if (gameScript.GameControls.PlayerControls.EndTurn.triggered)
        {
            EndTurn();
            
        }
    }
    public void EndTurn()
    {
        ExitAttackMode();
        perkEffects.Rebound(this);
        perkEffects.Reboot(this);
        DamageTiles();
        ClearAttackableTiles(false);
        actors.isPlayersTurn = false;
        StoredEnergy = 0;
        weakened = false;
        gameScript.firstTurn = false;
    }
    public void EnterAttackMode()
    {
        foreach (var tile in actors.GridTiles.Values)
            if (!tile.InAtackRange) tile.SetDarkOverlay(true);
    }
    private void TryAttackAt(Vector2Int targetPos)
    {
        bool doubleHit = perkEffects.DoubleHit();
        bool hit = !perkEffects.Clumsy();

        if (!actors.GridTiles.TryGetValue(targetPos, out Tile tile)) return;
        if (!tile.InAtackRange) return;

        foreach (var kvp in actors.ActorsCord)
        {
            if (kvp.Value == targetPos)
            {
                GameObject target = kvp.Key;
                Enemy enemy = target.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Atack(enemy, targetPos, doubleHit, hit);
                }
                break;
            }
        }
        if (hit)
        {
            AnimationManager animationSpawner = FindAnyObjectByType<AnimationManager>();
            if (doubleHit) animationSpawner.SpawnSlashAnimation(AnimationManager.StrikeType.Double, tile.GetCanvasTransform());
            else animationSpawner.SpawnSlashAnimation(AnimationManager.StrikeType.Default, tile.GetCanvasTransform());
        }
        StoredEnergy = 0;

        ChangeEnergy(--Energy, MaxEnergy);
        inAttackMode = false;
        ExitAttackMode();
    }
    private void Atack(Enemy enemy, Vector2Int targetPos, bool doubleHit, bool hit)
    {
        int damage = Strength;
        if (perkEffects.KineticPerk()) damage += StoredEnergy;
        if (perkEffects.Intimidated() && weakened) damage /= 2;
        if (!hit) return;
        enemy.ChangeHealth(enemy.HealthBar, enemy.Health - damage, enemy.MaxHealth);
        perkEffects.Lifesteal(this);

        perkEffects.Bewildered(enemy);

        perkEffects.AcidicBlade(this, actors.ActorsCord, targetPos);
        perkEffects.Backlash(this);
        if (doubleHit)
        {
            if (perkEffects.Intimidated() && weakened) enemy.ChangeHealth(enemy.HealthBar, enemy.Health - Strength / 2, enemy.MaxHealth);
            enemy.ChangeHealth(enemy.HealthBar, enemy.Health - Strength, enemy.MaxHealth);
            perkEffects.Lifesteal(this);
            perkEffects.Bewildered(enemy);
            perkEffects.AcidicBlade(this, actors.ActorsCord, targetPos);
            perkEffects.Intimidated();
        }
    }
    public void ExitAttackMode()
    {
        foreach (var tile in actors.GridTiles.Values)
            tile.SetDarkOverlay(false);
    }
    public void SetPlayer()
    {
        Vector2Int gridPosition = new(Random.Range(0, _spawnRangeWidth), Random.Range(0, _spawnRangeHeight));

        Vector3 worldPosition = new(gridPosition.x, gridPosition.y, 0);
        transform.localPosition = worldPosition;
        actors.ActorsCord.Add(gameObject, gridPosition);
        
        Tile tile = actors.GridTiles[gridPosition];
        tile.IsOccupied = true;
    }
}