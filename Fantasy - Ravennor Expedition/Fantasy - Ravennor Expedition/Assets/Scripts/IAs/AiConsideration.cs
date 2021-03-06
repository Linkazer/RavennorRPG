﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AiAbscissaType{ DistanceFromTarget,
                            TargetMaxHp, TargetCurrentHp, TargetPercentHp,
                            CasterMaxHp, CasterCurrentHp, CasterPercentHp,
                            TargetMalus, TargetBonus,
                            TargetDangerosity, TargetVulnerability,
                            TargetPhysicalArmor, TargetMagicalArmor
                          }

public enum AiCalculType{ Conditionnal, Affine, Logarythm,
                          Exponential, ReverseExponential,
                          Logistical
                        }

public enum AiConditionType { None, Up, Down, Equal }

[System.Serializable]
public class ValueForCalcul
{
    public AiCalculType calculType;
    public AiAbscissaType abscissaValue;
    public float maxValue;
    public float constant;
    public float coeficient;
    public float calculImportance = 1;
}

[System.Serializable]
public class AiConsideration
{
    [Header("Actions")]
    public CharacterActionScriptable wantedAction;
    [Header("Condition")]
    public AiAbscissaType conditionWanted;
    public float conditionValue;
    public AiConditionType conditionType;
    [Header("Calculs")]
    public float maxValue = 1;
    public List<ValueForCalcul> calculs;
    public int maxCooldown, cooldown;
}
