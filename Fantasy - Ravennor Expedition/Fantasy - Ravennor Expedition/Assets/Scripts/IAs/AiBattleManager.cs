using System.Collections;
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
            if (wantedAction != null && currentChara.CanDoAction(wantedAction.isWeaponBased))
            {
                hit = Physics2D.Raycast(currentChara.currentNode.worldPosition, (target.transform.position - currentChara.currentNode.worldPosition).normalized, Vector2.Distance(target.transform.position, currentChara.currentNode.worldPosition), layerMaskObstacle);

                if ((Pathfinding.instance.GetDistance(currentChara.currentNode, target.currentNode) > wantedAction.range || hit.collider != null) && currentChara.currentNode.worldPosition != nodeToMoveTo.worldPosition)
                {
                    //Debug.Log(currentChara + " move for action");
                    BattleManager.instance.MoveCharacter(currentChara, nodeToMoveTo.worldPosition, false);
                    currentChara.movementLeft -= nodeToMoveTo.gCost;
                }
                else if ((Pathfinding.instance.GetDistance(currentChara.currentNode, target.currentNode) <= wantedAction.range))
                {
                    //Debug.Log(currentChara + " do action");
                    currentChara.SetCooldown(wantedAction);
                    BattleManager.instance.LaunchAction(wantedAction, currentChara, target.transform.position, false);
                }
                else
                {
                    BattleManager.instance.EndTurn();
                }
            }
            else if (currentChara.CanMove)
            {
                if (target != null && (target.GetCurrentHps() > 0 || Pathfinding.instance.GetDistance(target.currentNode, currentChara.currentNode) <= 15))
                {
                    BattleManager.instance.EndTurn();
                }
                else
                {
                    currentChara.UseAllAction();
                    SearchForBestAction(currentChara, BattleManager.instance.GetAllChara(), true);

                    //Debug.Log(currentChara.name + " Move for next round : " + nodeToMoveTo.worldPosition);

                    Debug.Log(nodeToMoveTo);

                    if (nodeToMoveTo != currentChara.currentNode && nodeToMoveTo != null)
                    {
                        //Debug.Log(" Movement Left Moving : " + nodeToMoveTo.worldPosition + " != " + currentChara.currentNode.worldPosition);
                        BattleManager.instance.MoveCharacter(currentChara, nodeToMoveTo.worldPosition, true);
                        currentChara.movementLeft -= nodeToMoveTo.gCost;
                    }
                    else
                    {
                        nodeToMoveTo = GetClosestTargetNode(BattleManager.instance.GetPlayerChara());
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

        //Condition de l'IA

        float absice = GetAbcsissaValue(consid.conditionWanted, currentChara, targetToTry);

        //Debug.Log(currentChara.name + " " + absice);

        switch(consid.conditionType)
        {
            case AiConditionType.Up:
                if(absice<consid.conditionValue)
                {
                    return false;
                }
                break;
            case AiConditionType.Down:
                //Debug.Log("Down : " + absice + " > " + consid.conditionValue);
                if (absice > consid.conditionValue)
                {
                    //Debug.Log("Returned");
                    return false;
                }
                break;
            case AiConditionType.Equal:
                if (absice != consid.conditionValue)
                {
                    return false;
                }
                break;
        }

        //Condition pour l'Action

        if (targetToTry.GetCurrentHps() < 0)
        {
            return false;
        }

        if ((actionToTry.attackType != AttackType.Magical && currentChara.CheckForAffliction(Affliction.Atrophie)) || (actionToTry.attackType == AttackType.Magical && currentChara.CheckForAffliction(Affliction.Silence)))
        {
            return false;
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
                hit = Physics2D.Raycast(n.worldPosition, (targetToTry.transform.position - n.worldPosition).normalized, Vector2.Distance(targetToTry.transform.position, n.worldPosition), layerMaskObstacle);

                if(hit.collider == null || !actionToTry.hasViewOnTarget || askForNextTurn)
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
                hit = Physics2D.Raycast(n.worldPosition, (wantedTarget.transform.position - n.worldPosition).normalized, Vector2.Distance(wantedTarget.transform.position, n.worldPosition), layerMaskObstacle);

                if (hit.collider == null || !needView)
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
                if (Vector2.Distance(n.worldPosition, currentChara.currentNode.worldPosition) < range)
                {
                    range = Vector2.Distance(n.worldPosition, currentChara.currentNode.worldPosition);
                    nodeToReturn = n;
                }
            }
        }
        Debug.Log(nodeToReturn.worldPosition.ToString("F4"));
        return nodeToReturn;
    }

    #region Calculs de Considérations
    public float EvaluateAction(AiConsideration actionToEvaluate, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        float totalResult = 0;
        float coef = 0;

        foreach(ValueForCalcul value in actionToEvaluate.calculs)
        {
            totalResult += ConsiderationCalcul(value, caster, target, actionToEvaluate.maxValue);
            coef += value.calculImportance;
        }

        totalResult = totalResult / coef;
        if(totalResult > actionToEvaluate.maxValue)
        {
            totalResult = actionToEvaluate.maxValue;
        }
        return totalResult;
    }

    public float ConsiderationCalcul(ValueForCalcul values, RuntimeBattleCharacter caster, RuntimeBattleCharacter target, float maxValue)
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

        result = Mathf.Clamp(result * values.calculImportance, 0, maxValue);

        return result;
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
