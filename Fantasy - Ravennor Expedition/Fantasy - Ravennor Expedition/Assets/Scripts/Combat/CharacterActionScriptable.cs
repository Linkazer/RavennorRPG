using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType { Force, Dexterite, PuissMagique}

public enum ActionIncantation { Rapide, Simple, Lent, Hard}

public enum ActionTargets { SelfAllies, Ennemies, All, FreeSpace, Invocations}

public enum DamageType { Heal, Physical, Magical, Brut}

[CreateAssetMenu(fileName = "New Battle Action", menuName = "Create New Battle Action")]
public class CharacterActionScriptable : ScriptableObject
{
    public string nom;
    public Sprite icon;
    [TextArea(3,5)]
    public string description;

    public AttackType attackType;
    public int actionLevel;
    public int maanaCost;
    public ActionIncantation incantationTime;

    public ActionTargets target;
    public ActionTargets AICastTarget;
    private int targetNumber;

    [Header("Dégâts")]
    public bool hasPowerEffect = true;
    //[SerializeField]
    //private List<int> diceNumbers;
    [SerializeField]
    private List<Dice> dices;
    //private List<DiceType> diceTypes;
    public bool useWeaponDamage;
    public DamageType damageType;
    public float diceByLevelBonus;
    public DiceType diceByLevel;
    public bool autoCritical = false;

    [Header("Forme")]
    public Vector2 range;
    //public bool straightLine, diagonal;
    public List<Vector2Int> activeZoneCases = new List<Vector2Int>();
    public bool doesFaceCaster;
    public bool hasViewOnTarget;
    [Header("Projectile")]
    public Sprite projectile;
    public float speed;

    [Header("Effets")]
    public SpellEffectScriptables wantedEffect;
    public bool applyOnTarget = false, applyOnCaster = false, applyOnGround = false;

    [Header("Special abilities")]
    public int projectionRange = 0;
    public PersonnageScriptables invocation;
    public float lifeStealPercent = 0;

    [Header("Affichages")]
    public Sprite zoneSprite;
    public Sprite caseSprite;
    public Sprite caseFeedback;
    public string animationOnNodeName, animationOnZoneName;


    public CharacterActionScriptable()
    {
        if(!activeZoneCases.Contains(Vector2Int.zero))
        {
            activeZoneCases.Add(Vector2Int.zero);
        }
    }

    public List<Dice> GetDices()
    {
        return dices;
    }

    public Dice GetLevelBonusDices(int casterLevel)
    {
        return new Dice(diceByLevel, Mathf.RoundToInt((casterLevel - actionLevel) / diceByLevelBonus), damageType);
    }

    public int DamageRoll(int casterLevel)
    {
        int result = 0;

        for (int i = 0; i < dices.Count; i++)
        {
            result += GameDices.RollDice(dices[i].numberOfDice, dices[i].wantedDice);
        }

        if (Mathf.RoundToInt((casterLevel - actionLevel) / diceByLevelBonus) > 0)
        {
            result += GameDices.RollDice(Mathf.RoundToInt((casterLevel - actionLevel) / diceByLevelBonus), diceByLevel);
        }

        return result;
    }

    public virtual void SpecialAction()
    {

    }

    public bool HadFeedback()
    {
        return (zoneSprite != null || caseSprite != null || animationOnNodeName != "" || animationOnZoneName != "");
}
}
