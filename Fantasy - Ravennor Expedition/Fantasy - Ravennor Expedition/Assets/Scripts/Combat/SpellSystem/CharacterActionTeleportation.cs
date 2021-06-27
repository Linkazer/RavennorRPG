using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Teleportation Spell", menuName = "Spell/Teleportation Spell")]
public class CharacterActionTeleportation : CharacterActionScriptable
{
    [Header("Teleportation")]
    public Vector2Int positionToTeleport;
    public bool isJump;

    public CharacterActionDirect jumpEffect, landEffect;

    public CharacterActionTeleportation()
    {
        spellType = SpellType.Teleportation;
    }
}
