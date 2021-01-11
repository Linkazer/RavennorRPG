using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellEffectCommon
{
    public string nom;
    public Sprite spr;
    public string description;

    public List<SpellEffect> effects;

    public DamageType onTimeEffectType;
    public int onTimeEffectValue;
    public Affliction affliction;

    public ActionTargets possiblesTargets;

    public SpellEffectCommon()
    {

    }

    public SpellEffectCommon(SpellEffectCommon toCopy)
    {
        nom = toCopy.nom;
        spr = toCopy.spr;
        description = toCopy.description;

        effects = toCopy.effects;

        onTimeEffectType = toCopy.onTimeEffectType;
        onTimeEffectValue = toCopy.onTimeEffectValue;
        affliction = toCopy.affliction;

        possiblesTargets = toCopy.possiblesTargets;
    }
}
