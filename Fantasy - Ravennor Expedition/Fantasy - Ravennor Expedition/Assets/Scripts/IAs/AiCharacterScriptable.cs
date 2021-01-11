using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AI", menuName = "Create New AI Character")]
public class AiCharacterScriptable : PersonnageScriptables
{
    public List<AiConsideration> comportement;

    public void ResetComportement()
    {
        foreach(AiConsideration consid in comportement)
        {
            consid.cooldown = 0;
        }
    }
}
