using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellEffectAction
{
    public CharacterActionScriptable spellToUse;
    public EffectTrigger trigger;
    public ActionTargets possiblesTargets;

    [HideInInspector]
    public RuntimeBattleCharacter caster;
    [HideInInspector]
    public int maanaSpent;

    public SpellEffectAction()
    {

    }

    public SpellEffectAction(SpellEffectAction toCopy)
    {
        spellToUse = toCopy.spellToUse;
        caster = toCopy.caster;
        maanaSpent = toCopy.maanaSpent;
        trigger = toCopy.trigger;
        possiblesTargets = toCopy.possiblesTargets;
    }
}
