using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParcheminReponse
{
    [TextArea(2,3)]
    public string text;
    //Mettre les potentiels conditions

    public ParcheminScriptable nextStory;
}
