using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectTrigger { Apply, End, DamageTaken, DamageDeal, Heal, DoAction, Die, BeginTurn, EnterZone, ExitZone }

[System.Serializable]
public class SpellEffectCommon
{
    public string nom;
    public Sprite spr;
    [TextArea(3,5)]
    public string description;

    public int maxStack = 1;

    public List<SpellEffect> effects;

    public List<SpellEffectAction> actionEffect;

    /*public DamageType onTimeEffectType;
    public int onTimeEffectValue;*/
    public Affliction affliction;

    public SpellEffectCommon()
    {

    }

    public SpellEffectCommon(SpellEffectCommon toCopy, RuntimeBattleCharacter caster)
    {
        nom = toCopy.nom;
        spr = toCopy.spr;
        description = toCopy.description;

        List<SpellEffect> newList = new List<SpellEffect>();
        List<SpellEffectAction> newListAct = new List<SpellEffectAction>();

        foreach(SpellEffect eff in toCopy.effects)
        {
            newList.Add(new SpellEffect(eff));
            newList[newList.Count - 1].caster = caster;
        }

        foreach(SpellEffectAction eff in toCopy.actionEffect)
        {
            newListAct.Add(new SpellEffectAction(eff));
            newListAct[newListAct.Count - 1].caster = caster;
        }

        effects = newList;
        actionEffect = newListAct;

        affliction = toCopy.affliction;

    }
}
