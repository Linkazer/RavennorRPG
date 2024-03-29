﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleManager : MonoBehaviour
{
    public static PlayerBattleManager instance;

    [SerializeField]
    private PlayerBattleControllerManager controler;

    [SerializeField]
    private RuntimeBattleCharacter currentCharacter;

    [SerializeField]
    private List<CharacterActionScriptable> actionList;

    public int holdSpellIndex;

    public CharacterActionScriptable testAction;

    [SerializeField]
    private LayerMask layerMaskObstacle;
    private RaycastHit2D hit;

    private Vector2 actionPosition;

    private bool isOvercharging;

    [SerializeField] private Color moveColor;
    [SerializeField] private Color pathColor;
    [SerializeField] private Color pathOpportunityColor;
    [SerializeField] private Color spellColor;
    [SerializeField] private Color spellZoneColor;

    private void Start()
    {
        instance = this;
    }

    public void NewPlayerTurn(RuntimeBattleCharacter nextChara)
    {
        BattleUiManager.instance.SetNewCharacter(nextChara);

        currentCharacter = nextChara;

        isOvercharging = false;

        UpdateActionList(false);

        ActivatePlayerBattleController(true);
    }

    public void UpdateActionList(bool isOvercharged)
    {
        isOvercharging = isOvercharged;
        actionList = currentCharacter.GetActions(isOvercharged);
    }

    public void NextAction(Vector2 position)
    {
        if (Grid.instance.NodeFromWorldPoint(position).usableNode)
        {
            if (holdSpellIndex >= 0)
            {
                if (currentCharacter.HasEnoughMaana(actionList[holdSpellIndex].maanaCost))
                {
                    actionPosition = Grid.instance.NodeFromWorldPoint(position).worldPosition;
                    UseSpell(0);
                }
                else
                {
                    ShowDeplacement();
                    ActivatePlayerBattleController(true);
                }
            }
            else
            {
                MoveCharacter(position);
            }
        }
        else if (holdSpellIndex >= 0)
        {
            ChooseSpell(-1);
        }
    }

    public void ChooseSpell(int index)
    {
        //Grid.instance.ResetUsableNode();
        if (index < actionList.Count && index != holdSpellIndex && index >= 0 && (isOvercharging || BattleManager.instance.IsActionAvailable(currentCharacter, actionList[index])))
        {
            ShowSpell(actionList[index]);
            holdSpellIndex = index;
        }
        else
        {
            if (controler.IsPlayerTurn)
            {
                ShowDeplacement();
            }
            holdSpellIndex = -1;
        }
    }

    public CharacterActionScriptable GetSpell(int index)
    {
        return actionList[index];
    }

    [ContextMenu("ShowCurrentSpell")]
    public void TestSpell()
    {
        ShowSpell(testAction);
    }

    public void ShowSpell(CharacterActionScriptable wantedAction)
    {
        Grid.instance.CreateGrid();

        List<Node> canSpellOn = BattleManager.GetSpellUsableNodes(currentCharacter.currentNode, wantedAction);

        Grid.instance.SetUsableNodes(canSpellOn, spellColor);
    }

    List<Grid.NodeFeedback> displayedPath = new List<Grid.NodeFeedback>();

    public void ShowPath(Vector2 mousePos)
    {
        PathRequestManager.RequestPath(currentCharacter.currentNode.worldPosition, mousePos, currentCharacter.movementLeft, false, DisplayPath);
    }

    public void HidePath()
    {
        while(displayedPath.Count > 0)
        {
            Grid.instance.ResetNodeFeedback(displayedPath[0]);
            displayedPath.RemoveAt(0);
        }

        /*Grid.instance.ResetNodeColor(pathColor);
        Grid.instance.ResetNodeColor(pathOpportunityColor);*/
    }

    private void DisplayPath(Vector3[] newPath, bool pathSuccessful)
    {
        HidePath();

        if (newPath.Length > 0)
        {

            displayedPath = new List<Grid.NodeFeedback>();

            if (BattleManager.instance.CheckForOpportunityAttack(currentCharacter.currentNode.worldPosition, newPath[0], currentCharacter).Count > 0)
            {
                displayedPath.Add(Grid.instance.SetNodeFeedback(newPath[0], pathOpportunityColor, 7));
            }
            else
            {
                displayedPath.Add(Grid.instance.SetNodeFeedback(newPath[0], pathColor, 7));
            }

            for (int i = 1; i < newPath.Length; i++)
            {
                if (BattleManager.instance.CheckForOpportunityAttack(newPath[i - 1], newPath[i], currentCharacter).Count > 0)
                {
                    displayedPath.Add(Grid.instance.SetNodeFeedback(newPath[i], pathOpportunityColor, 7));
                }
                else
                {
                    displayedPath.Add(Grid.instance.SetNodeFeedback(newPath[i], pathColor, 7));
                }
            }
            
        }
    }

    public void ShowCurrentSpell(Vector2 mousePos)
    {
        CharacterActionScriptable wantedAction = actionList[holdSpellIndex];

        Vector2 direction = Vector2.one;
        if (wantedAction.doesFaceCaster)
        {
            direction = new Vector2(currentCharacter.currentNode.gridX, currentCharacter.currentNode.gridY);
            direction = new Vector2(Grid.instance.NodeFromWorldPoint(mousePos).gridX, Grid.instance.NodeFromWorldPoint(mousePos).gridY) - direction;
        }
        List<Vector2Int> spellZone = new List<Vector2Int>();
        foreach(Vector2Int vect in wantedAction.activeZoneCases)
        {
            if(direction.y == 0 && direction.x == 0)
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
        
        Grid.instance.ShowZone(mousePos, spellZone, spellZoneColor);
    }

    public bool CanAskSpell()
    {
        return !BattleUiManager.instance.IsAskingSpell();
    }

    public void UseSpell(int maanaSpent)
    {
        ActivatePlayerBattleController(false);

        currentCharacter.UseMaana(actionList[holdSpellIndex].maanaCost + maanaSpent);

        currentCharacter.SetCooldown(actionList[holdSpellIndex]);

        currentCharacter.UseSpell(actionList[holdSpellIndex]);

        BattleManager.instance.LaunchAction(actionList[holdSpellIndex], maanaSpent, currentCharacter, actionPosition, false);

        holdSpellIndex = -1;
        BattleUiManager.instance.SetOvercharge(false);
    }

    public void ShowDeplacement()
    {
        StartCoroutine(WaitToShowDeplacement());
    }

    public void MoveCharacter(Vector2 newPosition)
    {
        if (Grid.instance.NodeFromWorldPoint(newPosition).usableNode)
        {
            ActivatePlayerBattleController(false);
            BattleManager.instance.MoveCharacter(currentCharacter, newPosition, false);
            currentCharacter.movementLeft -= Grid.instance.NodeFromWorldPoint(newPosition).gCost;
        }
    }

    public void ActivatePlayerBattleController(bool state)
    {
        controler.SetPlayerTurn(state);
        if(state)
        {
            ShowDeplacement();
            BattleUiManager.instance.UpdateSpells();
        }
        else
        {
            Grid.instance.ResetNodeFeedback();
        }
    }

    IEnumerator WaitToShowDeplacement()
    {
        yield return new WaitForSeconds(Time.deltaTime);
        Grid.instance.CreateGrid();
        List<Node> canMoveTo = Pathfinding.instance.GetNodesWithMaxDistance(currentCharacter.currentNode, currentCharacter.movementLeft, true);
        canMoveTo.RemoveAt(0);
        Grid.instance.SetUsableNodes(canMoveTo, moveColor);
    }
}
