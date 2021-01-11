using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Personnage", menuName = "Create New Personnage")]
public class PersonnageScriptables : ScriptableObject
{
    [Header("Apparence")]
    public string nom = "Gérard";
    public Sprite icon;
    public Sprite spritePerso;
    public Sprite spriteDeMains;
    public GameObject specialPrefab;

    [Header("Niveau")]
    public int level = 1;
    [SerializeField]
    private int baseHps = 10, hpByLevel = 0, baseMoveSpeed = 30;
    private int maxHp;
    private int maanaMax;

    [Header("Caractéristiques de Combat")]
    [SerializeField]
    private int force;
    [SerializeField]
    private int constitution, agilite, intelligence, perception, charisme, puissMag;

    [Header("Caractéristiques de Combat - IA")]
    //[SerializeField]
    private int bonusForce;
    //[SerializeField]
    private int bonusAgilite, bonusPuissMag, bonusIntelligence, bonusConstit, bonusPerception, bonusCharisme;
    [Header("Bonus Stats secondaires")]
    //[SerializeField]
    private int bonusDegPhyMelee;
    //[SerializeField]
    private int bonusDegPhyDistance, bonusDegMag, bonusInitiative, bonusDefense, bonusEsquive, bonusChanceToucheForce, bonusChanceToucheDexterite, bonusChanceToucheMagic, bonusSoinAppli, bonusSoinRecu,
                bonusMaanaCost, bonusSpellRange, bonusCriticalChance, bonusPhysicalArmor, bonusMagicalArmor;
    private List<Dice> diceBonusDegPhy = new List<Dice>(), diceBonusDegMag = new List<Dice>(), diceBonusDegWeapon = new List<Dice>(), diceBonusDefense = new List<Dice>(), diceBonusToucheForce = new List<Dice>(), diceBonusToucheDexterite = new List<Dice>(), diceBonusToucheMagic = new List<Dice>();
    [Header("Bonus armures")]
    //[SerializeField]
    private int bonusPhysicalDefense, bonusMagicalDefense;

    //[Header("Autres")]

    //[Header("Arbre de compétences")]
    //public GameObject arbreCompetence;

    [Header("Sorts Disponibles")]
    public List<CharacterActionScriptable> sortsDisponibles;

    //Liste des Passifs

    //[Header("Equipements")]
    //Equipement

    #region Main Stats

    public void StatUp(int statVoulue)
    {
        switch (statVoulue)
        {
            case 0:
                force++;
                break;
            case 1:
                agilite++;
                break;
            case 2:
                puissMag++;
                break;
            case 3:
                intelligence++;
                break;
            case 4:
                constitution++;
                break;
            case 5:
                perception++;
                break;
            case 6:
                charisme++;
                break;
        }
    }

    public int GetMaxHps()
    {
        maxHp = baseHps + hpByLevel * level  + GetConstitution() * 6;
        return maxHp;
    }

    public int GetMaxMaana()
    {
        maanaMax = level + puissMag;
        return maanaMax;
    }

    public int GetForce()
    {
        if (force + bonusForce >= 0)
        {
            return force + bonusForce;
        }
        else
        {
            return 0;
        }
    }

    public int GetAgilite()
    {
        if (agilite + bonusAgilite >= 0)
        {
            return agilite + bonusAgilite;
        }
        else
        {
            return 0;
        }
    }

    public int GetPuissMag()
    {
        if (puissMag + bonusDegMag >= 0)
        {
            return puissMag + bonusPuissMag;
        }
        else
        {
            return 0;
        }
    }

    public int GetConstitution()
    {
        if (constitution + bonusConstit >= 0)
        {
            return constitution + bonusConstit;
        }
        else
        {
            return 0;
        }
    }

    public int GetPerception()
    {
        if (perception + bonusPerception >= 0)
        {
            return perception + bonusPerception;
        }
        else
        {
            return 0;
        }
    }

    public int GetIntelligence()
    {
        if (intelligence + bonusIntelligence >= 0)
        {
            return intelligence + bonusIntelligence;
        }
        else
        {
            return 0;
        }
    }

    public int GetCharisme()
    {
        if (charisme + bonusCharisme >= 0)
        {
            return charisme + bonusCharisme;
        }
        else
        {
            return 0;
        }
    }
    #endregion

    #region Secondary Stats
    public int GetMovementSpeed()
    {
        return GetAgilite() * 6 + baseMoveSpeed;
    }

    public float GetPhysicalDamageMelee()
    {
        return bonusDegPhyMelee + GetForce();
    }

    public float GetPhysicalDamageDistance()
    {
        return bonusDegPhyDistance;
    }

    public float GetMagicalDamage()
    {
        return GetPuissMag() + bonusDegMag;
    }

    public int GetInitiative()
    {
        return GetPerception() + bonusPerception;
    }

    public int GetDefense()
    {
        return 0;// GetAgilite() + bonusDefense;
    }

    public int GetToucheMelee()
    {
        return GetForce() + bonusChanceToucheForce;
    }

    public int GetToucheDistance()
    {
        return GetAgilite() + bonusChanceToucheDexterite;
    }

    public int GetToucheMagical()
    {
        return GetPuissMag() + bonusChanceToucheMagic;
    }

    public float GetCriticalDamageMultiplier()
    {
        return 1 + (((GetPerception()+1)/ 2) * 0.25f);
    }

