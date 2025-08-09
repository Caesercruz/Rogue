using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class ShowPerksSlider : MonoBehaviour
{
    List<Perk> buffs = new();
    List<Perk> debuffs = new();

    [Header("Animation Data")]
    [SerializeField] private Vector2Int prevPerkPosition;
    [SerializeField] private Vector2Int selectedPerkPosition;
    [SerializeField] private Vector2Int nextPerkPosition;
    [SerializeField] private int perkSelectedScale,otherScale;

    [Header("Transforms")]
    private GameScript gameScript;
    [SerializeField] private Transform selected;
    private void Awake(){
        gameScript = transform.parent.GetComponent<GameScript>();
        
    }
    private void Start()
    {
        SavePerks();
        ShowPerk(PerkType.Buff);
        ShowPerk(PerkType.Debuff);
    }

    private void SavePerks() { 
        foreach (var perk in gameScript.ActivePerks)
        {
            switch (perk.Type)
            {
                case PerkType.Buff:
                    buffs.Add(perk);
                    break;
                case PerkType.Debuff:
                    debuffs.Add(perk);
                    break;
            }
        }
    }
    private void ShowPerk(PerkType category)
    {
        if(category == PerkType.Buff) {
            selected = transform.Find("Buffs");
            selected.Find("Displayed Perk").GetComponent<Image>().sprite = buffs[0].Icon;
            selected.transform.Find("Perk Name").GetComponent<TextMeshProUGUI>().text = buffs[0].Description;
        }
        else if (category == PerkType.Debuff)
        {
            selected = transform.Find("Debuffs");
            selected.Find("Displayed Perk").GetComponent<Image>().sprite = debuffs[0].Icon;
            selected.transform.Find("Perk Name").GetComponent<TextMeshProUGUI>().text = debuffs[0].Description;
        }
    }
}