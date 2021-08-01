using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterStats
{
    Health,
    Maana, 
    PhysicalDamage,
    MagicalDamage,
    Initiative, 
    Defense,
    HitDice, 
    HitBonus,
    HealApplied, 
    HealRecieved,
    CriticalChance, 
    CriticalMultiplier,
    PhysicalArmor,
    MagicalArmor,
    ActionBonus, 
    AttackBonus
}

[CreateAssetMenu(fileName = "New Personnage", menuName = "Character/Personnage joueur")]
public class PersonnageScriptables : ScriptableObject
{
    [Header("Apparence")]
    public string nom = "Gérard";
    public Sprite icon;
    public Sprite portrait;
    public Sprite spritePerso;
    public Sprite spriteBras;
    public float brasPosition;
    [TextArea(3, 5)]
    public string description;

    [Header("Statistique de base")]
    [SerializeField] private int level = 1;
    private int maxHp;
    private int maanaMax;

    [Header("Caractéristiques de Combat")]
    [SerializeField] private int healthPoints;
    [SerializeField] private int maana;
    [SerializeField] private int movementSpeed;
    [SerializeField] private int hitDices;
    [SerializeField] private int defense;
    [SerializeField] private int physicalPower;
    [SerializeField] private int magicalPower;
    [SerializeField] private int luckyDice;
    [SerializeField] private int criticalMultiplicator;
    [SerializeField] private int physicalArmor;
    [SerializeField] private int magicalArmor;
    [SerializeField] private int actionNumber;
    [SerializeField] private int baseAttackNumber;

    protected int healthPointsBonus;
    protected int maanaBonus;
    protected int movementSpeedBonus;
    protected int hitDicesBonus;
    protected int defenseBonus;
    protected int physicalPowerBonus;
    protected int magicalPowerBonus;
    protected int luckyDiceBonus;
    protected int criticalMultiplicatorBonus;
    protected int bonusSoinAppli;
    protected int bonusSoinRecu;
    protected int bonusPhysicalArmor;
    protected int bonusMagicalArmor;
    protected int actionBonus;
    protected int baseAttackBonus;

    protected int hitBonus;


    protected List<Dice> diceBonusDegPhy = new List<Dice>();
    protected List<Dice> diceBonusDegMag = new List<Dice>();

    [Header("Arbre de compétences")]
    public CharacterLevelUpTable levelUpTable;

    [Header("Sorts Disponibles")]
    public List<CharacterActionScriptable> knownSpells;

    public List<SpellEffectScriptables> passifs;

    [SerializeField] private int initiative;

    #region Main Stats
    public int GetLevel => level;

    public int GetMaxHps()
    {
        maxHp = healthPoints + healthPointsBonus;
        return maxHp;
    }

    public int GetMaxMaana()
    {
        maanaMax = maana + maanaBonus;
        if (maanaMax >= 0)
        {
            return maanaMax;
        }
        return 0;
    }
    
    #endregion

    #region Secondary Stats
    public int GetMovementSpeed()
    {
        return movementSpeed + movementSpeedBonus;
    }

    public float GetPhysicalDamage()
    {
        return physicalPower + physicalPowerBonus;
    }

    public float GetMagicalDamage()
    {
        return magicalPower + magicalPowerBonus;
    }

    public virtual int GetInitiativeBrut()
    {
        return initiative;
    }

    public virtual int GetInitiativeDice()
    {
        return initiative;
    }

    public virtual int GetInititativeBonus()
    {
        return initiative;
    }

    public int GetDefense()
    {
        return defense + defenseBonus;
    }

    public int GetHitDice()
    {
        return hitDices + hitDicesBonus;
    }

    public int GetHitBonus()
    {
        return hitBonus;
    }

    public int GetCriticalDamageMultiplier()
    {
        return criticalMultiplicator + criticalMultiplicatorBonus;
    }

    public int GetSoinApplique()
    {
        return bonusSoinAppli;
    }

    public float GetSoinRecu()
    {
        return bonusSoinRecu;
    }

    public int GetCriticalChanceBonus()
    {
        return luckyDice + luckyDiceBonus;
    }

    public int GetPhysicalArmor()
    {
        return physicalArmor + bonusPhysicalArmor;
    }

    public int GetMagicalArmor()
    {
        return magicalArmor + bonusMagicalArmor;
    }

