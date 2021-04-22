using System.Collections;
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

    private void Start()
    {
        instance = this;
    }

    public void NewPlayerTurn(RuntimeBattleCharacter nextChara)
    {
        BattleUiManager.instance.SetNewCharacter(nextChara);

        currentCharacter = nextChara;

        actionList = currentCharacter.GetActions();

        ActivatePlayerBattleController(true);
    }

    public void EndPlayerTurn()
    {
        if (controler.enabled)
        {
            BattleManager.instance.EndTurn();
        }
    }

    public void NextAction(Vector2 position)
    {
        if (Grid.instance.NodeFromWorldPoint(position).usableNode)
        {
            ActivatePlayerBattleController(false);
            if (holdSpellIndex >= 0)
            {
                if (currentCharacter.HasEnoughMaana(actionList[holdSpellIndex].maanaCost))
                {
                    if (actionList[holdSpellIndex].incantationTime != ActionIncantation.Rapide)
                    {
                        currentCharacter.UseAction(actionList[holdSpellIndex].isWeaponBased);
                    }
                    UseSpell(Grid.instance.NodeFromWorldPoint(position).worldPosition);
                }
                else
                {
                    ShowDeplacement();
                    ActivatePlayerBattleController(true);
                }
                holdSpellIndex = -1;
            }
            else// if (Pathfinding.instance.GetDistance(Grid.instance.NodeFromWorldPoint(currentCharacter.transform.position), Grid.instance.NodeFromWorldPoint(position)) < currentCharacter.movementLeft)
            {
                MoveCharacter(position);
            }
        }
        else if (holdSpellIndex >= 0)
        {
            holdSpellIndex = -1;
            ShowDeplacement();
        }
    }

    public void ChooseSpell(int index)
    {
        //Grid.instance.ResetUsableNode();
        if (index < actionList.Count && index != holdSpellIndex && index >= 0 && currentCharacter.CanDoAction(actionList[index].isWeaponBased))
        {
            if (BattleManager.instance.IsActionAvailable(currentCharacter, actionList[index]))
            {
                ShowSpell(actionList[index]);
                holdSpellIndex = index;
            }
        }
        else
        {
            ShowDeplacement();
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
        List<Node> canSpellOn = Pathfinding.instance.GetNodesWithMaxDistance(Grid.instance.NodeFromWorldPoint(currentCharacter.transform.position), wantedAction.range.y, false);
        if (wantedAction.hasViewOnTarget)
        {
            for (int i = 1; i < canSpellOn.Count; i++)
            {

                if (!IsNodeVisible(canSpellOn[0], canSpellOn[i]))
                {
                    canSpellOn.RemoveAt(i);
                    i--;
                }
            }
        }

        Grid.instance.SetUsableNodes(canSpellOn, Color.blue);
    }

    private bool IsNodeVisible(Node startNode, Node targetNode)
    {
        /*int i = 0;
        Node checkNode = targetNode;

        while(i < 10 && checkNode != startNode)
        {
            i++;
            if (checkNode != null)
            {
                if (!checkNode.walkable)
                {
                    Debug.Log("Not walkable");
                    return false;
                }
                checkNode = checkNode.parent;
            }
        }
        return true;*/

        #region Tentative sans parent
        int x = targetNode.gridX;
        int y = targetNode.gridY;
        int j = y;

        float realJ = y;

        int diffX = targetNode.gridX - startNode.gridX;
        int diffY = targetNode.gridY - startNode.gridY;

        float absX = Mathf.Abs((float)diffX);
        float absY = Mathf.Abs((float)diffY);

        int xCoef = 1;
        int yCoef = 1;

        if (diffX < 0)
        {
            xCoef = -1;
        }
        else if(diffX == 0)
        {
            xCoef = 0;
        }

        if (diffY < 0)
        {
            yCoef = -1;
        }
        else if (diffY == 0)
        {
            yCoef = 0;
        }

        if (absX == absY)
        {
            for (int i = x; i != startNode.gridX; i-=xCoef)
            {
                if(!Grid.instance.GetNode(i, j).walkable)
                {
                    return false;
                }

                realJ -= yCoef;
                j = Mathf.RoundToInt(realJ);
            }
        }
        else if(absX > absY)
        {
            for (int i = x; i != startNode.gridX; i -= xCoef)
            { 
                if (!Grid.instance.GetNode(i, j).walkable)
                {
                    return false;
                }

                if (yCoef != 0 && diffY != 0)
                {
                    realJ -= (absY / absX) * (float)yCoef;
                    j = Mathf.RoundToInt(realJ);
                }
            }
        }
        else if(absX < absY)
        {
            realJ = x;
            j = x;
            for (int i = y; i != startNode.gridY; i -= yCoef)
            {
                if (!Grid.instance.GetNode(j, i).walkable)
                {
                    return false;
                }

                if (xCoef != 0 && diffX != 0)
                {
                    realJ -= (absX / absY) * (float)xCoef;
                    j = Mathf.RoundToInt(realJ);
                }
            }
        }

        return true;
        #endregion
    }

    public void ShowCurrentSpell(Vector2 mousePos)
    {
        CharacterActionScriptable wantedAction = actionList[holdSpellIndex];
        //ShowSpell(wantedAction);

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
            else //if (direction.x > 0 && (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) || direction.x == -direction.y))
            {
                spellZone.Add(new Vector2Int(vect.y, -vect.x));
            }
            /*else
            {
                Debug.Log("J'ai merdé");
            }*/
        }
        Grid.instance.ShowZone(mousePos, spellZone, Color.red);
    }

    public void UseSpell(Vector2 position)
    {
        currentCharacter.UseMaana(actionList[holdSpellIndex].maanaCost);

        currentCharacter.SetCooldown(actionList[holdSpellIndex]);

        currentCharacter.UseSpell(actionList[holdSpellIndex]);

        BattleManager.instance.LaunchAction(actionList[holdSpellIndex], currentCharacter, position, false);
    }

    public void ShowDeplacement()
    {
        StartCoroutine(WaitToShowDeplacement());
    }

    public void MoveCharacter(Vector2 newPosition)
    {
        if (Grid.instance.NodeFromWorldPoint(newPosition).usableNode)
        {
            BattleManager.instance.MoveCharacter(currentCharacter, newPosition);
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
        Grid.instance.SetUsableNodes(canMoveTo, Color.green);
    }
}
