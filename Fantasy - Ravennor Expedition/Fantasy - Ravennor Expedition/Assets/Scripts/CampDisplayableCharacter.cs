using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Displayable", menuName = "Character/ Displayer personnage")]
public class CampDisplayableCharacter : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private Sprite sprite;
    [SerializeField] private Vector2 position;
    [SerializeField] private Vector3 scale = Vector3.one;
    [SerializeField] private PersonnageScriptables scriptable;

    public Sprite Sprite => sprite;

    public Vector2 Position => position;

    public Vector3 Scale => scale;

    public PersonnageScriptables Scriptable => scriptable;

    public string ID => id;
}
