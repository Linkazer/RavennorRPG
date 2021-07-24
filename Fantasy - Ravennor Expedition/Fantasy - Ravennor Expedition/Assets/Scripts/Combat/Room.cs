using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room
{
    public List<PersonnageScriptables> ennemis;
    public List<Transform> ennemisPos;

    //[HideInInspector]
    public List<Vector2> ennemisPositions;

    public List<GameObject> toEnable;
    public List<GameObject> toDisable;
}
