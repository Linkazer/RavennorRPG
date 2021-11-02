using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Room
{
    public List<PersonnageScriptables> ennemis;
    public List<Transform> ennemisPos;

    //[HideInInspector]
    public List<Vector2> ennemisPositions;

    public UnityEvent toPlayOnEnable;
}
