using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerBattleControllerManager : MonoBehaviour
{
    public static PlayerBattleControllerManager instance;

    private Vector2 lastMousePos = Vector2.zero, currentMousePos;

    [SerializeField]
    private LayerMask UILayer;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            BattleManager.instance.EndTurn();
        }

        if(Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
        {
            PlayerBattleManager.instance.NextAction(currentMousePos);
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            PlayerBattleManager.instance.ChooseSpell(0);
        }

        if (PlayerBattleManager.instance.holdSpellIndex >= 0 && Grid.instance.NodeFromWorldPoint(currentMousePos) != Grid.instance.NodeFromWorldPoint(lastMousePos) && Grid.instance.NodeFromWorldPoint(currentMousePos).usableNode)
        {
            lastMousePos = currentMousePos;
            PlayerBattleManager.instance.ShowCurrentSpell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            PlayerBattleManager.instance.ChooseSpell(-1);
        }

        if(Input.GetMouseButtonDown(1))
        {
            if(Grid.instance.NodeFromWorldPoint(currentMousePos).hasCharacterOn)
            {
                BattleUiManager.instance.ShowCharaInformation(Grid.instance.NodeFromWorldPoint(currentMousePos).chara);
            }
            else
            {
                BattleUiManager.instance.HideCharaInformation();
            }
        }
    }

    public void ChooseSpell(int index)
    {
        if(enabled)
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
        /*for(int i = 0; i < results.Count; i++)
        {
            if(results[i].sortingLayer != UILayer)
            {
                results.RemoveAt(i);
                i--;
            }
        }*/
        return results.Count > 0;
    }
}
