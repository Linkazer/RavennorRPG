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
        return damageBase;
    }

    public int GetDices(int maanaSpent)
    {
        return diceNumber + maanaSpent;
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
