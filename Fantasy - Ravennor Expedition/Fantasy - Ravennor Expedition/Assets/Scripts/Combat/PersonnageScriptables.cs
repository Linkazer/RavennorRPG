using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterStats
{
    Health,
    Maana, 
    Power,
    MagicalDamage,
    Initiative, 
    Defense,
    HitDice, 
    Accuracy,
    HealApplied, 
    HealRecieved,
    CriticalChance, 
    CriticalMultiplier,
    Armor,
    MagicalArmor,
    ActionBonus, 
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
    private int maxHp;
    private int maanaMax;

    [Header("Caractéristiques de Combat")]
    [SerializeField] private int healthPoints;
    [SerializeField] private int maana;
    [SerializeField] private int movementSpeed;
    [SerializeField] private int accuracy;
    [SerializeField] private int defense;
    [SerializeField] private int power;
    [SerializeField] private int armor;
    [SerializeField] private int actionNumber;

    protected int healthPointsBonus;
    protected int maanaBonus;
    protected int movementSpeedBonus;
    protected int accuracyBonus;
    protected int defenseBonus;
    protected int powerBonus;
    protected int baseDamageBonus;
    protected int bonusSoinAppli;
    protected int bonusSoinRecu;
    protected int bonusArmor;
    protected int actionBonus;

    [Header("Arbre de compétences")]
    public CharacterLevelUpTable levelUpTable;

    [Header("Sorts Disponibles")]
    public List<CharacterActionScriptable> knownSpells;

    public List<SpellEffectScriptables> passifs;

    [SerializeField] private int initiative;

    #region Main Stats
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
    
    public List<CharacterActionScriptable> GetOverchargedSpell()
    {
        List<CharacterActionScriptable> toReturn = new List<CharacterActionScriptable>();
        for(int i = 0; i< knownSpells.Count; i++)
        {
            toReturn.Add(knownSpells[i].overchargedAction);
        }
        return toReturn;
    }

    #endregion

    #region Secondary Stats
    public int GetMovementSpeed()
    {
        return movementSpeed + movementSpeedBonus;
    }

    public int GetPower()
    {
        return power + powerBonus;
    }

    public int GetBaseDamage()
    {
        return baseDamageBonus;
    }

    public int GetMagicalDamage()
    {
        return 0;// magicalPower + magicalPowerBonus;
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

    public int GetAccuracy()
    {
        return accuracy + accuracyBonus;
    }

    public int GetSoinApplique()
    {
        return bonusSoinAppli;
    }

    public float GetSoinRecu()
    {
        return bonusSoinRecu;
    }

    public int GetArmor()
    {
        return armor + bonusArmor;
    }

    #endregion

    #region Utilitaries
    [ContextMenu("Reset values")]
    private void ResetBonuses()
    {
        healthPointsBonus = 0;
        maanaBonus = 0;
        movementSpeedBonus = 0;

        defenseBonus = 0;
        powerBonus = 0;
        baseDamageBonus = 0;

        bonusSoinAppli = 0;
        bonusSoinRecu = 0;
        bonusArmor = 0;

        actionBonus = 0;
}
    #endregion

    #region Ajouts/Retrait des effets
    public void StatBonus(int value, EffectType effType)
    {
        switch (effType)
        {
            case EffectType.Accuracy:
                accuracy += value;
                break;
            case EffectType.Power:
                powerBonus += value;
                break;
            case EffectType.BaseDamage:
                baseDamageBonus += value;
                break;
            case EffectType.Defense:
                defenseBonus += value;
                break;
            case EffectType.HealApplied:
                bonusSoinAppli += value;
                break;
            case EffectType.HealRecieved:
                bonusSoinRecu += value;
                break;
            case EffectType.Armor:
                bonusArmor += value;
                break;
            case EffectType.ActionBonus:
                actionBonus += value;
                break;
        }
    }
    #endregion

    public int GetPossibleActions()
    {
        return actionNumber + actionBonus + 1;
    }
}

