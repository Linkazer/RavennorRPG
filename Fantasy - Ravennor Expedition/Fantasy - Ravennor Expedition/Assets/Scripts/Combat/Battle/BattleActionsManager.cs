using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

public struct LaunchActionData
{
    public LaunchActionData(CharacterActionScriptable _wantedAction, RuntimeBattleCharacter _caster, Vector2 _positionWanted, bool _effectAction)
    {
        wantedAction = _wantedAction;
        caster = _caster;
        positionWanted = _positionWanted;
        effectAction = _effectAction;
    }

    public CharacterActionScriptable wantedAction;
    public RuntimeBattleCharacter caster;
    public Vector2 positionWanted;
    public bool effectAction;
}

public class BattleActionsManager : MonoBehaviour
{
    private static BattleActionsManager instance;

    [SerializeField] private RuntimeBattleCharacter passiveCheater;

    private LaunchActionData waitingActionData;

    private void Awake()
    {
        instance = this;
    }

    public static void SetCheater()
    {
        instance.passiveCheater.SetRuntimeCharacterData(instance.passiveCheater.GetCharacterDatas(), 10);
    }

    public static void MoveCharacter(RuntimeBattleCharacter character, Vector2 destination, bool isForNextTurn)
    {
        instance.AskToMove(character.gameObject, destination, character.movementLeft, isForNextTurn);
    }

    public static void EndCurrentAction(float delay)
    {
        instance.EndCurrentActionWithDelay(delay);
    }

    public void EndCurrentActionWithDelay(float timeDelay)
    {
        StartCoroutine(WaitForActionToEnd(timeDelay));
    }

    IEnumerator WaitForActionToEnd(float timeDelay)
    {
        if (timeDelay > 0)
        {
            yield return new WaitForSeconds(timeDelay);
        }
        EndCurrentAction(BattleManager.GetCurrentTurnChara);
    }


    public void EndCurrentAction(RuntimeBattleCharacter character)
    {
        character.SetAnimation("Default");
        character.ModifyCurrentNode(Grid.instance.NodeFromWorldPoint(character.transform.position));

        if (character.GetTeam() == 0)
        {
            // Mise à jour du Pathfinding autour du personnage
            Pathfinding.instance.SearchPath(Grid.instance.NodeFromWorldPoint(character.transform.position), null, false);
            PlayerBattleManager.instance.ActivatePlayerBattleController(true);
        }
        else if (character.GetTeam() > -1)
        {
            AiBattleManager.instance.SearchNextMove(0.5f);
        }
    }

    /*private void CancelCurrentAction()
    {
        if (currentWantedAction.incantationTime != ActionIncantation.Rapide)
        {
            currentCharacterTurn.ResetOneAction();
        }
        EndCurrentAction();
    }*/

    public static void LaunchAction(CharacterActionScriptable wantedAction, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction)
    {
        if (!effectAction)
        {
            caster.SetSpriteDirection((positionWanted.x < caster.transform.position.x));
        }
        if (effectAction || wantedAction.isWeaponBased)
        {
            instance.UseAction(wantedAction, caster, positionWanted, effectAction);
        }
        else
        {
            caster.SetAnimation("LaunchSpell");
            instance.waitingActionData = new LaunchActionData(wantedAction, caster, positionWanted, effectAction);
        }
    }

    public static void LaunchActionWithoutCaster(CharacterActionScriptable wantedAction, Vector2 positionWanted, bool effectAction)
    {
        instance.UseAction(wantedAction, instance.passiveCheater, positionWanted, effectAction);
    }

    public static void DoCurrentAction()
    {
        instance.DoAction((CharacterActionDirect)instance.waitingActionData.wantedAction, instance.waitingActionData.caster, instance.waitingActionData.positionWanted, instance.waitingActionData.effectAction);
    }

    public static void UseCurrentAction()
    {
       instance.UseAction(instance.waitingActionData.wantedAction, instance.waitingActionData.caster, instance.waitingActionData.positionWanted, instance.waitingActionData.effectAction);
    }

