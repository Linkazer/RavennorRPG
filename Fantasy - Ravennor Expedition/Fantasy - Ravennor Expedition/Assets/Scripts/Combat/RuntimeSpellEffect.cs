using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeSpellEffect
{
    public int currentCooldown;
    public int currentStack;

    public SpellEffectCommon effet;

    private RuntimeBattleCharacter caster;
    private RuntimeBattleCharacter target;

    public RuntimeSpellEffect(SpellEffectCommon common, int maanaSpent, int startCooldown, RuntimeBattleCharacter nCaster)
    {
        effet = new SpellEffectCommon(common, maanaSpent, nCaster);

        currentCooldown = startCooldown;
    }

    public void ApplyEffect(RuntimeBattleCharacter nCaster, RuntimeBattleCharacter nTarget)
    {
        caster = nCaster;
        target = nTarget;

        target.AddEffect(this);

        caster.beginTurnEvt.AddListener(UpdateCooldown);
    }

    public void RemoveEffect()
    {
        caster.beginTurnEvt.RemoveListener(UpdateCooldown);
    }

    public void UpdateCooldown()
    {
        currentCooldown--;
        if(currentCooldown <= 0)
        {
            RemoveEffect();
            target.ResolveEffect(this, EffectTrigger.End);
            target.RemoveEffect(effet);
        }
    }
}
