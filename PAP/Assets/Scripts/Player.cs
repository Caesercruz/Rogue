using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : Actors
{
    public Slider HealthBar;

    private bool inAttackMode;
    private int storedEnergy;

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
                int damage = tile.UnderAtack;

                // Tirar vida
                ChangeHealth(HealthBar, Health - damage, MaxHealth);
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

                storedEnergy += 3;
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
                Reboot();
                DamageTiles();
                ClearAttackableTiles(false);
                actors.isPlayersTurn = false;
                storedEnergy = 0;
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
                    if (KineticPerk()) enemy.ChangeHealth(enemy.HealthBar, enemy.Health - (Strength + storedEnergy), enemy.MaxHealth);
                    else enemy.ChangeHealth(enemy.HealthBar, enemy.Health - Strength, enemy.MaxHealth);

                    Bewildered(enemy);

                    AcidicBlade(targetPos);
                    
                    if (doubleHit) enemy.ChangeHealth(enemy.HealthBar, enemy.Health - Strength, enemy.MaxHealth);

                    Lifesteal();
                }
                break;
            }
        }

        AnimationManager animationSpawner = FindAnyObjectByType<AnimationManager>();
        if (doubleHit) animationSpawner.SpawnSlashAnimation(AnimationManager.StrikeType.Double, tile.GetCanvasTransform());
        else animationSpawner.SpawnSlashAnimation(AnimationManager.StrikeType.Default, tile.GetCanvasTransform());

        storedEnergy = 0;

        ChangeEnergy(--Energy, MaxEnergy);
        inAttackMode = false;
        ExitAttackMode();
    }
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
            ChangeHealth(HealthBar, Health + MaxHealth / 2, MaxHealth);
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
            if (Random.Range(0f, 1f) <= 0.75f) { Debug.Log("Double"); return true; }
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
    public void ExitAttackMode()
    {
        foreach (var tile in actors.GridTiles.Values)
            tile.SetDarkOverlay(false);
    }
}