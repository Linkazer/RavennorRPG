using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Invocation Spell", menuName = "Spell/Invocation Spell")]
public class CharacterActionInvocation : CharacterActionScriptable
{
    public List<PersonnageScriptables> invocations;

    public override SpellType SpellType => SpellType.Invocation;
}
