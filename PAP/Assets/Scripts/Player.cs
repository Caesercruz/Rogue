using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : Actors
{
    private static readonly float ClumsyPersentage = .15f, DoubleHitPercentage = .75f, BacklashPercentage = .10f, CriticPercentage = .20f;

    public Slider HealthBar;
    
    private bool inAttackMode, leakedEnergy = false, weakened = false;
    private int StoredEnergy;

    private void Start()
    {
        gameScript = GameObject.Find("GameManager").GetComponent<GameScript>();
        actors = GameObject.Find("BoardManager").GetComponent<Actors>();
        gameScript._playerInstance = gameObject;

        HealthBar = Instantiate(HealthBar, GameObject.Find("Canvas").transform);

        HealthBar.name = "Player's HealthBar";

        ChangeEnergy(MaxEnergy, MaxEnergy);
        ChangeHealth(HealthBar, MaxHealth, MaxHealth);
    }

    private void Update()
    {
        if (Health == 0)
        {
            gameScript.Gamestate = GameScript.GameState.LostRun;
            Destroy(gameObject);
        }
        if (gameScript.Gamestate == GameScript.GameState.Combat && actors.isPlayersTurn)
        {
            VerifyMove();
        }
        else if (gameScript.Gamestate == GameScript.GameState.Combat && !actors.isPlayersTurn)
        {
            ChangeEnergy(MaxEnergy, MaxEnergy);
            BatteryLeak();

            actors.isPlayersTurn = true;
        }
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
                if(ExposedCircuits()) Mathf.RoundToInt(damage *= 1.2f);
                ChangeHealth(HealthBar, Health - Mathf.RoundToInt(damage), MaxHealth);
                leakedEnergy = true;
                weakened = true;
            }
        }
    }

    private void VerifyMove()
    {
        if (actors.isPlayersTurn)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A)
                || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                if (Energy < 1)
                {
                    Debug.Log("Insufficient energy ");
                    return;
                }
            }

            bool moved = false;
            if (Input.GetKeyDown(KeyCode.W)) moved = actors.MoveCharacter(gameObject, Vector2Int.up);
            if (Input.GetKeyDown(KeyCode.A)) moved = actors.MoveCharacter(gameObject, Vector2Int.left);
            if (Input.GetKeyDown(KeyCode.S)) moved = actors.MoveCharacter(gameObject, Vector2Int.down);
            if (Input.GetKeyDown(KeyCode.D)) moved = actors.MoveCharacter(gameObject, Vector2Int.right);

            if (moved)
            {
                ChangeEnergy(--Energy, MaxEnergy);
                ExitAttackMode();
                inAttackMode = false;

                StoredEnergy += 3;
            }

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
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
                            // Clique em tile enquanto já está em modo de ataque = Atacar
                            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                            Vector2Int clickedPos = new(Mathf.RoundToInt(mouseWorldPos.x), Mathf.RoundToInt(mouseWorldPos.y));

                            TryAttackAt(clickedPos);
                        }
                        else
                        {
                            // Entrar em modo de ataque
                            EnterAttackMode();
                            inAttackMode = true;
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        ExitAttackMode();
                        inAttackMode = false;
                    }
                }

            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ExitAttackMode();
                Rebound();
                Reboot();
                DamageTiles();
                ClearAttackableTiles(false);
                actors.isPlayersTurn = false;
                StoredEnergy = 0;
                weakened = false;
            }
        }
    }

    public void EnterAttackMode()
    {
        foreach (var tile in actors.GridTiles.Values)
            if (!tile.InAtackRange) tile.SetDarkOverlay(true);
    }

    private void TryAttackAt(Vector2Int targetPos)
    {
        bool doubleHit = DoubleHit();
        bool hit = !Clumsy();

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
                    Atack(enemy, targetPos, doubleHit,hit);
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
    private void Atack(Enemy enemy, Vector2Int targetPos, bool doubleHit,bool hit)
    {
        int damage = Strength;
        if (KineticPerk()) damage += StoredEnergy;
        if (Intimidated() && weakened) damage /= 2;
        if (!hit) return;
        enemy.ChangeHealth(enemy.HealthBar, enemy.Health - damage, enemy.MaxHealth);
        Lifesteal();

        Bewildered(enemy);

        AcidicBlade(targetPos);
        Backlash();
        if (doubleHit)
        {
            if (Intimidated() && weakened) enemy.ChangeHealth(enemy.HealthBar, enemy.Health - Strength / 2, enemy.MaxHealth);
            enemy.ChangeHealth(enemy.HealthBar, enemy.Health - Strength, enemy.MaxHealth);
            Lifesteal();
            Bewildered(enemy);
            AcidicBlade(targetPos);
            Intimidated();
        }
    }
    public void ExitAttackMode()
    {
        foreach (var tile in actors.GridTiles.Values)
            tile.SetDarkOverlay(false);
    }
    //Active Perks
    private void Bewildered(Enemy enemy)
    {
        Perk bewildered = gameScript.ActivePerks.Find(p => p.name == "Bewildered");
        if (bewildered == null) return;
        enemy.Weakness += 2;

        ClearAttackableTiles(false);
        enemy.RecalculateEnemyAttacks();
    }
    private void Reboot()
    {
        Perk reboot = gameScript.ActivePerks.Find(p => p.name == "Reboot");
        if (reboot == null) return;
        if (Energy == MaxEnergy)
        {
            ChangeHealth(HealthBar, (Health + MaxHealth / 2), MaxHealth);
        }
    }
    private void Lifesteal()
    {
        Perk lifesteal = gameScript.ActivePerks.Find(p => p.name == "LifeSteal");
        if (lifesteal == null) return;
        ChangeHealth(HealthBar, Health + 2, MaxHealth);
    }
    private bool DoubleHit()
    {

        Perk doubleHit = gameScript.ActivePerks.Find(p => p.name == "Double Hit");
        if (doubleHit != null)
        {
            if (Random.Range(0f, 1f) < DoubleHitPercentage)return true;
        }
        return false;
    }
    private bool KineticPerk()
    {
        Perk kineticEnergy = gameScript.ActivePerks.Find(p => p.name == "Kinetic Energy");
        if (kineticEnergy == null) return false;
        return true;
    }
    private void AcidicBlade(Vector2Int targetPos)
    {
        Perk acidicBlade = gameScript.ActivePerks.Find(p => p.name == "Acidic Blade");
        if (acidicBlade == null) return;

        // Vetores de posições adjacentes (8 direções)
        Vector2Int[] adjacentOffsets = new Vector2Int[]
        {
        new (-1, -1), new (-1, 0), new (-1, 1),
        new ( 0, -1),              new ( 0, 1),
        new ( 1, -1), new ( 1, 0), new ( 1, 1)
        };

        foreach (var offset in adjacentOffsets)
        {
            Vector2Int adjacentPos = targetPos + offset;

            foreach (var kvp2 in actors.ActorsCord)
            {
                if (kvp2.Value == adjacentPos)
                {
                    GameObject adjacentEnemyObj = kvp2.Key;
                    Enemy adjacentEnemy = adjacentEnemyObj.GetComponent<Enemy>();
                    if (adjacentEnemy != null)
                    {
                        adjacentEnemy.ChangeHealth(adjacentEnemy.HealthBar, adjacentEnemy.Health - Strength / 2, adjacentEnemy.MaxHealth);
                    }
                }
            }
        }
    }
    //Active Debuffs
    private void Rebound()
    {
        Perk rebound = gameScript.ActivePerks.Find(p => p.name == "Rebound");
        if (rebound == null) return;
        ChangeHealth(HealthBar, Health - Energy * 2, MaxHealth);
    }
    private void BatteryLeak()
    {
        Perk batteryleak = gameScript.ActivePerks.Find(p => p.name == "Battery Leak");
        if (batteryleak == null) return;
        if (leakedEnergy) ChangeEnergy(--Energy, MaxEnergy);
        leakedEnergy = false;
    }
    private bool Intimidated()
    {
        Perk intimidated = gameScript.ActivePerks.Find(p => p.name == "Intimidated");
        if (intimidated == null) return false;
        return true;
    }
    private bool Clumsy()
    {
        Perk clumsy = gameScript.ActivePerks.Find(p => p.name == "Clumsy");
        if (clumsy == null) return false;

        return Random.Range(0f, 1f) < ClumsyPersentage; // true = falha, 20% das vezes
    }
    private void Backlash()
    {
        Perk backlash = gameScript.ActivePerks.Find(p => p.name == "Backlash");
        if (backlash == null) return;
        if (Random.Range(0f, 1f) < BacklashPercentage) ChangeHealth(HealthBar, Health - 2, MaxHealth);
    }
    private bool ExposedCircuits()
    {
        Perk exposedCircuits = gameScript.ActivePerks.Find(p => p.name == "Exposed Circuits");
        if (exposedCircuits == null) return false;
        Debug.Log("Exposed");
        return Random.Range(0f, 1f) < CriticPercentage;
    }
}