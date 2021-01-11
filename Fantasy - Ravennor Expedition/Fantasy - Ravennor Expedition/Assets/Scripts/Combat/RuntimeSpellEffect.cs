using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeSpellEffect
{
    public int currentCooldown;

    public SpellEffectCommon effet;

    public RuntimeSpellEffect(SpellEffectCommon common, Dice effectDices, DiceType bonusDices, int diceBonusNumber, int startCooldown)
    {
        effet = new SpellEffectCommon(common);

        effet.onTimeEffectValue = GameDices.RollDice(effectDices.numberOfDice, effectDices.wantedDice) + GameDices.RollDice(diceBonusNumber, bonusDices) + effet.onTimeEffectValue;

        currentCooldown = startCooldown;
    }
}
