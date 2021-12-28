using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiBattleManager : MonoBehaviour
{
    public static AiBattleManager instance;

    [SerializeField]
    private RuntimeBattleCharacter currentChara;
    private Node target;
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

        SearchForBestAction(currentChara, BattleManager.GetAllChara, false);

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
        RuntimeBattleCharacter targetCharacter = null;
        if (target != null)
        {
            targetCharacter = target.chara;
        }

        if (currentChara.GetCurrentHps() > 0)
        {
            if (wantedAction != null && currentChara.CanDoAction())
            {
                //Debug.Log("AI do something");
                hit = Physics2D.Raycast(currentChara.currentNode.worldPosition, (target.worldPosition - currentChara.currentNode.worldPosition).normalized, Vector2.Distance(target.worldPosition, currentChara.currentNode.worldPosition), layerMaskObstacle);

                if (nodeToMoveTo != null && currentChara.currentNode.worldPosition != nodeToMoveTo.worldPosition)
                {
                    //Debug.Log(currentChara + " move for action : " + nodeToMoveTo.worldPosition.ToString("F4"));
                    BattleActionsManager.MoveCharacter(currentChara, nodeToMoveTo.worldPosition, false);
                    currentChara.movementLeft -= nodeToMoveTo.gCost;
                }
                else if ((Pathfinding.instance.GetDistance(currentChara.currentNode, target) <= wantedAction.range))
                {
                    currentChara.SetCooldown(wantedAction);
                    BattleActionsManager.LaunchAction(wantedAction, currentChara, target.worldPosition, false);
                    if (currentChara.CanDoAction())
                    {
                        //Debug.Log("Can do action");
                        wantedAction = null;
                        SearchForBestAction(currentChara, BattleManager.GetAllChara, false);
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

                if (target != null && targetCharacter != null && (targetCharacter.GetCurrentHps() > 0 && Pathfinding.instance.GetDistance(target, currentChara.currentNode) <= wantedDist))
                {
                    Debug.Log("End Turn with Objective");
                    BattleManager.instance.EndTurn();
                }
                else
                {
                    currentChara.UseAllAction();
                    SearchForBestAction(currentChara, BattleManager.GetAllChara, true);

                    //Debug.Log(currentChara.name + " Move for next round : " + nodeToMoveTo.worldPosition);

                    if (nodeToMoveTo != currentChara.currentNode && nodeToMoveTo != null)
                    {
                        //Debug.Log(" Movement Left Moving : " + nodeToMoveTo.worldPosition + " != " + currentChara.currentNode.worldPosition);
                        BattleActionsManager.MoveCharacter(currentChara, nodeToMoveTo.worldPosition, true);
                        currentChara.movementLeft -= nodeToMoveTo.gCost;
                    }
                    else
                    {
                        nodeToMoveTo = GetClosestTargetNode(BattleManager.GetPlayerChara);
                        //Debug.Log("From : " + currentChara.currentNode.worldPosition + " to : " + nodeToMoveTo.worldPosition);
                        if (nodeToMoveTo != null && Pathfinding.instance.GetDistance(nodeToMoveTo, currentChara.currentNode) <= wantedDist)
                        {
                            Debug.Log("End Turn with Objective close");
                            BattleManager.instance.EndTurn();
                        }
                        else if (nodeToMoveTo != currentChara.currentNode && nodeToMoveTo != null)
                        {
                            BattleActionsManager.MoveCharacter(currentChara, nodeToMoveTo.worldPosition, true);
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
            List<Node> possiblesActionsTargets = new List<Node>();

            int considCount = 0;

            foreach (AiConsideration consid in aiCaster.comportement)
            {
                considCount++;
                if ((consid.cooldown <= 0 && caster.GetSpellCooldown(consid.wantedAction) <= 0) || (askForNextTurn && consid.cooldown <= 1 && caster.GetSpellCooldown(consid.wantedAction) <= 1))
                {
                    List<Node> possibleMovement = new List<Node>();
                    possibleMovement.Add(caster.currentNode);
                    if (consid.optimizePosition)
                    {
                        possibleMovement = new List<Node>(Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, currentChara.movementLeft, true));
                    }
                    for (int i = 0; i < possibleMovement.Count; i++)
                    {
                        Node nodeToTry = possibleMovement[i];

                        List<Node> targetNodes = new List<Node>();
                        if(consid.wantedAction.castTarget == ActionTargets.All)
                        {
                            targetNodes = BattleActionsManager.GetSpellUsableNodes(nodeToTry, consid.wantedAction);
                        }
                        else
                        {
                            for(int j = 0; j < targets.Count; j++)
                            {
                                targetNodes.Add(targets[j].currentNode);
                            }
                        }

                        for(int k = 0; k < targetNodes.Count; k++)
                        {
                            Node targetNodeToTry = targetNodes[k];
                            RuntimeBattleCharacter targetCharacter = targetNodeToTry.chara;

                            if (CanSpellBeUsed(consid, nodeToTry, consid.wantedAction, targetNodeToTry, askForNextTurn, consid.optimizePosition))
                            {
                                float newScore = EvaluateAction(consid, nodeToTry, targetNodeToTry, caster, targetCharacter);
                                //Debug.Log(caster + " try " + consid.wantedAction + " on " + targetCharacter + " Score : " + newScore + "(Consideration Nb : " + considCount);
                                if (newScore >= maxScore)
                                {
                                    if (newScore > maxScore)
                                    {
                                        possiblesActions = new List<CharacterActionScriptable>();
                                        possiblesActionsMoveNeeded = new List<Node>();
                                        possiblesActionsTargets = new List<Node>();
                                    }

                                    if (askForNextTurn)
                                    {
                                        nodeToMoveTo = GetNodeToHitTarget(targetNodeToTry, consid.wantedAction.range, consid.wantedAction.hasViewOnTarget, 1000);
                                    }
                                    else if (nodeToTry != caster.currentNode)
                                    {
                                        nodeToMoveTo = nodeToTry;
                                    }
                                    else
                                    {
                                        nodeToMoveTo = GetNodeToHitTarget(targetNodeToTry, consid.wantedAction.range, consid.wantedAction.hasViewOnTarget, 0);
                                    }
                                    maxScore = newScore;
                                    wantedAction = consid.wantedAction;
                                    target = targetNodeToTry;
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

    private bool CanSpellBeUsed(AiConsideration consid, Node nodeToTry, CharacterActionScriptable actionToTry, Node targetNode, bool askForNextTurn, bool optimizedPath)
    {
        RuntimeBattleCharacter potentialTarget = targetNode.chara;

        if (potentialTarget != null)
        {
            switch (actionToTry.castTarget)
            {
                case ActionTargets.Ennemies:
                    if (potentialTarget.GetTeam() != 0)
                    {
                        return false;
                    }
                    break;
                case ActionTargets.SelfAllies:
                    if (potentialTarget.GetTeam() == 0)
                    {
                        return false;
                    }
                    break;
            }
        }

        if(!askForNextTurn && !Grid.IsNodeVisible(nodeToTry, targetNode))
        {
            return false;
        }

        //Condition de l'IA

        for (int i = 0; i < consid.conditions.Count; i++)
        {
            ValueForCondition condition = consid.conditions[i];

            float absice = GetAbcsissaValue(consid.wantedAction, condition.conditionWanted, nodeToTry, targetNode, currentChara, potentialTarget);

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

        if (potentialTarget != null && potentialTarget.GetCurrentHps() <= 0)
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
                Vector2Int positionToTry = new Vector2Int(targetNode.gridX, targetNode.gridY);
                Vector3 wolrdPosToTry = Grid.GetTargetPosWithFacingPosition(currentChara.currentNode.worldPosition, positionToTry, teleportActionToTry.positionsToTeleport[i]);
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

        List<Node> possibleDeplacement = new List<Node>();

        if(!optimizedPath)
        {
            possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, currentChara.movementLeft, true);
        }
        if (askForNextTurn)
        {
            possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, 150, true);
        }

        float rangeNeeded = actionToTry.range;

        bool foundSomething = false;

        foreach (Node n in possibleDeplacement)
        {
            if (Pathfinding.instance.GetDistance(n, targetNode) <= rangeNeeded && (askForNextTurn || Grid.IsNodeVisible(n, targetNode)))
            {
                return true;
            }
            else
            {
                List<Node> touchableNodes = BattleActionsManager.GetSpellUsableNodes(n, actionToTry);

                for (int i = 0; i < touchableNodes.Count; i++)
                {
                    List<Node> zoneNodes = BattleActionsManager.GetHitNodes(targetNode.worldPosition, n.worldPosition, actionToTry);

                    if (zoneNodes.Contains(targetNode))
                    {
                        return true;
                    }
                }
            }
        }

        return foundSomething;
    }

    private Node GetNodeToHitTarget(Node wantedTargetNode, float rangeWanted, bool needView, int deplacementBoost)
    {
        Node nodeToReturn = currentChara.currentNode;

        List<Node> possibleDeplacement = Pathfinding.instance.GetNodesWithMaxDistance(currentChara.currentNode, currentChara.movementLeft + deplacementBoost, true);

        float currentDistance = 1000;

        foreach (Node n in possibleDeplacement)
        {
            //if (Pathfinding.instance.GetDistance(n, wantedTargetNode) <= rangeWanted)
            {
                bool hasView = Grid.IsNodeVisible(n, wantedTargetNode);

                if (hasView || !needView)
                {
                    if (Pathfinding.instance.GetDistance(wantedTargetNode, n) < currentDistance)
                    {
                        nodeToReturn = n;
                        currentDistance = Pathfinding.instance.GetDistance(wantedTargetNode, n);
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
    public float EvaluateAction(AiConsideration actionToEvaluate, Node nodeToTry, Node targetNode, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        float totalResult = 0;
        float coef = 0;

        foreach(ValueForCalcul value in actionToEvaluate.calculs)
        {
            totalResult += ConsiderationCalcul(actionToEvaluate.wantedAction, value, nodeToTry, targetNode, caster, target, 1 + actionToEvaluate.maximumValueModifier) * value.calculImportance;
            coef += value.calculImportance;
        }

        totalResult = actionToEvaluate.startScore + totalResult / coef;
        if (totalResult > 1 + actionToEvaluate.maximumValueModifier)
        {
            totalResult = 1 + actionToEvaluate.maximumValueModifier;
        }

        return totalResult * (actionToEvaluate.considerationImportance + 1);
    }

    public float ConsiderationCalcul(CharacterActionScriptable spell, ValueForCalcul values, Node nodeToTry, Node targetNode, RuntimeBattleCharacter caster, RuntimeBattleCharacter target, float maxValue)
    {
        float result = 0;
        float abcsissa = GetAbcsissaValue(spell, values.abscissaValue, nodeToTry, targetNode, caster, target);

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
                result = LogisticalCalcul(abcsissa, values.constant, values.coeficient, values.checkAroundMax);
                break;
        }

        result = Mathf.Clamp(result, 0, maxValue);

        return result;
    }

    public float GetAbcsissaValue(CharacterActionScriptable spell, AiAbscissaType abcsissa, Node nodeToTry, Node targetNode, RuntimeBattleCharacter caster, RuntimeBattleCharacter target)
    {
        switch(abcsissa)
        {
            case AiAbscissaType.DistanceFromTarget:
                return Pathfinding.instance.GetDistance(nodeToTry, targetNode);
            case AiAbscissaType.TargetMaxHp:
                if(target == null)
                {
                    return 0;
                }
                return target.GetCharacterDatas().GetMaxHps();
            case AiAbscissaType.TargetCurrentHp:
                if (target == null)
                {
                    return 0;
                }
                return target.GetCurrentHps();
            case AiAbscissaType.TargetPercentHp:
                if (target == null)
                {
                    return 0;
                }
                return target.GetPercentHp() * 100f;
            case AiAbscissaType.CasterMaxHp:
                if (caster == null)
                {
                    return 0;
                }
                return caster.GetCharacterDatas().GetMaxHps();
            case AiAbscissaType.CasterCurrentHp:
                if (caster == null)
                {
                    return 0;
                }
                return caster.GetCurrentHps();
            case AiAbscissaType.CasterPercentHp:
                if (caster == null)
                {
                    return 0;
                }
                return caster.GetPercentHp();
            case AiAbscissaType.TargetMalus:
                break;
            case AiAbscissaType.TargetBonus:
                break;
            case AiAbscissaType.TargetDangerosity:
                if (target == null)
                {
                    return 0;
                }
                return target.GetDangerosity();
            case AiAbscissaType.TargetVulnerability:
                if (target == null)
                {
                    return 0;
                }
                return target.GetVulnerability();
            case AiAbscissaType.TargetPhysicalArmor:
                if (target == null)
                {
                    return 0;
                }
                return target.GetCharacterDatas().GetArmor();
            case AiAbscissaType.NumberEnnemyArea:
                return GetCharacterInArea(caster, targetNode, spell, false);
            case AiAbscissaType.NumberAllyArea:
                return GetCharacterInArea(caster, targetNode, spell, true);
            case AiAbscissaType.NumberWoundedEnnemyArea:
                return GetCharacterInArea(caster, targetNode, spell, false, true);
            case AiAbscissaType.NumberWoundedAllyArea:
                return GetCharacterInArea(caster, targetNode, spell, true, true);
        }
        return 0;
    }

    private int GetCharacterInArea(RuntimeBattleCharacter caster, Node targetNode, CharacterActionScriptable spell, bool isAlly, bool checkWounded = false)
    {
        List<Node> toTest = BattleActionsManager.GetHitNodes(targetNode.worldPosition, caster.currentNode.worldPosition, spell);
        int charaAmount = 0;
        for(int i = 0; i < toTest.Count; i++)
        {
            if(toTest[i].HasCharacterOn)
            {
                if(toTest[i].chara.GetTeam() == caster.GetTeam() && isAlly)
                {
                    if (!checkWounded || toTest[i].chara.GetPercentHp() < 1)
                    {
                        charaAmount++;
                    }
                }
                else if (toTest[i].chara.GetTeam() != caster.GetTeam() && !isAlly)
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
