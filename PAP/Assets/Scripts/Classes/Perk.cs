using UnityEngine;

[CreateAssetMenu(fileName = "New Perk", menuName = "Perk System/Perk")]
public class Perk : ScriptableObject
{
    [TextArea] public string description;
    public Sprite icon;
    public PerkType type;

}

public enum PerkType
{
    Buff,
    Debuff,
    Neutral,
}