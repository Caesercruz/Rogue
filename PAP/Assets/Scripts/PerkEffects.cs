using System.Collections.Generic;
using UnityEngine;

public class PerkEffects : MonoBehaviour
{
    private GameScript gameScript;
    private static readonly float ClumsyPercentage = .15f, DoubleHitPercentage = .75f, BacklashPercentage = .10f, CriticPercentage = .20f;
    private void Start()
    {
        gameScript = GetComponent<GameScript>();
    }
    public void ApplyPerks(Player player)
    {
        if (gameScript.ActivePerks.Find(p => p.name == "Energetic"))
        {
            player.ChangeEnergy(player.MaxEnergy, player.MaxEnergy + 2);
        }
        if (gameScript.ActivePerks.Find(p => p.name == "Reinforced Plates") && !gameScript.RenforcedPlates)
        {
            player.ChangeHealth(player.HealthBar, player.Health + 10, player.MaxHealth + 10);
            gameScript.RenforcedPlates = true;
        }
        if (gameScript.ActivePerks.Find(p => p.name == "Increased Reach"))
        {
            player.AttackPattern.Add(new Vector2Int(-2, -2));
            player.AttackPattern.Add(new Vector2Int(-2, -1));
            player.AttackPattern.Add(new Vector2Int(-2, -0));
            player.AttackPattern.Add(new Vector2Int(-2, 1));
            player.AttackPattern.Add(new Vector2Int(-2, 2));

            player.AttackPattern.Add(new Vector2Int(-1, -2));
            player.AttackPattern.Add(new Vector2Int(-1, 2));

            player.AttackPattern.Add(new Vector2Int(0, -2));
            player.AttackPattern.Add(new Vector2Int(0, 2));

            player.AttackPattern.Add(new Vector2Int(1, -2));
            player.AttackPattern.Add(new Vector2Int(1, 2));

            player.AttackPattern.Add(new Vector2Int(2, -2));
            player.AttackPattern.Add(new Vector2Int(2, -1));
            player.AttackPattern.Add(new Vector2Int(2, -0));
            player.AttackPattern.Add(new Vector2Int(2, 1));
            player.AttackPattern.Add(new Vector2Int(2, 2));
        }

        if (gameScript.ActivePerks.Find(p => p.name == "Weak"))
        {
            player.Strength = player.Strength - 2;
        }
        if (gameScript.ActivePerks.Find(p => p.name == "Rusty Plates") && !gameScript.RustyPlates)
        {
            player.ChangeHealth(player.HealthBar, player.Health, player.MaxHealth - 7);
            gameScript.RustyPlates = true;
        }
        if (gameScript.ActivePerks.Find(p => p.name == "Low Energy"))
        {
            player.ChangeEnergy(player.Energy, player.MaxEnergy - 1);
        }
    }
    //Active Perks
    public void Bewildered(Enemy enemy)
    {
        Perk bewildered = gameScript.ActivePerks.Find(p => p.name == "Bewildered");
        if (bewildered == null) return;
        enemy.Weakness += 2;

        enemy.ClearAttackableTiles(false);
        enemy.RecalculateEnemyAttacks();
    }
    public void Reboot(Player player)
    {
        Perk reboot = gameScript.ActivePerks.Find(p => p.name == "Reboot");
        if (reboot == null) return;
        if (player.Energy == player.MaxEnergy)
        {
            player.ChangeHealth(player.HealthBar, (player.Health + player.MaxHealth / 2), player.MaxHealth);
        }
    }
    public void Lifesteal(Player player)
    {
        Perk lifesteal = gameScript.ActivePerks.Find(p => p.name == "LifeSteal");
        if (lifesteal == null) return;
        player.ChangeHealth(player.HealthBar, player.Health + 2, player.MaxHealth);
    }
    public bool DoubleHit()
    {

        Perk doubleHit = gameScript.ActivePerks.Find(p => p.name == "Double Hit");
        if (doubleHit != null)
        {
            if (Random.Range(0f, 1f) < DoubleHitPercentage) return true;
        }
        return false;
    }
    public bool KineticPerk()
    {
        Perk kineticEnergy = gameScript.ActivePerks.Find(p => p.name == "Kinetic Energy");
        if (kineticEnergy == null) return false;
        return true;
    }
    public void AcidicBlade(Player player, Dictionary<GameObject, Vector2Int> actorsCord, Vector2Int targetPos)
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

            foreach (var kvp2 in actorsCord)
            {
                if (kvp2.Value == adjacentPos)
                {
                    GameObject adjacentEnemyObj = kvp2.Key;
                    Enemy adjacentEnemy = adjacentEnemyObj.GetComponent<Enemy>();
                    if (adjacentEnemy != null)
                    {
                        adjacentEnemy.ChangeHealth(adjacentEnemy.HealthBar, adjacentEnemy.Health - player.Strength / 2, adjacentEnemy.MaxHealth);
                    }
                }
            }
        }
    }
    //Debuffs
    public void Rebound(Player player)
    {
        Perk rebound = gameScript.ActivePerks.Find(p => p.name == "Rebound");
        if (rebound == null) return;
        player.ChangeHealth(player.HealthBar, player.Health - player.Energy * 2, player.MaxHealth);
    }
    public void BatteryLeak(Player player)
    {
        Perk batteryleak = gameScript.ActivePerks.Find(p => p.name == "Battery Leak");
        if (batteryleak == null) return;
        if (player.leakedEnergy) player.ChangeEnergy(--player.Energy, player.MaxEnergy);
        player.leakedEnergy = false;
    }
    public bool Intimidated()
    {
        Perk intimidated = gameScript.ActivePerks.Find(p => p.name == "Intimidated");
        if (intimidated == null) return false;
        return true;
    }
    public bool Clumsy()
    {
        Perk clumsy = gameScript.ActivePerks.Find(p => p.name == "Clumsy");
        if (clumsy == null) return false;

        return Random.Range(0f, 1f) < ClumsyPercentage; // true = falha, 20% das vezes
    }
    public void Backlash(Player player)
    {
        Perk backlash = gameScript.ActivePerks.Find(p => p.name == "Backlash");
        if (backlash == null) return;
        if (Random.Range(0f, 1f) < BacklashPercentage) player.ChangeHealth(player.HealthBar, player.Health - 2, player.MaxHealth);
    }
    public bool ExposedCircuits()
    {
        Perk exposedCircuits = gameScript.ActivePerks.Find(p => p.name == "Exposed Circuits");
        if (exposedCircuits == null) return false;
        Debug.Log("Exposed");
        return Random.Range(0f, 1f) < CriticPercentage;
    }
}