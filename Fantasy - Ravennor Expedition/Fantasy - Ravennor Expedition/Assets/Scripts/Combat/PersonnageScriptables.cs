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
    public Sprite portrait;
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
    //[Header("Bonus Stats secondaires")]
    //[SerializeField]
    protected int bonusDegPhyMelee;
    //[SerializeField]
    protected int bonusDegPhyDistance, bonusDegMag, bonusInitiative, bonusDefense, bonusChanceToucheForce, bonusChanceToucheDexterite, bonusChanceToucheMagic, bonusSoinAppli, bonusSoinRecu,
                bonusMaana, bonusSpellRange, bonusCriticalChance, bonusPhysicalArmor, bonusMagicalArmor, touchMeleeDice, toucheDistanceDice, toucheMagicalDice;
    protected List<Dice> diceBonusDegPhy = new List<Dice>(), diceBonusDegMag = new List<Dice>();//, diceBonusDegWeapon = new List<Dice>(), diceBonusDefense = new List<Dice>(), diceBonusToucheForce = new List<Dice>(), diceBonusToucheDexterite = new List<Dice>(), diceBonusToucheMagic = new List<Dice>();
    //[Header("Bonus armures")]
    //[SerializeField]
    private int bonusPhysicalDefense, bonusMagicalDefense;

    //[Header("Autres")]

    [Header("Arbre de compétences")]
    public CharacterLevelUpTable levelUpTable;

    [Header("Sorts Disponibles")]
    public List<CharacterActionScriptable> learnedSpells;
    public List<CharacterActionScriptable> sortsDisponibles;

    [HideInInspector]
    public List<SpellEffectScriptables> passifs;
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
                constitution++;
                break;
            case 2:
                agilite++;
                break;
            case 3:
                intelligence++;
                break;
            case 4:
                perception++;
                break;
            case 5:
                puissMag++;
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
        maanaMax = level + puissMag + bonusMaana;
        if (maanaMax >= 0)
        {
            return maanaMax;
        }
        return 0;
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

    public List<int> GetAllMainStats()
    {
        int[] newArr = new int[] { GetForce(), GetConstitution(), GetAgilite(), GetIntelligence(), GetPerception(), GetPuissMag() };
        return new List<int>(newArr);
    }

    public List<int> GetAllSecondaryStats()
    {
        int[] newArr = new int[] { (int)GetPhysicalDamageMelee(), (int)GetMagicalDamage(), GetInitiative(), GetBrutDefense(), GetCharisme(), GetPerception(), GetPuissMag() };
        return new List<int>(newArr);
    }

    #region Secondary Stats
    public int GetMovementSpeed()
    {
        return GetAgilite() * 6 + baseMoveSpeed;
    }

    public int GetMemory()
    {
        int totalMemory = 2 + GetIntelligence();
        if(totalMemory > 10)
        {
            return 10;
        }
        else
        {
            return totalMemory;
        }
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

    public int GetBrutDefense()
    {
        return GetAgilite()*2+GetForce()+level+bonusDefense;
    }

    /*public List<Dice> GetDefenseDices()
    {
        List<Dice> toReturn = new List<Dice>();
        toReturn.Add(new Dice(DiceType.D6, GetAgilite(), DamageType.Brut));
        Debug.Log(nom + " " + GetAgilite());
        foreach (Dice d in GetBonusDice(EffectType.Defense))
        {
            toReturn.Add(d);

        }

        return toReturn;
    }*/

    /*public int GetDefenseDice()
    {
        return GetAgilite() + defenseDice;
    }*/

    public int GetBrutToucheMelee()
    {
        return bonusChanceToucheForce;// + level;
    }

    public int GetBrutToucheDistance()
    {
        return bonusChanceToucheDexterite;// + level;
    }

    public int GetBrutToucheMagical()
    {
        return bonusChanceToucheMagic;// + level;
    }

    /*public List<Dice> GetToucheDices(int index)
    {
        List<Dice> toReturn = new List<Dice>();
        switch (index)
        {
            case 1:
                toReturn.Add(new Dice(DiceType.D6, GetForce(), DamageType.Brut));
                foreach (Dice d in GetBonusDice(EffectType.ChanceToucheForce))
                {
                    toReturn.Add(d);
                }
                break;
            case 2:
                toReturn.Add(new Dice(DiceType.D6, GetAgilite(), DamageType.Brut));
                foreach (Dice d in GetBonusDice(EffectType.ChanceToucheDexterite))
                {
                    toReturn.Add(d);
                }
                break;
            case 3:
                toReturn.Add(new Dice(DiceType.D6, GetPuissMag(), DamageType.Brut));
                foreach (Dice d in GetBonusDice(EffectType.ChanceToucheMagic))
                {
                    toReturn.Add(d);
                }
                break;
        }

        return toReturn;
    }*/

    public int GetTouchDices(int index)
    {
        switch (index)
        {
            case 1:
                return GetForce() + touchMeleeDice;
            case 2:
                return GetAgilite() + toucheDistanceDice;
            case 3:
                return GetPuissMag() + toucheMagicalDice;
        }
        return 0;
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

    public int GetSpellRangeBonus()
    {
        return bonusSpellRange;
    }

    public int GetCriticalChanceBonus()
    {
        return Mathf.RoundToInt(((GetPerception() -1) / 3)) + 1 + bonusCriticalChance;
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
            /*case EffectType.Defense:
                return diceBonusDefense;
            case EffectType.ChanceToucheForce:
                return diceBonusToucheForce;
            case EffectType.ChanceToucheDexterite:
                return diceBonusToucheDexterite;
            case EffectType.ChanceToucheMagic:
                return diceBonusToucheMagic;*/
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
                break;
            case EffectType.ChanceToucheForce:
                if (bonusDice.numberOfDice > 0)
                {
                    touchMeleeDice += value;
                }
                else
                {
                    bonusChanceToucheForce += value;
                }
                break;
            case EffectType.ChanceToucheDexterite:
                if (bonusDice.numberOfDice > 0)
                {
                    toucheDistanceDice += value;
                }
                else
                {
                    bonusChanceToucheDexterite += value;
                }
                break;
            case EffectType.ChanceToucheMagic:
                if (bonusDice.numberOfDice > 0)
                {
                    toucheMagicalDice += value;
                }
                else
                {
                    bonusChanceToucheMagic += value;
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
            case EffectType.MaanaBonus:
                bonusMaana += value;
                break;
            case EffectType.CriticalChance:
                bonusCriticalChance += value;
                break;
            //Manque Critical Damage/Physical et magical armor
        }
    }
    #endregion


    public virtual void ResetStats()
    {
        /*bonusForce = 0;
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
        bonusChanceToucheForce = 0;
        bonusChanceToucheDexterite = 0;
        bonusChanceToucheMagic = 0;
        bonusSoinAppli = 0;
        bonusSoinRecu = 0;
        bonusMaana = 0;
        bonusSpellRange = 0;
        bonusCriticalChance = 0;
        bonusPhysicalArmor = 0;
        bonusMagicalArmor = 0;

        touchMeleeDice = 0;
        toucheDistanceDice = 0;
        toucheMagicalDice = 0;

        diceBonusDegPhy = new List<Dice>();
        diceBonusDegMag = new List<Dice>();*/
    }

    public void LevelUpStat(int index)
    {
        switch (index)
        {
            case 0:
                force++;
                break;
            case 1:
                constitution++;
                break;
            case 2:
                agilite++;
                break;
            case 3:
                intelligence++;
                break;
            case 4:
                perception++;
                break;
            case 5:
                puissMag++;
                break;
        }
    }

    public void RemoveUpStat(int index)
    {
        switch (index)
        {
            case 0:
                force--;
                break;
            case 1:
                constitution--;
                break;
            case 2:
                agilite--;
                break;
            case 3:
                intelligence--;
                break;
            case 4:
                perception--;
                break;
            case 5:
                puissMag--;
                break;
        }
    }

    public void SetLevel(int levelWanted)
    {
        level = levelWanted;
    }
}

