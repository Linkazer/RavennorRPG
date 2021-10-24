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

        target = null;

        SearchForBestAction(currentChara, BattleManager.instance.GetAllChara(), false);

        SearchNextMove(1.5f);
    }

    public void SearchNextMove(float timeToWait)
    {
        StartCoroutine(WaitBeforeTryAction(timeToWait));
    }

    IEnumerator WaitBeforeTryAction(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        TryNextMove();
    }

    public void TryNextMove()
    {
        if (currentChara.GetCurrentHps() > 0)
        {
            if (wantedAction != null && currentChara.CanDoAction())
            {
                hit = Physics2D.Raycast(currentChara.currentNode.worldPosition, (target.transform.position - currentChara.currentNode.worldPosition).normalized, Vector2.Distance(target.transform.position, currentChara.currentNode.worldPosition), layerMaskObstacle);

                if ((Pathfinding.instance.GetDistance(currentChara.currentNode, target.currentNode) > wantedAction.range || hit.collider != null) && currentChara.currentNode.worldPosition != nodeToMoveTo.worldPosition)
                {
                    Debug.Log(currentChara + " move for action : " + nodeToMoveTo.worldPosition.ToString("F4"));
                    BattleManager.instance.MoveCharacter(currentChara, nodeToMoveTo.worldPosition, false);
                    currentChara.movementLeft -= nodeToMoveTo.gCost;
                }
                else if ((Pathfinding.instance.GetDistance(currentChara.currentNode, target.currentNode) <= wantedAction.range))
                {
                    currentChara.SetCooldown(wantedAction);
                    BattleManager.instance.LaunchAction(wantedAction, 0, currentChara, target.transform.position, false);
                }
                else
                {
                    Debug.Log("End Turn");
                    BattleManager.instance.EndTurn();
                }
            }
            else if (currentChara.CanMove && (currentChara.GetCharacterDatas() as AiCharacterScriptable).planForOtherTurns)
            {
                int wantedDist = 15;
                if(wantedAction != null)
                {
                    wantedDist = wantedAction.range;
                }
                if (target != null && (target.GetCurrentHps() > 0 && Pathfinding.instance.GetDistance(target.currentNode, currentChara.currentNode) <= wantedDist))
                {
                    BattleManager.instance.EndTurn();
                }
                else
                {
                    currentChara.UseAllAction();
                    SearchForBestAction(currentChara, BattleManager.instance.GetAllChara(), true);

                    //Debug.Log(currentChara.name + " Move for next round : " + nodeToMoveTo.worldPosition);

                    if (nodeToMoveTo != currentChara.currentNode && nodeToMoveTo != null)
                    {
                        //Debug.Log(" Movement Left Moving : " + nodeToMoveTo.worldPosition + " != " + currentChara.currentNode.worldPosition);
                        BattleManager.instance.MoveCharacter(currentChara, nodeToMoveTo.worldPosition, true);
                        currentChara.movementLeft -= nodeToMoveTo.gCost;
                    }
                    else
                    {
                        nodeToMoveTo = GetClosestTargetNode(BattleManager.instance.GetPlayerChara());
                        //Debug.Log("From : " + currentChara.currentNode.worldPosition + " to : " + nodeToMoveTo.worldPosition);
                        if (nodeToMoveTo != currentChara.currentNode && nodeToMoveTo != null)
                        {
                            BattleManager.instance.MoveCharacter(currentChara, nodeToMoveTo.worldPosition, true);
                            currentChara.movementLeft -= nodeToMoveTo.gCost;
                        }
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

        nodeToMoveTo = currentChara.currentNode;

        if (aiCaster != null)
        {
            float maxScore = -99;

            AiConsideration considToCooldown = null;

            foreach (AiConsideration consid in aiCaster.comportement)
            {
                if ((consid.cooldown <= 0 && caster.GetSpellCooldown(consid.wantedAction)<=0) || (askForNextTurn && consid.cooldown <= 1 && caster.GetSpellCooldown(consid.wantedAction) <= 1))
                {
                    //List<Node> toCheck = Pathfinding.instance.GetNodesWithMaxDistance(caster.currentNode, consid.wantedAction.range.y, false);

                    foreach (RuntimeBattleCharacter chara in targets)
                    {
                        //StartCoroutine(TestCanUse(consid.wantedAction, chara, askForNextTurn));
                        if (CanSpellBeUsed(consid, consid.wantedAction, chara, askForNextTurn))
                        {
                            float newScore = EvaluateAction(consid, caster, chara);
                            if (newScore > maxScore)
                            {
                                if (askForNextTurn)
                                {
                                    nodeToMoveTo = GetNodeToHitTarget(chara, consid.wantedAction.range, consid.wantedAction.hasViewOnTarget, 1000);
                                }
                                else
                                {
                                    nodeToMoveTo = GetNodeToHitTarget(chara, consid.wantedAction.range, consid.wantedAction.hasViewOnTarget, 0);
                                }
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

            if (considToCooldown != null && !askForNextTurn)
            {
                considToCooldown.cooldown = considToCooldown.maxCooldown;
            }
        }
    }

    private bool CanSpellBeUsed(AiConsideration consid, CharacterActionScriptable actionToTry, RuntimeBattleCharacter targetToTry, bool askForNextTurn)
    {
        switch (actionToTry.castTarget)
        {
            case ActionTargets.Ennemies:
                if (targetToTry.GetTeam() != 0)
                {
                    return false;
                }
                break;
            case ActionTargets.SelfAllies:
                if (targetToTry.GetTeam() == 0)
                {
                    return false;
                }
                break;
        }

        //Condition de l'IA

        for (int i = 0; i < consid.conditions.Count; i++)
        {
            ValueForCondition condition = consid.conditions[i];

            float absice = GetAbcsissaValue(consid.wantedAction, condition.conditionWanted, currentChara, targetToTry);

            switch (condition.conditionType)
            {
                case AiConditionType.Up:
                    if (absice < condition.conditionValue)
                    {
                        return false;
                    }
                    break;
                case AiConditionType.Down:
                    if (absice > condition.conditionValue)
                    {
                        return false;
                    }
                    break;
                case AiConditionType.Equal:
                    if (absice != condition.conditionValue)
                    {
                        return false;
                    }
                    break;
            }
        }

        //Condition pour l'Action

        if (targetToTry.GetCurrentHps() <= 0)
        {
            return false;
        }

        if ((actionToTry.attackType != AttackType.Magical && currentChara.CheckForAffliction(Affliction.Atrophie)) || (actionToTry.attackType == AttackType.Magical && currentChara.CheckForAffliction(Affliction.Silence)))
        {
            return false;
        }

        if(actionToTry.SpellType == SpellType.Teleportation)
        {
            CharacterActionTeleportation teleportActionToTry = actionToTry as CharacterActionTeleportation;
            int i = 0;
            for(i = 0; i < teleportActionToTry.positionsToTeleport.Count; i++)
            {
                Vector2Int positionToTry = new Vector2Int(targetToTry.currentNode.gridX, targetToTry.currentNode.gridY);
                Vector3 wolrdPosToTry = BattleManager.GetTargetPosWithFacingPosition(currentChara.currentNode.worldPosition, positionToTry, teleportActionToTry.positionsToTeleport[i]);
                if (Grid.instance.NodeFromWorldPoint(wolrdPosToTry).walkable)
                {
                    break;
                }
            }
            if(i >= teleportActionToTry.positionsToTeleport.Count)
            {
                return false;
            }
        }

        List<Node> possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, currentChara.movementLeft, true);

        if (askForNextTurn)
        {
            possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, 150, true);
        }

        float rangeNeeded = actionToTry.range;

        bool foundSomething = false;

        foreach(Node n in possibleDeplacement)
        {
            if(Pathfinding.instance.GetDistance(n, targetToTry.currentNode) <= rangeNeeded)
            {
                if(actionToTry.hasViewOnTarget && BattleManager.instance.IsNodeVisible(targetToTry.currentNode, n) || !actionToTry.hasViewOnTarget || askForNextTurn)
                {
                    return true;
                }
            }
        }

        return foundSomething;
    }

    private Node GetNodeToHitTarget(RuntimeBattleCharacter wantedTarget, float rangeWanted, bool needView, int deplacementBoost)
    {
        Node nodeToReturn = currentChara.currentNode;

        List<Node> possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, currentChara.movementLeft + deplacementBoost, true);

        float currentDistance = 1000;

        foreach (Node n in possibleDeplacement)
        {
            if (Pathfinding.instance.GetDistance(n, wantedTarget.currentNode) <= rangeWanted)
            {
                bool hasView = BattleManager.instance.IsNodeVisible(n, wantedTarget.currentNode);

                if (hasView || !needView)
                {
                    if (Pathfinding.instance.GetDistance(currentChara.currentNode, n) < currentDistance)
                    {
                        nodeToReturn = n;
                        currentDistance = Pathfinding.instance.GetDistance(currentChara.currentNode, n);
                    }
                }
            }
        }

        return nodeToReturn;
    }

    private Node GetClosestTargetNode(List<RuntimeBattleCharacter> possiblesTargets)
    {
        float range = 1000;
        Node nodeToReturn = currentChara.currentNode;

        foreach(RuntimeBattleCharacter r in possiblesTargets)
        {
            foreach (Node n in Grid.instance.GetNeighbours(r.currentNode))
            {
                if (n.walkable && Vector2.Distance(n.worldPosition, currentChara.currentNode.worldPosition) < range)
                {
                    range = Vector2.Distance(n.worldPosition, currentChara.currentNode.worldPosition);
                    nodeToReturn = n;
                }
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
            totalResult += ConsiderationCalcul(actionToEvaluate.wantedAction, value, caster, target, actionToEvaluate.maxValue);
            coef += value.calculImportance;
        }

        totalResult = totalResult / coef;
        if(totalResult > actionToEvaluate.maxValue)
        {
            totalResult = actionToEvaluate.maxValue;
        }
        return totalResult;
    }

    public float ConsiderationCalcul(CharacterActionScriptable spell, ValueForCalcul values, RuntimeBattleCharacter caster, RuntimeBattleCharacter target, float maxValue)
    {
        float result = 0;
        float abcsissa = GetAbcsissaValue(spell, values.abscissaValue, caster, target);

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

        result = Mathf.Clamp(result * values.calculImportance, 0, maxValue);

        return result;
    }

    public float GetAbcsissaValue(CharacterActionScriptable spell, AiAbscissaType abcsissa, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
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
                return target.GetCharacterDatas().GetArmor();
            case AiAbscissaType.NumberEnnemyArea:
                return GetCharacterInArea(caster, target.currentNode, spell, false);
            case AiAbscissaType.NumberAllyArea:
                return GetCharacterInArea(caster, target.currentNode, spell, true);
        }
        return 0;
    }

    private int GetCharacterInArea(RuntimeBattleCharacter caster, Node targetNode, CharacterActionScriptable spell, bool isAlly)
    {
        List<Node> toTest = BattleManager.GetHitNodes(targetNode.worldPosition, caster.currentNode.worldPosition, spell);
        int charaAmount = 0;
        for(int i = 0; i < toTest.Count; i++)
        {
            if(toTest[i].HasCharacterOn)
            {
                if(toTest[i].chara.GetTeam() == caster.GetTeam() && isAlly)
                {
                    charaAmount++;
                }
                else if (toTest[i].chara.GetTeam() != caster.GetTeam() && !isAlly)
                {
                    charaAmount++;
                }
            }
        }
        return charaAmount;
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
