using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Parchemin dialogue", menuName = "Create Parchemin Story")]
public class ParcheminScriptable : ScriptableObject
{
    [TextArea(5, 25)]
    public string storyText;

    public List<ParcheminReponse> reponses;

    public bool battleBegin = false, goToCamp = false;
}
