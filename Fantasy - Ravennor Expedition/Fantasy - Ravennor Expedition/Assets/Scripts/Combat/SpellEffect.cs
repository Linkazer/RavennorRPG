using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Force, Agilite, PuissMagique, Constitution, Perception,
    PhysicalMeleDamage, PhysicalDistanceDamage, MagicalDamage, 
    Initiative, Defense, Esquive, ChanceToucheForce, ChanceToucheDexterite, ChanceToucheMagic,
    HealApplied, HealRecieved, MaanaBonus, SpellRange,
    CriticalChance, CriticalMultiplier,
    PhysicalArmor, MagicalArmor
}

[System.Serializable]
public class SpellEffect
{
    public EffectType type;
    public int value;
    public int scaleByLevel;
    public Dice dicesBonus;

    public SpellEffect()
    {

    }

    public SpellEffect(SpellEffect toCopy)
    {
        type = toCopy.type;
        value = toCopy.value;
        scaleByLevel = toCopy.scaleByLevel;
        dicesBonus = new Dice(toCopy.dicesBonus.wantedDice, toCopy.dicesBonus.numberOfDice, toCopy.dicesBonus.wantedDamage);
    }
}
