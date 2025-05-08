using UnityEngine;

[CreateAssetMenu(fileName = "New Perk", menuName = "Perk System/Perk")]
public class Perk : ScriptableObject
{
    public string perkName;
    [TextArea] public string description;
    public Sprite icon;
    public PerkType type;
    public bool active;

}

public enum PerkType
{
Buff,
Debuff,
Neutral,
}
