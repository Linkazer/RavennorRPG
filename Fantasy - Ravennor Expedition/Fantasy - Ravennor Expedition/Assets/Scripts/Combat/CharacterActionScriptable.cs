using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellType { Direct, Projectile, Deplacement, Invocation}

public enum AttackType { Force, Dexterite, PuissMagique}

public enum ActionIncantation { Rapide, Simple, Lent, Hard}

public enum ActionTargets { SelfAllies, Ennemies, All, FreeSpace, Invocations}

public enum DamageType { Heal, Physical, Magical, Brut}

//[CreateAssetMenu(fileName = "New Battle Action", menuName = "Create New Battle Action")]
public class CharacterActionScriptable : ScriptableObject
{
    public string nom;
    public Sprite icon;
    [TextArea(3,5)]
    public string description;

    [HideInInspector]
    public SpellType spellType;

    public AttackType attackType;
    public int actionLevel;
    public int maanaCost;
    public ActionIncantation incantationTime;

    public ActionTargets target;
    public ActionTargets AICastTarget;
    protected int targetNumber;

    [Header("Forme")]
    public Vector2 range;
    //public bool straightLine, diagonal;
    public List<Vector2Int> activeZoneCases = new List<Vector2Int>();
    public bool doesFaceCaster;
    public bool hasViewOnTarget;
    [Header("Projectile")]
    public Sprite projectile;
    public float speed;

    [Header("Affichages")]
    public Sprite zoneSprite;
    public Sprite caseSprite;
    public Sprite caseFeedback;
    public string animationOnNodeName, animationOnZoneName;

    [SerializeField]
    private int maxCooldown = 0;

    public CharacterActionScriptable()
    {
        if(!activeZoneCases.Contains(Vector2Int.zero))
        {
            activeZoneCases.Add(Vector2Int.zero);
        }
    }


    public bool HadFeedback()
    {
        return (zoneSprite != null || caseSprite != null || animationOnNodeName != "" || animationOnZoneName != "");
    }

    public int GetMaxCooldown()
    {
        return maxCooldown;
    }
}
