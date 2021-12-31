using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectNature { Physical, Magical }

public enum Affliction{ None, Paralysie, Immobilisation, Silence, Atrophie, InstantKill, Evasion}

[CreateAssetMenu(fileName = "New Spell Effect", menuName = "Spell/Effet")]
public class SpellEffectScriptables : ScriptableObject
{
    public SpellEffectCommon effet;

    public EffectNature nature;
    public int duree;
    public bool isMalus;

    public List<SpellEffectScriptables> bonusToCancel;

    public Sprite spriteZone, spriteCase;
}
