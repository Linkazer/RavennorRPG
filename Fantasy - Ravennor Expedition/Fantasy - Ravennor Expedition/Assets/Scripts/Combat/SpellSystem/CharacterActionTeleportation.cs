using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Teleportation Spell", menuName = "Spell/Teleportation Spell")]
public class CharacterActionTeleportation : CharacterActionScriptable
{
    [Header("Teleportation")]
    public List<Vector2Int> positionsToTeleport = new List<Vector2Int>();
    public bool isJump;

    public CharacterActionDirect jumpEffect, landEffect;

    public override SpellType SpellType => SpellType.Teleportation;
}
