﻿using System.Collections;
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

    //Targets
    //Applications ??

    //public List<SpellEffect> effects;

    //public DamageType onTimeEffectType;
    //public int effectOnTimePower;
    /*public Dice diceOnTime;
    public float diceByLevelBonus;
    public DiceType diceByLevel;*/
    //public Affliction affliction;

    public Sprite spriteZone, spriteCase;
}
