using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Encounter/New Encounter")]
public class EncounterData : ScriptableObject
{
    public bool isInfected;
    public List<EnemySpawnInfo> DiferentEnemies;
}

[Serializable]
public class EnemySpawnInfo
{
    public string Name;
    public GameObject enemyPrefab;
    public int amount;
}