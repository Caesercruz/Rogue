using System;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "New Perk", menuName = "Perk System/Perk")]
public class Perk : ScriptableObject
{
    public string Description;
    public Sprite Icon;
    public PerkType Type;
    public int Level = 0; 
}
public enum PerkType
{
    Buff,
    Debuff,
    Neutral,
}