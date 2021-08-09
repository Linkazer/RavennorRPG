using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectTrigger { Apply, End, DamageTaken, DamageDealSelf, DamageDealTarget, Heal, DoAction, Die, BeginTurn}

[System.Serializable]
public class SpellEffectCommon
{
    public string nom;
    public Sprite spr;
    [TextArea(3,5)]
    public string description;
    public SpellEffectScriptables wantedEffectToTrigger;

    public int maxStack = 1;

    public List<SpellEffect> effects;

    public List<SpellEffectAction> actionEffect;

    /*public DamageType onTimeEffectType;
    public int onTimeEffectValue;*/
    public Affliction affliction;

    public int maana;

    public SpellEffectCommon()
    {

    }

    public SpellEffectCommon(SpellEffectCommon toCopy, int maanaSpent, RuntimeBattleCharacter caster)
    {
        nom = toCopy.nom;
        spr = toCopy.spr;
        description = toCopy.description;
        maana = maanaSpent;

        List<SpellEffect> newList = new List<SpellEffect>();
        List<SpellEffectAction> newListAct = new List<SpellEffectAction>();

        foreach(SpellEffect eff in toCopy.effects)
        {
            newList.Add(new SpellEffect(eff));
            newList[newList.Count - 1].maanaSpent = maanaSpent;
        }

        foreach(SpellEffectAction eff in toCopy.actionEffect)
        {
            newListAct.Add(new SpellEffectAction(eff));
            newListAct[newListAct.Count - 1].caster = caster;
            newListAct[newListAct.Count - 1].maanaSpent = maanaSpent;
        }

        effects = newList;
        actionEffect = newListAct;

        affliction = toCopy.affliction;

    }
}
