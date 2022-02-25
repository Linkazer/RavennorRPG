using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerBattleControllerManager : MonoBehaviour
{
    public static PlayerBattleControllerManager instance;

    public Camera usedCamera;

    private Vector2 lastMousePos = Vector2.zero, currentMousePos;
    private Node currentMouseNode;

    [SerializeField]
    private LayerMask UILayer;

    [SerializeField]
    private MenuManager gameMenu;

    private bool isPlayerTurn;

    public bool IsPlayerTurn => isPlayerTurn;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        currentMousePos = usedCamera.ScreenToWorldPoint(Input.mousePosition);
        currentMouseNode = Grid.instance.NodeFromWorldPoint(currentMousePos);

        #region Global Input
        if (Input.GetMouseButtonDown(1))
        {
            if (currentMouseNode.HasCharacterOn)
            {
                BattleUiManager.instance.ShowCharaInformation(currentMouseNode.chara);
            }
            else
            {
                BattleUiManager.instance.HideCharaInformation();
            }
        }
        #endregion

        #region Player turn Inputs
        if (isPlayerTurn)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndPlayerTurn();
                return;
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                PlayerBattleManager.instance.NextAction(currentMousePos);
            }

            if (!currentMouseNode.usableNode)
            {
                //Grid.instance.HideZone();
                PlayerBattleManager.instance.HidePath();
            }
            else
            {
                if (PlayerBattleManager.instance.holdSpellIndex >= 0)
                {
                    PlayerBattleManager.instance.ShowCurrentSpell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }
                else if(Grid.instance.NodeFromWorldPoint(currentMousePos).usableNode)
                {
                    PlayerBattleManager.instance.ShowPath(currentMousePos);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                PlayerBattleManager.instance.ChooseSpell(-1);
            }

            lastMousePos = currentMousePos;
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameMenu.OpenMenu();
        }
    }

    public void ChooseSpell(int index)
    {
        if(isPlayerTurn)
        {
            PlayerBattleManager.instance.ChooseSpell(index);
        }
    }

    public CharacterActionScriptable GetSpell(int index)
    {
        return PlayerBattleManager.instance.GetSpell(index);
    }

    public bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public void SetPlayerTurn(bool state)
    {
        if(!enabled)
        {
            enabled = true;
        }
        BattleUiManager.instance.SetPlayerUI(state);
        isPlayerTurn = state;
    }

    public void EndPlayerTurn()
    {
        BattleManager.instance.EndTurn();
        PlayerBattleManager.instance.ChooseSpell(-1);
    }

}
