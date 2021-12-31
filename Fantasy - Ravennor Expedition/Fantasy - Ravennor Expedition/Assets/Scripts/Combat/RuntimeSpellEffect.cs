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

    private RuntimeBattleCharacter turnIndexApplied;

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

        if (currentCooldown == 0)
        {
            turnIndexApplied = BattleManager.instance.GetCurrentTurnChara();
            turnIndexApplied.endTurnEvt += UpdateCooldown;
        }
        else
        {
            turnIndexApplied = BattleManager.instance.GetCurrentTurnChara();
            turnIndexApplied.beginTurnEvt += UpdateCooldown;
        }
    }

    public void RemoveEffect()
    {
        turnIndexApplied.endTurnEvt -= UpdateCooldown;
        turnIndexApplied.beginTurnEvt -= UpdateCooldown;
    }

    public void UpdateCooldown()
    {
        if (currentCooldown >= 0)
        {
            currentCooldown--;
            if (currentCooldown == 0)
            {
                RemoveEffect();
                target.ResolveEffect(this, EffectTrigger.End);
                target.RemoveEffect(effet);
            }
        }
    }
}