    public static void AskUseAction(CharacterActionScriptable wantedAction, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction)
    {
        instance.UseAction(wantedAction, caster, positionWanted, effectAction);
    }

    private void UseAction(CharacterActionScriptable wantedAction, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction)
    {
        if (!effectAction)
        {
            caster.useActionEvt?.Invoke();
            caster.ResolveEffect(EffectTrigger.DoAction);

            //diary.AddText(caster.name + " utilise " + wantedAction.nom + ".");
        }

        waitingActionData = new LaunchActionData(wantedAction, caster, positionWanted, effectAction);

        if (wantedAction.incantationTime != ActionIncantation.Rapide)
        {
            caster.UseAction(wantedAction.isWeaponBased);
        }

        switch (wantedAction.SpellType)
        {
            case SpellType.Direct:
                if (wantedAction.projectile != null)
                {
                    BattleAnimationManager.instance.PlayProjectile(caster.transform.position, positionWanted, wantedAction.projectile, wantedAction.speed);
                }
                else
                {
                    DoAction((CharacterActionDirect)wantedAction, caster, positionWanted, effectAction);
                }
                break;
            /*case SpellType.Invocation:
                SpellResolution.InvokeAlly(caster, (CharacterActionInvocation)wantedAction, positionWanted);
                break;
            case SpellType.Teleportation:
                SpellResolution.TeleportationSpell(caster, (CharacterActionTeleportation)wantedAction, positionWanted);
                break;*/
            case SpellType.SimpleEffect:
                break;
        }
    }

    public static List<Node> GetSpellUsableNodes(Node casterNode, CharacterActionScriptable spell)
    {
        List<Node> canSpellOn = Pathfinding.instance.GetNodesWithMaxDistance(casterNode, spell.range, false);

        if (spell.hasViewOnTarget)
        {
            for (int i = 1; i < canSpellOn.Count; i++)
            {
                if (!Grid.IsNodeVisible(canSpellOn[0], canSpellOn[i]))
                {
                    canSpellOn.RemoveAt(i);
                    i--;
                }
            }
        }
        if (!spell.castOnSelf)
        {
            canSpellOn.RemoveAt(0);
        }

        return canSpellOn;
    }