    public float GetSoinApplique()
    {
        return bonusSoinAppli;
    }

    public float GetSoinRecu()
    {
        return bonusSoinRecu;
    }

    public int GetMaanaCostBonus()
    {
        return bonusMaanaCost;
    }

    public int GetSpellRangeBonus()
    {
        return bonusSpellRange;
    }

    public int GetCriticalChanceBonus()
    {
        return 1 + Mathf.RoundToInt(((GetPerception() -1) / 3)) + 1 + bonusCriticalChance;
    }

    public int GetPhysicalArmor()
    {
        return 0 + bonusPhysicalArmor;
    }

    public int GetMagicalArmor()
    {
        return 0 + bonusMagicalArmor;
    }

    public List<Dice> GetBonusDice(EffectType wantedType)
    {
        switch(wantedType)
        {
            case EffectType.PhysicalMeleDamage:
                return diceBonusDegPhy;
            case EffectType.MagicalDamage:
                return diceBonusDegMag;
            case EffectType.Defense:
                return diceBonusDefense;
            case EffectType.ChanceToucheForce:
                return diceBonusToucheForce;
            case EffectType.ChanceToucheDexterite:
                return diceBonusToucheDexterite;
            case EffectType.ChanceToucheMagic:
                return diceBonusToucheMagic;
        }
        return new List<Dice>();
    }
    #endregion
   
    #region Ajouts/Retrait des effets
    public void StatBonus(int value, EffectType effType, Dice bonusDice, bool adding)
    {
        switch (effType)
        {
            case EffectType.Force:
                bonusForce += value;
                break;
            case EffectType.Agilite:
                bonusAgilite += value;
                break;
            case EffectType.PuissMagique:
                bonusPuissMag += value;
                break;
            case EffectType.Constitution:
                bonusConstit += value;
                break;
            case EffectType.Perception:
                bonusPerception += value;
                break;
            case EffectType.PhysicalMeleDamage:
                bonusDegPhyMelee += value;
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
            case EffectType.PhysicalDistanceDamage:
                bonusDegPhyDistance += value;
                break;
            case EffectType.MagicalDamage:
                bonusDegMag += value;
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
            case EffectType.Initiative:
                bonusInitiative += value;
                break;
            case EffectType.Defense:
                bonusDefense += value;
                if (bonusDice != null)
                {
                    if (adding)
                    {
                        diceBonusDefense.Add(bonusDice);
                    }
                    else
                    {
                        diceBonusDefense.Remove(bonusDice);
                    }
                }
                break;
            case EffectType.Esquive:
                bonusEsquive += value;
                break;
            case EffectType.ChanceToucheForce:
                bonusChanceToucheForce += value;
                if (bonusDice != null)
                {
                    if (adding)
                    {
                        diceBonusToucheForce.Add(bonusDice);
                    }
                    else
                    {
                        diceBonusToucheForce.Remove(bonusDice);
                    }
                }
                break;
            case EffectType.ChanceToucheDexterite:
                bonusChanceToucheDexterite += value;
                if (bonusDice != null)
                {
                    if (adding)
                    {
                        diceBonusToucheDexterite.Add(bonusDice);
                    }
                    else
                    {
                        diceBonusToucheDexterite.Remove(bonusDice);
                    }
                }
                break;
            case EffectType.ChanceToucheMagic:
                bonusChanceToucheMagic += value;
                if (bonusDice != null)
                {
                    if (adding)
                    {
                        diceBonusToucheMagic.Add(bonusDice);
                    }
                    else
                    {
                        diceBonusToucheMagic.Remove(bonusDice);
                    }
                }
                break;
            case EffectType.HealApplied:
                bonusSoinAppli += value;
                break;
            case EffectType.HealRecieved:
                bonusSoinRecu += value;
                break;
            case EffectType.SpellRange:
                bonusSpellRange += value;
                break;
            case EffectType.MaanaCost:
                bonusMaanaCost += value;
                break;
            case EffectType.CriticalChance:
                bonusCriticalChance += value;
                break;
            //Manque Critical Damage/Physical et magical armor
        }
    }
    #endregion

    public void ResetStats()
    {
        bonusForce = 0;
        bonusAgilite = 0; 
        bonusPuissMag = 0; 
        bonusIntelligence = 0;
        bonusConstit = 0;
        bonusPerception = 0;
        bonusCharisme = 0;
        bonusDegPhyMelee = 0;
        bonusDegPhyDistance = 0;
        bonusDegMag = 0;
        bonusInitiative = 0;
        bonusDefense = 0;
        bonusEsquive = 0;
        bonusChanceToucheForce = 0;
        bonusChanceToucheDexterite = 0;
        bonusChanceToucheMagic = 0;
        bonusSoinAppli = 0;
        bonusSoinRecu = 0;
        bonusMaanaCost = 0;
        bonusSpellRange = 0;
        bonusCriticalChance = 0;
        bonusPhysicalArmor = 0;
        bonusMagicalArmor = 0;

        diceBonusDegPhy = new List<Dice>();
        diceBonusDegMag = new List<Dice>();
        diceBonusDegWeapon = new List<Dice>();
        diceBonusDefense = new List<Dice>();
        diceBonusToucheForce = new List<Dice>();
        diceBonusToucheDexterite = new List<Dice>();
        diceBonusToucheMagic = new List<Dice>();
    }
}

