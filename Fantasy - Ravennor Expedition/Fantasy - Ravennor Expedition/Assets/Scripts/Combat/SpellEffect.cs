using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    None,
    Power,
    Defense,
    Armor,
    Accuracy,
    HealApplied, 
    HealRecieved, 
    ActionBonus,
    BaseDamage
}

[System.Serializable]
public class SpellEffect
{
    public EffectType type;
    public ActionTargets possiblesTargets = ActionTargets.All;
    public int value;
    public float scaleByMaana;
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
        trigger = toCopy.trigger;
        maanaSpent = toCopy.maanaSpent;
    }
}