    public static List<Node> GetHitNodes(Vector2 position, Vector2 casterPosition, CharacterActionScriptable spell)
    {
        Vector2 direction = Vector2.one;
        if (spell.doesFaceCaster)
        {
            direction = casterPosition;
            direction = new Vector2(Grid.instance.NodeFromWorldPoint(position).gridX, Grid.instance.NodeFromWorldPoint(position).gridY) - direction;
        }
        List<Vector2Int> spellZone = new List<Vector2Int>();
        foreach (Vector2Int vect in spell.activeZoneCases)
        {
            if (direction.y == 0 && direction.x == 0)
            {
                spellZone.Add(new Vector2Int(vect.x, vect.y));
            }
            else if (direction.y > 0 && (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) || direction.x == direction.y))
            {
                spellZone.Add(new Vector2Int(vect.x, vect.y));
            }
            else if (direction.x < 0 && (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) || direction.x == -direction.y))
            {
                spellZone.Add(new Vector2Int(-vect.y, vect.x));
            }
            else if (direction.y < 0 && (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) || direction.x == direction.y))
            {
                spellZone.Add(new Vector2Int(-vect.x, -vect.y));
            }
            else
            {
                spellZone.Add(new Vector2Int(vect.y, -vect.x));
            }
        }

        return Grid.instance.GetZoneFromPosition(position, spellZone);
    }

    public void DoAction(CharacterActionDirect wantedAction, RuntimeBattleCharacter caster, Vector2 positionWanted, bool effectAction)
    {
        List<Node> hitNodes = GetHitNodes(positionWanted, new Vector2(caster.currentNode.gridX, caster.currentNode.gridY), wantedAction);

        // Prend en compte tous les ennemis et alliés de la map en fonction des targets voulut
        switch (wantedAction.target)
        {
            case ActionTargets.EveryAllies:
                hitNodes = new List<Node>();
                List<RuntimeBattleCharacter> allyTeam = BattleManager.GetAllyTeamCharacters(caster.GetTeam());

                for (int i = 0; i < allyTeam.Count; i++)
                {
                    hitNodes.Add(allyTeam[i].currentNode);
                }
                break;
            case ActionTargets.EveryEnnemies:
                hitNodes = new List<Node>();
                List<RuntimeBattleCharacter> ennemyTeam = BattleManager.GetEnemyTeamCharacters(caster.GetTeam());
                for (int i = 0; i < ennemyTeam.Count; i++)
                {
                    hitNodes.Add(ennemyTeam[i].currentNode);
                }
                break;
        }

        if (wantedAction.zoneSprite != null)
        {
            if (effectAction)
            {
                BattleAnimationManager.instance.PlayOnNode(positionWanted, wantedAction.zoneSprite, wantedAction.caseFeedback, -1, wantedAction.soundToPlay);
            }
            else
            {
                BattleAnimationManager.instance.PlayOnNode(positionWanted, wantedAction.zoneSprite, wantedAction.caseFeedback, 0.5f, wantedAction.soundToPlay);
            }
        }

        if (wantedAction.wantedEffectOnCaster.Count > 0)
        {
            foreach (SpellEffectScriptables eff in wantedAction.wantedEffectOnCaster)
            {
                SpellResolution.ApplyEffects(eff, caster, caster);
            }
        }

        List<Vector2> nodesPos = new List<Vector2>();

        foreach (Node n in hitNodes)
        {
            nodesPos.Add(n.worldPosition);

            if (n.HasCharacterOn && IsTargetAvailable(caster.GetTeam(), n.chara.GetTeam(), wantedAction.target, caster.GetInvocations().Contains(n.chara)))
            {
                SpellResolution.ResolveSpell(wantedAction, caster, n.chara, effectAction);
            }

            //Application d'effets sur le Sol
            if (wantedAction.wantedEffectOnGround.Count > 0)
            {
                foreach (SpellEffectScriptables eff in wantedAction.wantedEffectOnGround)
                {
                    RuntimeSpellEffect runEffet = new RuntimeSpellEffect(
                    eff.effet,
                    eff.duree,
                    caster
                    );

                    n.AddEffect(eff, runEffet, caster);

                    if (eff.spriteCase != null)
                    {
                        BattleAnimationManager.instance.AddZoneEffect(n.worldPosition, eff.spriteCase, caster, eff.duree, runEffet);
                    }
                }
            }
        }

        if (wantedAction.caseSprite != null)
        {
            if (effectAction)
            {
                BattleAnimationManager.instance.PlayOnNode(nodesPos, wantedAction.caseSprite, wantedAction.caseFeedback, -1, wantedAction.soundToPlay);
            }
            else
            {
                BattleAnimationManager.instance.PlayOnNode(nodesPos, wantedAction.caseSprite, wantedAction.caseFeedback, 0.5f, wantedAction.soundToPlay);
            }
        }

        if (!wantedAction.HadFeedback() && !effectAction)
        {
            EndCurrentActionWithDelay(0.2f);
        }
    }

    public static bool CheckActionAvailable(RuntimeBattleCharacter character, CharacterActionScriptable wantedAction)
    {
        return instance.IsActionAvailable(character, wantedAction);
    }

    public bool IsActionAvailable(RuntimeBattleCharacter character, CharacterActionScriptable wantedAction)
    {
        if (character.GetSpellCooldown(wantedAction) > 0 || character.HasSpellUtilisationLeft(wantedAction))
        {
            return false;
        }

        if (character.GetCurrentMaana() < wantedAction.maanaCost)
        {
            return false;
        }

        if ((wantedAction.attackType != AttackType.Magical && character.CheckForAffliction(Affliction.Atrophie)) || (wantedAction.attackType == AttackType.Magical && character.CheckForAffliction(Affliction.Silence)))
        {
            return false;
        }

        switch (wantedAction.incantationTime)
        {
            case ActionIncantation.Rapide:
                return true;
            case ActionIncantation.Simple:
                if (character.CanDoAction())
                {
                    return true;
                }
                return false;
            case ActionIncantation.Lent:
                if (character.CanDoAction() && character.CanMove)
                {
                    return true;
                }
                return false;
            case ActionIncantation.Hard:
                if (character.CanDoAction())
                {
                    return true;
                }
                return false;
        }
        return false;
    }

    public bool IsTargetAvailable(int casterTeam, int targetTeam, ActionTargets wantedTarget, bool isInvocation)
    {
        switch (wantedTarget)
        {
            case ActionTargets.SelfAllies:
                if (casterTeam == targetTeam)
                {
                    return true;
                }
                return false;
            case ActionTargets.Ennemies:
                if (casterTeam != targetTeam)
                {
                    return true;
                }
                return false;
            case ActionTargets.EveryAllies:
                if (casterTeam == targetTeam)
                {
                    return true;
                }
                return false;
            case ActionTargets.EveryEnnemies:
                if (casterTeam != targetTeam)
                {
                    return true;
                }
                return false;
            case ActionTargets.All:
                return true;
            case ActionTargets.Invocations:
                return isInvocation;
        }
        return false;
    }

    #region Déplacement

    private GameObject toMove;

    public float speed = 1;
    [HideInInspector]
    public Vector3[] path;
    private Vector3 lastPathPosition;
    int targetIndex = -1;

    Vector2 posUnit, posTarget, direction;

    public void AskToMove(GameObject objectToMove, Vector3 destination, int maxDistance, bool isForNextTurn)
    {
        toMove = objectToMove;
        int distance = Pathfinding.instance.GetDistance(Grid.instance.NodeFromWorldPoint(objectToMove.transform.position), Grid.instance.NodeFromWorldPoint(destination));
        BattleManager.GetCurrentTurnChara.movedEvt.Invoke(distance);
        PathRequestManager.RequestPath(objectToMove.transform.position, destination, maxDistance, isForNextTurn, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;
            BattleManager.GetCurrentTurnChara.SetAnimation("Moving");
            //Grid.instance.ResetUsableNode();
            Grid.instance.ResetNodeFeedback();
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
        else
        {
            Debug.Log(BattleManager.GetCurrentTurnChara + "No Path");
            if (BattleManager.GetCurrentTurnChara.GetTeam() != 0)
            {
                BattleManager.AskEndTurn();
            }
            else
            {
                EndCurrentAction(BattleManager.GetCurrentTurnChara);
            }
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 0)
        {
            RuntimeBattleCharacter currentCharacterTurn = BattleManager.GetCurrentTurnChara;

            Vector3 currentWaypoint = path[0];

            if (toMove == currentCharacterTurn.gameObject)
            {
                currentCharacterTurn.SetSpriteDirection((currentWaypoint.x < toMove.transform.position.x));
            }

            while (true)
            {
                posUnit = new Vector2(toMove.transform.position.x, toMove.transform.position.y);
                posTarget = new Vector2(currentWaypoint.x, currentWaypoint.y);
                if (Vector2.Distance(posUnit, posTarget) < (0.01f * speed))
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        Grid.instance.CreateGrid();
                        EndCurrentAction(currentCharacterTurn);
                        yield break;
                    }

                    if (toMove == currentCharacterTurn.gameObject)
                    {
                        currentCharacterTurn.SetSpriteDirection((path[targetIndex].x < toMove.transform.position.x));
                        currentCharacterTurn.ModifyCurrentNode(Grid.instance.NodeFromWorldPoint(currentWaypoint));
                    }

                    currentWaypoint = path[targetIndex];
                    lastPathPosition = currentWaypoint;
                }
                direction = new Vector2(currentWaypoint.x - toMove.transform.position.x, currentWaypoint.y - toMove.transform.position.y).normalized;

                toMove.transform.position += new Vector3(direction.x, direction.y, 0) * speed * Time.deltaTime;

                yield return null;
            }
        }
    }
    #endregion
}
