using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Spell", menuName = "Spell/Damage Spell")]
public class CharacterActionDirect : CharacterActionScriptable
{
    [Header("Dégâts")]
    public bool hasPowerEffect = true;

    [SerializeField]
    protected int damageBase;
    [SerializeField]
    protected List<Dice> dices;
    public bool noBonusSpell;
    public DamageType damageType;
    public float diceByLevelBonus;
    public DiceType diceByLevel;
    [SerializeField]
    protected float damageBaseByLevel;
    public bool autoCritical = false;

    public ScalePossibility scaleOrigin;
    public SpellEffectScriptables wantedScaleEffect;
    public Dice scalingDices;
    public float diceByScale;
    public float bonusByScale;

    public float lifeStealPercent = 0;

    public CharacterActionDirect()
    {
        spellType = SpellType.Direct;
    }

    public int GetBaseDamage(int casterLevel)
    {
        return damageBase + Mathf.FloorToInt((casterLevel-actionLevel) * damageBaseByLevel);
    }

    public List<Dice> GetDices()
    {
        return dices;
    }

    public Dice GetLevelBonusDices(int casterLevel)
    {
        if (Mathf.RoundToInt((casterLevel - actionLevel) * diceByLevelBonus) > 0)
        {
            return new Dice(diceByLevel, Mathf.RoundToInt((casterLevel - actionLevel) * diceByLevelBonus), damageType);
        }
        return null;
    }

    public int DamageRoll(int casterLevel)
    {
        int result = 0;

        for (int i = 0; i < dices.Count; i++)
        {
            result += GameDices.RollDice(dices[i].numberOfDice, dices[i].wantedDice);
        }

        if (Mathf.RoundToInt((casterLevel - actionLevel) * diceByLevelBonus) > 0)
        {
            result += GameDices.RollDice(Mathf.RoundToInt((casterLevel - actionLevel) * diceByLevelBonus), diceByLevel);
        }

        return result;
    }

}
