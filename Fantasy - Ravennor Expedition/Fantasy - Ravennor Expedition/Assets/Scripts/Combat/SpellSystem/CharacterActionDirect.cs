using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Spell", menuName = "Spell/Damage Spell")]
public class CharacterActionDirect : CharacterActionScriptable
{
    [Header("Dégâts")]
    public bool hasPowerEffect = true;

    [SerializeField]
    protected int damageBase;
    [SerializeField]
    protected List<Dice> dices;
    public bool noBonusSpell;
    public DamageType damageType;
    [SerializeField]
    protected float damageBaseByMaana;
    public bool autoCritical = false;

    public ScalePossibility scaleOrigin;
    public SpellEffectScriptables wantedScaleEffect;
    public Dice scalingDices;
    public float diceByScale;
    public float bonusByScale;

    public float lifeStealPercent = 0;

    public CharacterActionDirect()
    {
        spellType = SpellType.Direct;
    }

    public int GetBaseDamage(int maanaSpent)
    {
        return damageBase + Mathf.FloorToInt(maanaSpent * damageBaseByMaana);
    }

    public List<Dice> GetDices()
    {
        return dices;
    }

    public List<Dice> GetDices(int maanaSpent)
    {
        List<Dice> toReturn = new List<Dice>(dices);
        for(int i = 0; i < toReturn.Count; i++)
        {
            toReturn[i] = new Dice(toReturn[i].wantedDice, toReturn[i].numberOfDice + Mathf.RoundToInt(maanaSpent * toReturn[i].diceByMaanaSpent), toReturn[i].wantedDamage, toReturn[i].diceByMaanaSpent);
        }
        return toReturn;
    }

    /*public Dice GetLevelBonusDices(int maanaSpent)
    {
        if (Mathf.RoundToInt(maanaSpent* diceByLevelBonus) > 0)
        {
            return new Dice(diceByLevel, Mathf.RoundToInt(maanaSpent* diceByLevelBonus), damageType);
        }
        return null;
    }*/
}
