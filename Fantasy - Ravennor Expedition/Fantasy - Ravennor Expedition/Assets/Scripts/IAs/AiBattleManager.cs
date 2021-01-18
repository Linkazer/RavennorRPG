﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiBattleManager : MonoBehaviour
{
    public static AiBattleManager instance;

    [SerializeField]
    private RuntimeBattleCharacter currentChara;
    private RuntimeBattleCharacter target;
    [SerializeField]
    private CharacterActionScriptable wantedAction;
    private Node nodeToMoveTo;

    [SerializeField]
    private LayerMask layerMaskObstacle;
    private RaycastHit2D hit;

    void Awake()
    {
        instance = this;
    }

    public void BeginNewTurn(RuntimeBattleCharacter newChara)
    {
        currentChara = newChara;
        nodeToMoveTo = null;

        foreach (AiConsideration consid in (currentChara.GetCharacterDatas() as AiCharacterScriptable).comportement)
        {
            consid.cooldown--;
        }

        SearchForBestAction(currentChara, BattleManager.instance.GetAllChara(), false);

        SearchNextMove();
    }

    public void SearchNextMove()
    {
        StartCoroutine(WaitBeforeTryAction());
    }

    IEnumerator WaitBeforeTryAction()
    {
        yield return new WaitForSeconds(1.5f);
        TryNextMove();
    }

    public void TryNextMove()
    {
        if (currentChara.GetCurrentHps() > 0)
        {
            if (currentChara.actionAvailable && wantedAction != null)
            {
                hit = Physics2D.Raycast(currentChara.currentNode.worldPosition, (target.transform.position - currentChara.currentNode.worldPosition).normalized, Vector2.Distance(target.transform.position, currentChara.currentNode.worldPosition), layerMaskObstacle);

                if ((Pathfinding.instance.GetDistance(currentChara.currentNode, target.currentNode) > wantedAction.range.y || hit.collider != null) && currentChara.currentNode.worldPosition != nodeToMoveTo.worldPosition)
                {
                    //Debug.Log(currentChara + " move for action");
                    BattleManager.instance.MoveCharacter(currentChara, nodeToMoveTo.worldPosition);
                    currentChara.movementLeft -= nodeToMoveTo.gCost;
                }
                else if ((Pathfinding.instance.GetDistance(currentChara.currentNode, target.currentNode) <= wantedAction.range.y))
                {
                    //Debug.Log(currentChara + " do action");
                    BattleManager.instance.LaunchAction(wantedAction, currentChara, target.transform.position);
                }
                else
                {
                    BattleManager.instance.EndTurn();
                }
            }
            else if (!currentChara.hasMoved)
            {
                if (target != null && target.GetCurrentHps() > 0)
                {
                    BattleManager.instance.EndTurn();
                }
                else
                {
                    Debug.Log("Move for next round");
                    currentChara.actionAvailable = false;
                    SearchForBestAction(currentChara, BattleManager.instance.GetAllChara(), true);

                    if (nodeToMoveTo != currentChara.currentNode && nodeToMoveTo != null)
                    {
                        //Debug.Log(" Movement Left Moving : " + nodeToMoveTo.worldPosition + " != " + currentChara.currentNode.worldPosition);
                        BattleManager.instance.MoveCharacter(currentChara, nodeToMoveTo.worldPosition);
                        currentChara.movementLeft -= nodeToMoveTo.gCost;
                    }
                    else
                    {
                        //Debug.Log(" Movement Left EndTurn");
                        BattleManager.instance.EndTurn();
                    }
                }
            }
            else
            {
                //Debug.Log("AI End turn");
                BattleManager.instance.EndTurn();
            }
        }
        else
        {
            BattleManager.instance.EndTurn();
        }
    }

    private void SearchForBestAction(RuntimeBattleCharacter caster, List<RuntimeBattleCharacter> targets, bool askForNextTurn)
    {
        float evaluatedValues = -99;

        AiCharacterScriptable aiCaster = caster.GetCharacterDatas() as AiCharacterScriptable;

        target = null;
        wantedAction = null;

        nodeToMoveTo = null;

        if (aiCaster != null)
        {
            float maxScore = -99;

            AiConsideration considToCooldown = null;

            foreach (AiConsideration consid in aiCaster.comportement)
            {
                if (consid.cooldown <= 0 || (askForNextTurn && consid.cooldown <= 1))
                {
                    //List<Node> toCheck = Pathfinding.instance.GetNodesWithMaxDistance(caster.currentNode, consid.wantedAction.range.y, false);

                    foreach (RuntimeBattleCharacter chara in targets)
                    {
                        //StartCoroutine(TestCanUse(consid.wantedAction, chara, askForNextTurn));
                        if (CanSpellBeUsed(consid.wantedAction, chara, askForNextTurn))
                        {
                            float newScore = EvaluateAction(consid, caster, chara);
                            if (newScore > maxScore)
                            {
                                maxScore = newScore;
                                evaluatedValues = newScore;
                                wantedAction = consid.wantedAction;
                                target = chara;
                                considToCooldown = consid;
                            }
                        }
                    }
                }
            }

            if(target == null)
            {
                nodeToMoveTo = GetClosestTargetNode(targets);
            }

            if (considToCooldown != null && !askForNextTurn)
            {
                considToCooldown.cooldown = considToCooldown.maxCooldown;
            }
        }
    }

    private IEnumerator TestCanUse(CharacterActionScriptable actionToTry, RuntimeBattleCharacter targetToTry, bool askForNextTurn)
    {
        List<Node> possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, currentChara.movementLeft, true);

        if (askForNextTurn || currentChara.movementLeft > 90)
        {
            possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, 90, true);
        }

        float rangeNeeded = actionToTry.range.y;
        /*if(askForNextTurn)
        {
            rangeNeeded += currentChara.GetMaxMovement();
        }*/

        Debug.Log("Boucle lenght : " + possibleDeplacement.Count);

        for (int i = 0; i < possibleDeplacement.Count; i++)
        {
            //Node n = possibleDeplacement[i];
            yield return new WaitForSeconds(Time.deltaTime);
            Debug.Log(currentChara + " Iteration");
            if (Pathfinding.instance.GetDistance(possibleDeplacement[i], targetToTry.currentNode) < rangeNeeded)
            {
                //hit = Physics2D.Raycast(n.worldPosition, (targetToTry.transform.position - n.worldPosition).normalized, Vector2.Distance(targetToTry.transform.position, n.worldPosition), layerMaskObstacle);

                if (hit.collider == null || !actionToTry.hasViewOnTarget || askForNextTurn)
                {
                    nodeToMoveTo = possibleDeplacement[i];

                    Debug.Log("Succeed");
                }
            }
        }
    }

    private bool CanSpellBeUsed(CharacterActionScriptable actionToTry, RuntimeBattleCharacter targetToTry, bool askForNextTurn)
    {
        //Debug.Log("Can spel be use");

        if (targetToTry.GetCurrentHps() < 0)
        {
            return false;
        }

        switch (actionToTry.AICastTarget)
        {
            case ActionTargets.Ennemies:
                if (targetToTry.GetTeam() != 1)
                {
                    return false;
                }
                break;
            case ActionTargets.SelfAllies:
                if (targetToTry.GetTeam() == 1)
                {
                    return false;
                }
                break;
        }


        if((actionToTry.attackType != AttackType.PuissMagique && currentChara.CheckForAffliction(Affliction.Atrophie)) || (actionToTry.attackType == AttackType.PuissMagique && currentChara.CheckForAffliction(Affliction.Silence)))
        {
            return false;
        }

        List<Node> possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, currentChara.movementLeft, true);

        if (askForNextTurn || currentChara.movementLeft > 50)
        {
            possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, 50, true);
        }

        float rangeNeeded = actionToTry.range.y;
        /*if(askForNextTurn)
        {
            rangeNeeded += currentChara.GetMaxMovement();
        }*/

        foreach(Node n in possibleDeplacement)
        {
            if(Pathfinding.instance.GetDistance(n, targetToTry.currentNode) < rangeNeeded)
            {
                hit = Physics2D.Raycast(n.worldPosition, (targetToTry.transform.position - n.worldPosition).normalized, Vector2.Distance(targetToTry.transform.position, n.worldPosition), layerMaskObstacle);

                if(hit.collider == null || !actionToTry.hasViewOnTarget || askForNextTurn)
                {
                    nodeToMoveTo = n;

                    return true;
                }
            }
        }

        return false;
    }

    private Node GetClosestTargetNode(List<RuntimeBattleCharacter> possiblesTargets)
    {
        float range = 1000;
        Node nodeToReturn = new Node();

        foreach(RuntimeBattleCharacter r in possiblesTargets)
        {
            if(r.GetTeam() != currentChara.GetTeam() &&  Vector2.Distance(r.currentNode.worldPosition, currentChara.currentNode.worldPosition) < range)
            {
                range = Vector2.Distance(r.currentNode.worldPosition, currentChara.currentNode.worldPosition);
                nodeToReturn = r.currentNode;
            }
        }

        return nodeToReturn;
    }

    #region Calculs de Considérations
    public float EvaluateAction(AiConsideration actionToEvaluate, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        float totalResult = 0;
        float coef = 0;

        foreach(ValueForCalcul value in actionToEvaluate.calculs)
        {
            totalResult += ConsiderationCalcul(value, caster, target);
            coef += value.calculImportance;
        }

        totalResult = totalResult / coef;
        if(totalResult > actionToEvaluate.maxValue)
        {
            totalResult = actionToEvaluate.maxValue;
        }
        return totalResult;
    }

    public float ConsiderationCalcul(ValueForCalcul values, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        float result = 0;
        float abcsissa = GetAbcsissaValue(values.abscissaValue, caster, target);

        if (values.maxValue < 1)
        {
            values.maxValue = 1;
        }

        switch (values.calculType)
        {
            case AiCalculType.Conditionnal:
                result = ConditionnalCalcul(abcsissa, values.constant, values.coeficient);
                break;
            case AiCalculType.Affine:
                result = AffineCalcul(abcsissa, values.constant, values.coeficient) / values.maxValue; ;
                break;
            case AiCalculType.Logarythm:
                if(values.coeficient>=1)
                {
                    values.coeficient = 1;
                }
                result = LogarythmCalcul(abcsissa, values.constant, values.coeficient) / (Mathf.Pow(values.maxValue, values.coeficient));
                break;
            case AiCalculType.Exponential:
                result = ExponentialCalcul(abcsissa, values.constant, values.coeficient) / Mathf.Exp(values.maxValue * values.coeficient);
                break;
            case AiCalculType.ReverseExponential:
                result = ExponentialReverseCalcul(abcsissa, values.constant, values.coeficient);
                break;
            case AiCalculType.Logistical:
                values.constant = Mathf.Clamp(values.constant, 0, values.maxValue);
                result = LogisticalCalcul(abcsissa, values.constant, values.coeficient);
                break;
        }

        result = Mathf.Clamp(result, 0, 1);

        return result*values.calculImportance;
    }

    public float GetAbcsissaValue(AiAbscissaType abcsissa, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        switch(abcsissa)
        {
            case AiAbscissaType.DistanceFromTarget:
                return Pathfinding.instance.GetDistance(caster.currentNode, target.currentNode);
            case AiAbscissaType.TargetMaxHp:
                return target.GetCharacterDatas().GetMaxHps();
            case AiAbscissaType.TargetCurrentHp:
                return target.GetCurrentHps();
            case AiAbscissaType.TargetPercentHp:
                return target.GetPercentHp();
            case AiAbscissaType.CasterMaxHp:
                return caster.GetCharacterDatas().GetMaxHps();
            case AiAbscissaType.CasterCurrentHp:
                return caster.GetCurrentHps();
            case AiAbscissaType.CasterPercentHp:
                return caster.GetPercentHp();
            case AiAbscissaType.TargetMalus:
                break;
            case AiAbscissaType.TargetBonus:
                break;
            case AiAbscissaType.TargetDangerosity:
                return target.GetDangerosity();
            case AiAbscissaType.TargetVulnerability:
                return target.GetVulnerability();
            case AiAbscissaType.TargetPhysicalArmor:
                return target.GetCharacterDatas().GetPhysicalArmor();
            case AiAbscissaType.TargetMagicalArmor:
                return target.GetCharacterDatas().GetMagicalArmor();
        }
        return 0;
    }

    public float ConditionnalCalcul(float x, float k, float c)
    {
        if(c>= 0 && x>k)
        {
            return 1;
        }
        else if(c < 0 && x < k)
        {
            return 1;
        }
        return 0;
    }

    public float AffineCalcul(float x, float k, float c)
    {
        return x * c + k;
    }

    public float LogarythmCalcul(float x, float k, float c)
    {
        return k - (Mathf.Pow(x, c));
    }

    public float ExponentialCalcul(float x, float k, float c)
    {
        return Mathf.Exp(c * x + k);
    }

    public float ExponentialReverseCalcul(float x, float k, float c)
    {
        return 1/Mathf.Exp(c * x + k);
    }

    public float LogisticalCalcul(float x, float k, float c)
    {
        return 1 / (1 + (Mathf.Exp((c-k) * x)));
    }
    #endregion
}
