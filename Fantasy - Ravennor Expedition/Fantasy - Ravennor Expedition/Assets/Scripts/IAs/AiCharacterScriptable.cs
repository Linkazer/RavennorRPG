using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AI", menuName = "Character/IA Character")]
public class AiCharacterScriptable : PersonnageScriptables
{
    [Header("IA")]

    public bool planForOtherTurns = true;

    public List<AiConsideration> comportement;

    public void ResetComportement()
    {
        foreach(AiConsideration consid in comportement)
        {
            consid.cooldown = 0;
        }
    }
}
