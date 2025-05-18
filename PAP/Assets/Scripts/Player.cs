using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : Actors
{
    public Slider HealthBar;

    private bool inAttackMode;

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

                        actors.ClearAttackableTiles(true);
                        SetAttackableTiles(true);
                        if (inAttackMode)
                        {
                            // Clique em tile enquanto já está em modo de ataque = Atacar
                            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                            Vector2Int clickedPos = new(Mathf.RoundToInt(mouseWorldPos.x), Mathf.RoundToInt(mouseWorldPos.y));

                            TryAttackAt(clickedPos);

                            Perk doubleHit = gameScript.ActivePerks.Find(p => p.name == "Double Hit");
                            if (doubleHit != null) if (Random.Range(0f, 1f) > 0.75f)
                                {
                                    TryAttackAt(clickedPos);
                                }
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
                DamageTiles();
                actors.ClearAttackableTiles(false);
                actors.isPlayersTurn = false;
            }
        }
    }

    public void EnterAttackMode()
    {
        foreach (var tile in actors.GridTiles.Values)
            if (!tile.InAtackRange) tile.SetDarkOverlay(true);
    }

    void TryAttackAt(Vector2Int targetPos)
    {
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
                    enemy.ChangeHealth(enemy.HealthBar, enemy.Health - Strength, enemy.MaxHealth);
                }
                break;
            }
        }
        //Play animation
        AnimationManager animationSpawner = FindAnyObjectByType<AnimationManager>();
        animationSpawner.SpawnSlashAnimation(tile.GetCanvasTransform());
        
        ChangeEnergy(--Energy, MaxEnergy);
        inAttackMode = false;
        ExitAttackMode();
    }

    public void ExitAttackMode()
    {
        foreach (var tile in actors.GridTiles.Values)
            tile.SetDarkOverlay(false);
    }
}