    public List<Dice> GetBonusDice(EffectType wantedType)
    {
        switch(wantedType)
        {
            case EffectType.PhysicalDamage:
                return diceBonusDegPhy;
            case EffectType.MagicalDamage:
                return diceBonusDegMag;
        }
        return new List<Dice>();
    }
    #endregion

    #region Utilitaries
    [ContextMenu("Reset values")]
    private void ResetBonuses()
    {
        healthPointsBonus = 0;
        maanaBonus = 0;
        movementSpeedBonus = 0;
        hitDicesBonus = 0;
        defenseBonus = 0;
        physicalPowerBonus = 0;
        magicalPowerBonus = 0;
        luckyDiceBonus = 0;
        criticalMultiplicatorBonus = 0;
        bonusSoinAppli = 0;
        bonusSoinRecu = 0;
        bonusPhysicalArmor = 0;
        bonusMagicalArmor = 0;

        hitBonus = 0;


        diceBonusDegPhy = new List<Dice>();
        diceBonusDegMag = new List<Dice>();

        actionBonus = 0;
        baseAttackBonus = 0;
}
    #endregion

    #region Ajouts/Retrait des effets
    public void StatBonus(int value, EffectType effType, Dice bonusDice, bool adding)
    {
        switch (effType)
        {
            case EffectType.PhysicalDamage:
                physicalPowerBonus += value;
                if (bonusDice != null)
                {
                    if (adding)
                    {
                        diceBonusDegPhy.Add(bonusDice);
                    }
                    else
                    {
                        diceBonusDegPhy.Remove(bonusDice);
                    }
                }
                break;
            case EffectType.MagicalDamage:
                magicalPowerBonus += value;
                if (bonusDice != null)
                {
                    if (adding)
                    {
                        diceBonusDegMag.Add(bonusDice);
                    }
                    else
                    {
                        diceBonusDegMag.Remove(bonusDice);
                    }
                }
                break;
            case EffectType.Defense:
                defenseBonus += value;
                break;
            case EffectType.HitDice:
                hitDicesBonus += value;
                break;
            case EffectType.HitBonus:
                hitBonus += value;
                break;
            case EffectType.HealApplied:
                bonusSoinAppli += value;
                break;
            case EffectType.HealRecieved:
                bonusSoinRecu += value;
                break;
            case EffectType.MaanaBonus:
                maanaBonus += value;
                break;
            case EffectType.CriticalChance:
                luckyDiceBonus += value;
                break;
            case EffectType.CriticalMultiplier:
                criticalMultiplicatorBonus += value;
                break;
            case EffectType.PhysicalArmor:
                bonusPhysicalArmor += value;
                break;
            case EffectType.MagicalArmor:
                bonusMagicalArmor += value;
                break;
            case EffectType.ActionBonus:
                actionBonus += value;
                break;
        }
    }
    #endregion

    public void GetPossibleActions(out int _possibleAction, out int _possibleAttack)
    {
        _possibleAction = actionNumber + actionBonus + 1;
        _possibleAttack = baseAttackNumber + baseAttackBonus;
    }

    public void LevelUpStat(int value, CharacterStats effType)
    {
        switch (effType)
        {
            case CharacterStats.Health:
                healthPoints += value;
                break;
            case CharacterStats.PhysicalDamage:
                physicalPower += value;
                break;
            case CharacterStats.MagicalDamage:
                magicalPower += value;
                break;
            case CharacterStats.Defense:
                defense += value;
                break;
            case CharacterStats.HitDice:
                hitDices += value;
                break;
            case CharacterStats.HitBonus:
                hitBonus += value;
                break;
            case CharacterStats.HealApplied:
                bonusSoinAppli += value;
                break;
            case CharacterStats.HealRecieved:
                bonusSoinRecu += value;
                break;
            case CharacterStats.Maana:
                maana += value;
                break;
            case CharacterStats.CriticalChance:
                luckyDice += value;
                break;
            case CharacterStats.CriticalMultiplier:
                criticalMultiplicator += value;
                break;
            case CharacterStats.PhysicalArmor:
                physicalArmor += value;
                break;
            case CharacterStats.MagicalArmor:
                magicalArmor += value;
                break;
            case CharacterStats.ActionBonus:
                actionNumber += value;
                break;
            case CharacterStats.AttackBonus:
                baseAttackNumber += value;
                break;
        }
    }

    public void UpLevel()
    {
        level++;
    }
}

