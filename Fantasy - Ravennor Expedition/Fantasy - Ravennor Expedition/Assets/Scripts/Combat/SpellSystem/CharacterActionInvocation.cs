using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Invocation Spell", menuName = "Spell/Create Invocation Spell")]
public class CharacterActionInvocation : CharacterActionScriptable
{
    public PersonnageScriptables invocation;

    public CharacterActionInvocation()
    {
        spellType = SpellType.Invocation;
    }
}
