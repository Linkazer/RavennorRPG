using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellType { Direct, Projectile, Deplacement, Invocation, Teleportation, DamagingTeleportation, SimpleEffect}

public enum AttackType { Physical, Magical}

public enum ActionIncantation { Rapide, Simple, Lent, Hard}

public enum ActionTargets { SelfAllies, Ennemies, All, FreeSpace, Invocations}

public enum DamageType { Heal, Physical, Magical, Brut}

public enum ScalePossibility { None, EffectStack, HpLostPercent, Distance}

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
    public int maanaCost;
    public bool canUseMoreMaana = true;
    [SerializeField]
    private int maxCooldown = 0;
    public int maxUtilisation = -1;
    public ActionIncantation incantationTime;
    public bool isWeaponBased;

    public ActionTargets target = ActionTargets.All;
    public ActionTargets castTarget = ActionTargets.All;
    public bool castOnSelf;

    [Header("Forme")]
    //public Vector2 range;
    public int range;
    //public bool straightLine, diagonal;
    public List<Vector2Int> activeZoneCases = new List<Vector2Int>();
    public bool doesFaceCaster = true;
    public bool hasViewOnTarget = true;
    [Header("Projectile")]
    public Sprite projectile;
    public float speed;

    [Header("Affichages")]
    public Sprite zoneSprite;
    public Sprite caseSprite;
    public Sprite caseFeedback;

    public AudioClip soundToPlay;

    //public string animationOnNodeName, animationOnZoneName;

    [Header("Effets")]
    public List<SpellEffectScriptables> wantedEffectOnTarget, wantedEffectOnCaster, wantedEffectOnGround;

    public CharacterActionScriptable()
    {
        if(!activeZoneCases.Contains(Vector2Int.zero))
        {
            activeZoneCases.Add(Vector2Int.zero);
        }
    }


    public bool HadFeedback()
    {
        return (zoneSprite != null || caseSprite != null);
    }

    public int GetMaxCooldown()
    {
        return maxCooldown;
    }
}
