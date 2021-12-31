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
    protected int diceNumber;
    public DamageType damageType = DamageType.Damage;
    public bool ignorePower;
    public bool autoCritical = false;

    /*public ScalePossibility scaleOrigin;
    public SpellEffectScriptables wantedScaleEffect;
    public Dice scalingDices;
    public float diceByScale;
    public float bonusByScale;*/

    public float lifeStealPercent = 0;

    [Header("On Hit Effects")]
    public List<SpellEffectScriptables> wantedHitEffectOnTarget, wantedHitEffectOnCaster;

    public override SpellType SpellType => SpellType.Direct;

    public int GetBaseDamage()
    {
        return damageBase;
    }

    public int GetDices(int power = 0)
    {
        if (!ignorePower && diceNumber > 0)
        {
            return diceNumber + power;
        }
        return diceNumber;
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
