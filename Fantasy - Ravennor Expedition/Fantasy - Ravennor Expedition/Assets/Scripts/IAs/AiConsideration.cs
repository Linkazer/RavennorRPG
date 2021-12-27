using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AiAbscissaType{ DistanceFromTarget,
                            TargetMaxHp, TargetCurrentHp, TargetPercentHp,
                            CasterMaxHp, CasterCurrentHp, CasterPercentHp,
                            TargetMalus, TargetBonus,
                            TargetDangerosity, TargetVulnerability,
                            TargetPhysicalArmor, TargetMagicalArmor,
                            NumberEnnemyArea, NumberAllyArea, NumberWoundedEnnemyArea, NumberWoundedAllyArea
                          }

public enum AiCalculType{ Conditionnal, Affine, Logarythm,
                          Exponential, ReverseExponential,
                          Logistical
                        }

public enum AiConditionType { None, UpOrEqual, DownOrEqual, Equal }

[System.Serializable]
public class ValueForCalcul
{
    public AiCalculType calculType;
    public AiAbscissaType abscissaValue;
    public bool checkAroundMax;
    public float maxValue;
    public float constant;
    public float coeficient;
    public float calculImportance = 1;

    public ValueForCalcul()
    {
        calculImportance = 1;
    }
}

[System.Serializable]
public class ValueForCondition
{
    public AiAbscissaType conditionWanted;
    public float conditionValue;
    public AiConditionType conditionType;
}

[System.Serializable]
public class AiConsideration
{
    [Header("Actions")]
    public CharacterActionScriptable wantedAction;
    [Header("Condition")]
    public List<ValueForCondition> conditions;
    [Header("Calculs")]
    [Tooltip("Minimum -1 si on veut que la Considération ne soit pas prise en compte.")] public float considerationImportance = 0;
    [Tooltip("Met une limite au score maximum des calculs.")] public float maximumValueModifier;
    public float startScore;
    public List<ValueForCalcul> calculs;
    public int maxCooldown, cooldown;
}
