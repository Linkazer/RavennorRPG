using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AI", menuName = "Character/IA Character")]
public class AiCharacterScriptable : PersonnageScriptables
{
    [Header("IA")]
    public List<AiConsideration> comportement;

    [Header("Bonus Stats secondaires")]
    [SerializeField]
    private int aiBonusDegPhyMelee;
    [SerializeField]
    private int aiBonusDegPhyDistance, aiBonusDegMag, aiBonusInitiative, aiBonusInitiativeDice, aiBonusDefense, aiBonusChanceToucheForce, aiBonusChanceToucheDexterite, aiBonusChanceToucheMagic, aiBonusSoinAppli, aiBonusSoinRecu,
                aiBonusMaana, aiBonusCriticalChance, aiBonusPhysicalArmor, aiBonusMagicalArmor, aiTouchMeleeDice, aiToucheDistanceDice, aiToucheMagicalDice;
    [SerializeField]
    private List<Dice> aiDiceBonusDegPhy = new List<Dice>(), aiDiceBonusDegMag = new List<Dice>();

    public override int GetInitiativeBrut()
    {
        return base.GetInitiativeBrut() + aiBonusInitiative;
    }

    public override int GetInitiativeDice()
    {
        return GetPerception() + aiBonusInitiativeDice;
    }

    public override int GetInititativeBonus()
    {
        return bonusInitiative + aiBonusInitiative;
    }

    public void ResetComportement()
    {
        foreach(AiConsideration consid in comportement)
        {
            consid.cooldown = 0;
        }
    }

    public override void ResetStats()
    {
        bonusDegPhyMelee = aiBonusDegPhyMelee;
        bonusDegPhyDistance = aiBonusDegPhyDistance;
        bonusDegMag = aiBonusDegMag;
        bonusInitiative = aiBonusInitiative;
        bonusDefense = aiBonusDefense;
        bonusChanceToucheForce = aiBonusChanceToucheForce;
        bonusChanceToucheDexterite = aiBonusChanceToucheDexterite;
        bonusChanceToucheMagic = aiBonusChanceToucheMagic;
        bonusSoinAppli = aiBonusSoinAppli;
        bonusSoinRecu = aiBonusSoinRecu;
        bonusMaana = aiBonusMaana;
        bonusCriticalChance = aiBonusCriticalChance;
        bonusPhysicalArmor = aiBonusPhysicalArmor;
        bonusMagicalArmor = aiBonusMagicalArmor;

        touchMeleeDice = aiTouchMeleeDice;
        toucheDistanceDice = aiToucheDistanceDice;
        toucheMagicalDice = aiToucheMagicalDice;

        diceBonusDegPhy = aiDiceBonusDegPhy;
        diceBonusDegMag = aiDiceBonusDegMag;
    }
}
