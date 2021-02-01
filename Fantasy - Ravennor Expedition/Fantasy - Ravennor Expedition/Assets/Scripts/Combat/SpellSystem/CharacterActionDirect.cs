using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Spell", menuName = "Spell/Create Damage Spell")]
public class CharacterActionDirect : CharacterActionScriptable
{
    [Header("Dégâts")]
    public bool hasPowerEffect = true;
    //[SerializeField]
    //private List<int> diceNumbers;
    [SerializeField]
    protected List<Dice> dices;
    //private List<DiceType> diceTypes;
    public bool useWeaponDamage;
    public DamageType damageType;
    public float diceByLevelBonus;
    public DiceType diceByLevel;
    public bool autoCritical = false;

    public float lifeStealPercent = 0;

    [Header("Effets")]
    public SpellEffectScriptables wantedEffect;
    public bool applyOnTarget = false, applyOnCaster = false, applyOnGround = false;

    public CharacterActionDirect()
    {
        spellType = SpellType.Direct;
    }

    public List<Dice> GetDices()
    {
        return dices;
    }

    public Dice GetLevelBonusDices(int casterLevel)
    {
        return new Dice(diceByLevel, Mathf.RoundToInt((casterLevel - actionLevel) / diceByLevelBonus), damageType);
    }

    public int DamageRoll(int casterLevel)
    {
        int result = 0;

        for (int i = 0; i < dices.Count; i++)
        {
            result += GameDices.RollDice(dices[i].numberOfDice, dices[i].wantedDice);
        }

        if (Mathf.RoundToInt((casterLevel - actionLevel) / diceByLevelBonus) > 0)
        {
            result += GameDices.RollDice(Mathf.RoundToInt((casterLevel - actionLevel) / diceByLevelBonus), diceByLevel);
        }

        return result;
    }
}
