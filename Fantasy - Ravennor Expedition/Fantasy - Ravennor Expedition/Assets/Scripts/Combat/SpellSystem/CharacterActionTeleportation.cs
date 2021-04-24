using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Teleportation Spell", menuName = "Spell/Create Teleportation Spell")]
public class CharacterActionTeleportation : CharacterActionScriptable
{
    [Header("Teleportation")]
    public Vector2Int positionToTeleport;
    public bool isJump;

    [Header("Effets")]
    public List<SpellEffectScriptables> wantedEffectOnTarget, wantedEffectOnCaster, wantedEffectOnGround;

    public CharacterActionTeleportation()
    {
        spellType = SpellType.Teleportation;
    }
}
