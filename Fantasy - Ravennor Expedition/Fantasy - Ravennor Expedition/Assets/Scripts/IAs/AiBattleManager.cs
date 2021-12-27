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
                //Debug.Log("AI do something");
                hit = Physics2D.Raycast(currentChara.currentNode.worldPosition, (target.transform.position - currentChara.currentNode.worldPosition).normalized, Vector2.Distance(target.transform.position, currentChara.currentNode.worldPosition), layerMaskObstacle);

                /*(Pathfinding.instance.GetDistance(currentChara.currentNode, target.currentNode) > wantedAction.range || hit.collider != null)*/ // Si on a un bug de déplacement des IA
                if (nodeToMoveTo != null && currentChara.currentNode.worldPosition != nodeToMoveTo.worldPosition)
                {
                    //Debug.Log(currentChara + " move for action : " + nodeToMoveTo.worldPosition.ToString("F4"));
                    BattleManager.instance.MoveCharacter(currentChara, nodeToMoveTo.worldPosition, false);
                    currentChara.movementLeft -= nodeToMoveTo.gCost;
                }
                else if ((Pathfinding.instance.GetDistance(currentChara.currentNode, target.currentNode) <= wantedAction.range))
                {
                    currentChara.SetCooldown(wantedAction);
                    BattleManager.instance.LaunchAction(wantedAction, 0, currentChara, target.transform.position, false);
                    if (currentChara.CanDoAction())
                    {
                        //Debug.Log("Can do action");
                        wantedAction = null;
                        SearchForBestAction(currentChara, BattleManager.instance.GetAllChara(), false);
                    }
                }
                else
                {
                    Debug.Log("End Turn");
                    BattleManager.instance.EndTurn();
                }
            }
            else if (currentChara.CanMove && (currentChara.GetCharacterDatas() as AiCharacterScriptable).planForOtherTurns)
            {
                int wantedDist = (currentChara.GetCharacterDatas() as AiCharacterScriptable).closestDistanceWhenNoAction;
                if(wantedAction != null)
                {
                    wantedDist = wantedAction.range;
                }

                if (target != null && (target.GetCurrentHps() > 0 && Pathfinding.instance.GetDistance(target.currentNode, currentChara.currentNode) <= wantedDist))
                {
                    Debug.Log("End Turn Target Available");
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
                        if (nodeToMoveTo != null && Pathfinding.instance.GetDistance(nodeToMoveTo, currentChara.currentNode) <= wantedDist)
                        {
                            Debug.Log("End Turn close to Wanted Node");
                            BattleManager.instance.EndTurn();
                        }
                        else if (nodeToMoveTo != currentChara.currentNode && nodeToMoveTo != null)
                        {
                            BattleManager.instance.MoveCharacter(currentChara, nodeToMoveTo.worldPosition, true);
                            currentChara.movementLeft -= nodeToMoveTo.gCost;
                        }
                        else
                        {
                            Debug.Log("AI End turn No possible movement");
                            BattleManager.instance.EndTurn();
                        }
                    }
                }
            }
            else
            {
                Debug.Log("AI End turn");
                BattleManager.instance.EndTurn();
            }
        }
        else
        {
            Debug.Log("AI End Turn Last");
            BattleManager.instance.EndTurn();
        }
    }

    private void SearchForBestAction(RuntimeBattleCharacter caster, List<RuntimeBattleCharacter> targets, bool askForNextTurn)
    {
        AiCharacterScriptable aiCaster = caster.GetCharacterDatas() as AiCharacterScriptable;

        target = null;
        wantedAction = null;

        nodeToMoveTo = currentChara.currentNode;

        if (aiCaster != null)
        {
            float maxScore = -99;

            AiConsideration considToCooldown = null;

            List<CharacterActionScriptable> possiblesActions = new List<CharacterActionScriptable>();
            List<Node> possiblesActionsMoveNeeded = new List<Node>();
            List<RuntimeBattleCharacter> possiblesActionsTargets = new List<RuntimeBattleCharacter>();

            int considCount = 0;

            foreach (AiConsideration consid in aiCaster.comportement)
            {
                considCount++;
                if ((consid.cooldown <= 0 && caster.GetSpellCooldown(consid.wantedAction)<=0) || (askForNextTurn && consid.cooldown <= 1 && caster.GetSpellCooldown(consid.wantedAction) <= 1))
                {
                    //List<Node> toCheck = Pathfinding.instance.GetNodesWithMaxDistance(caster.currentNode, consid.wantedAction.range.y, false);

                    foreach (RuntimeBattleCharacter chara in targets)
                    {
                        List<Node> possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, currentChara.movementLeft, true);

                        foreach (Node casterPosition in possibleDeplacement)
                        {
                            //StartCoroutine(TestCanUse(consid.wantedAction, chara, askForNextTurn));
                            if (CanSpellBeUsed(consid, consid.wantedAction, casterPosition, chara, askForNextTurn))
                            {
                                float newScore = EvaluateAction(consid, casterPosition, caster, chara);
                                Debug.Log(caster + " try " + consid.wantedAction + " on " + chara + " Score : " + newScore + "(Consideration Nb : " + considCount);
                                if (newScore >= maxScore)
                                {
                                    if (newScore > maxScore)
                                    {
                                        possiblesActions = new List<CharacterActionScriptable>();
                                        possiblesActionsMoveNeeded = new List<Node>();
                                        possiblesActionsTargets = new List<RuntimeBattleCharacter>();
                                    }

                                    if (askForNextTurn)
                                    {
                                        nodeToMoveTo = GetNodeToHitTarget(chara, consid.wantedAction.range, consid.wantedAction.hasViewOnTarget, 1000);
                                    }
                                    else if (casterPosition != caster.currentNode)
                                    {
                                        nodeToMoveTo = casterPosition;
                                    }
                                    else
                                    {
                                        nodeToMoveTo = GetNodeToHitTarget(chara, consid.wantedAction.range, consid.wantedAction.hasViewOnTarget, 0);
                                    }
                                    maxScore = newScore;
                                    wantedAction = consid.wantedAction;
                                    target = chara;
                                    considToCooldown = consid;

                                    possiblesActions.Add(wantedAction);
                                    possiblesActionsMoveNeeded.Add(nodeToMoveTo);
                                    possiblesActionsTargets.Add(target);
                                }
                            }
                        }
                    }
                }
            }

            if (possiblesActions.Count > 0)
            {
                int randomAction = Random.Range(0, possiblesActions.Count);
                wantedAction = possiblesActions[randomAction];
                nodeToMoveTo = possiblesActionsMoveNeeded[randomAction];
                target = possiblesActionsTargets[randomAction];
            }
            else
            {
                wantedAction = null;
                nodeToMoveTo = currentChara.currentNode;
                target = null;
            }

            if (considToCooldown != null && !askForNextTurn)
            {
                considToCooldown.cooldown = considToCooldown.maxCooldown;
            }
        }
    }

    private bool CanSpellBeUsed(AiConsideration consid, CharacterActionScriptable actionToTry, Node nodeToTry, RuntimeBattleCharacter targetToTry, bool askForNextTurn)
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

        if(!HasVision(nodeToTry, targetToTry.currentNode, actionToTry.range))
        {
            return false;
        }

        //Condition de l'IA

        for (int i = 0; i < consid.conditions.Count; i++)
        {
            ValueForCondition condition = consid.conditions[i];

            float absice = GetAbcsissaValue(consid.wantedAction, condition.conditionWanted, nodeToTry, targetToTry.currentNode, currentChara, targetToTry);

            switch (condition.conditionType)
            {
                case AiConditionType.UpOrEqual:
                    if (absice < condition.conditionValue)
                    {
                        return false;
                    }
                    break;
                case AiConditionType.DownOrEqual:
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
                Vector3 wolrdPosToTry = BattleManager.GetTargetPosWithFacingPosition(nodeToTry.worldPosition, positionToTry, teleportActionToTry.positionsToTeleport[i]);
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

        List<Node> possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(nodeToTry, currentChara.movementLeft, true);

        if (askForNextTurn)
        {
            possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(nodeToTry, 150, true);
        }

        float rangeNeeded = actionToTry.range;

        foreach (Node n in possibleDeplacement)
        {
            if (Pathfinding.instance.GetDistance(n, targetToTry.currentNode) <= rangeNeeded && askForNextTurn)
            {
                return true;
            }
            else
            {
                List<Node> touchableNodes = BattleManager.GetSpellUsableNodes(n, actionToTry);

                for (int i = 0; i < touchableNodes.Count; i++)
                {
                    List<Node> zoneNodes = BattleManager.GetHitNodes(targetToTry.currentNode.worldPosition, n.worldPosition, actionToTry);

                    if (zoneNodes.Contains(targetToTry.currentNode))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
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
    private bool HasVision(Node startNode, Node targetNode, float range)
    {
        if (Pathfinding.instance.GetDistance(startNode, targetNode) <= range)
        {
            return BattleManager.instance.IsNodeVisible(startNode, targetNode);
        }
        return false;
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
    public float EvaluateAction(AiConsideration actionToEvaluate, Node casterPosition, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        float totalResult = 0;
        float coef = 0;

        foreach (ValueForCalcul value in actionToEvaluate.calculs)
        {
            totalResult += ConsiderationCalcul(actionToEvaluate.wantedAction, value, casterPosition, caster, target, 1 + actionToEvaluate.maximumValueModifier) * value.calculImportance;
            coef += value.calculImportance;
        }

        totalResult = actionToEvaluate.startScore + totalResult / coef;
        if (totalResult > 1 + actionToEvaluate.maximumValueModifier)
        {
            totalResult = 1 + actionToEvaluate.maximumValueModifier;
        }



        return totalResult * (actionToEvaluate.considerationImportance + 1);
    }

    public float ConsiderationCalcul(CharacterActionScriptable spell, ValueForCalcul values, Node casterPosition, RuntimeBattleCharacter caster, RuntimeBattleCharacter target, float maxValue)
    {
        float result = 0;
        float abcsissa = 0;

        if (values.maxValue < 1)
        {
            values.maxValue = 1;
        }

        abcsissa = GetAbcsissaValue(spell, values.abscissaValue, casterPosition, target.currentNode, caster, target);

        float potentialResult = CalculateSpecifiedValues(values, abcsissa);

        if (potentialResult > result)
        {
            result = potentialResult;
        }

        result = Mathf.Clamp(result, 0, maxValue);

        return result;
    }

    private float CalculateSpecifiedValues(ValueForCalcul values, float abcsissa)
    {
        float result = 0;

        switch (values.calculType)
        {
            case AiCalculType.Conditionnal:
                result = ConditionnalCalcul(abcsissa, values.constant, values.coeficient);
                break;
            case AiCalculType.Affine:
                result = AffineCalcul(abcsissa, values.constant, values.coeficient) / values.maxValue; ;
                break;
            case AiCalculType.Logarythm:
                if (values.coeficient >= 1)
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
                result = LogisticalCalcul(abcsissa, values.constant, values.coeficient, values.checkAroundMax);
                break;
        }

        return result;
    }

    public float GetAbcsissaValue(CharacterActionScriptable spell, AiAbscissaType abcsissa, Node casterNode, Node targetNode, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        switch(abcsissa)
        {
            case AiAbscissaType.DistanceFromTarget:
                return Pathfinding.instance.GetDistance(casterNode, targetNode);
            case AiAbscissaType.TargetMaxHp:
                if (target != null)
                {
                    return target.GetCharacterDatas().GetMaxHps();
                }
                return 0;
            case AiAbscissaType.TargetCurrentHp:
                if (target != null)
                {
                    return target.GetCurrentHps();
                }
                return 0;
            case AiAbscissaType.TargetPercentHp:
                if (target != null)
                {
                    return target.GetPercentHp() * 100f;
                }
                return 0;
            case AiAbscissaType.CasterMaxHp:
                if (caster != null)
                {
                    return caster.GetCharacterDatas().GetMaxHps();
                }
                return 0;
            case AiAbscissaType.CasterCurrentHp:
                if (caster != null)
                {
                    return caster.GetCurrentHps();
                }
                return 0;
            case AiAbscissaType.CasterPercentHp:
                if (caster != null)
                {
                    return caster.GetPercentHp() * 100f;
                }
                return 0;
            case AiAbscissaType.TargetMalus:
                break;
            case AiAbscissaType.TargetBonus:
                break;
            case AiAbscissaType.TargetDangerosity:
                if (target != null)
                {
                    return target.GetDangerosity();
                }
                return 0;
            case AiAbscissaType.TargetVulnerability:
                if (target != null)
                {
                    return target.GetVulnerability();
                }
                return 0;
            case AiAbscissaType.TargetPhysicalArmor:
                if (target != null)
                {
                    return target.GetCharacterDatas().GetArmor();
                }
                return 0;
            case AiAbscissaType.NumberEnnemyArea:
                return GetCharacterInArea(casterNode, targetNode, caster.GetTeam(), spell, false);
            case AiAbscissaType.NumberAllyArea:
                return GetCharacterInArea(casterNode, targetNode, caster.GetTeam(), spell, true);
            case AiAbscissaType.NumberWoundedEnnemyArea:
                return GetCharacterInArea(casterNode, targetNode, caster.GetTeam(), spell, false, true);
            case AiAbscissaType.NumberWoundedAllyArea:
                return GetCharacterInArea(casterNode, targetNode, caster.GetTeam(), spell, true, true);
        }
        return 0;
    }

    private int GetCharacterInArea(Node casterNode, Node targetNode, int teamIndex, CharacterActionScriptable spell, bool isAlly, bool checkWounded = false)
    {
        List<Node> toTest = BattleManager.GetHitNodes(targetNode.worldPosition, casterNode.worldPosition, spell);
        int charaAmount = 0;
        for(int i = 0; i < toTest.Count; i++)
        {
            if(toTest[i].HasCharacterOn)
            {
                if(toTest[i].chara.GetTeam() == teamIndex && isAlly)
                {
                    if (!checkWounded || toTest[i].chara.GetPercentHp() < 1)
                    {
                        charaAmount++;
                    }
                }
                else if (toTest[i].chara.GetTeam() != teamIndex && !isAlly)
                {
                    if (!checkWounded || toTest[i].chara.GetPercentHp() < 1)
                    {
                        charaAmount++;
                    }
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

    public float LogisticalCalcul(float x, float k, float c, bool checkAroundMax)
    {
        if(checkAroundMax)
        {
            return 2 / (1 + (Mathf.Exp(Mathf.Abs(x - k) * c)));
        }
        return 1 / (1 + (Mathf.Exp((x-k) * c)));
    }
    #endregion
}
