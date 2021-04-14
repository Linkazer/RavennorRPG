using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeSpellEffect
{
    public int currentCooldown;
    public int currentStack;

    public SpellEffectCommon effet;

    public RuntimeSpellEffect(SpellEffectCommon common, int startCooldown, RuntimeBattleCharacter caster)
    {
        effet = new SpellEffectCommon(common, caster);

        currentCooldown = startCooldown;
    }
}
