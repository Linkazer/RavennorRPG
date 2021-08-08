using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    None,
    PhysicalDamage, MagicalDamage, 
    Initiative, Defense, HitDice, HitBonus,
    HealApplied, HealRecieved, MaanaBonus,
    CriticalChance, CriticalMultiplier,
    PhysicalArmor, MagicalArmor,
    ActionBonus, AttackBonus
}

[System.Serializable]
public class SpellEffect
{
    public EffectType type;
    public ActionTargets possiblesTargets = ActionTargets.All;
    public int value;
    public float scaleByMaana;
    public Dice dicesBonus;
    public EffectTrigger trigger;

    [HideInInspector]
    public int maanaSpent;

    public int RealValue()
    {
        if(maanaSpent != 0)
        {
            return value + Mathf.FloorToInt((maanaSpent) * scaleByMaana);
        }
        else
        {
            return value;
        }
    }

    public SpellEffect()
    {

    }

    public SpellEffect(SpellEffect toCopy)
    {
        possiblesTargets = toCopy.possiblesTargets;
        type = toCopy.type;
        value = toCopy.value;
        scaleByMaana = toCopy.scaleByMaana;
        dicesBonus = new Dice(toCopy.dicesBonus.wantedDice, toCopy.dicesBonus.numberOfDice, toCopy.dicesBonus.wantedDamage, toCopy.dicesBonus.diceByMaanaSpent);
        trigger = toCopy.trigger;
        maanaSpent = toCopy.maanaSpent;
    }
}
