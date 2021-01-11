using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorObject : MonoBehaviour
{
    [SerializeField]
    private int index;

    private Node currentNode;

    private void Start()
    {
        currentNode = Grid.instance.NodeFromWorldPoint(transform.position);
    }

    private void OnMouseDown()
    {
        RuntimeBattleCharacter chara = BattleManager.instance.GetCurrentTurnChara();

        if (chara.actionAvailable)
        {
            if (Pathfinding.instance.GetDistance(chara.currentNode, currentNode) < 15)
            {
                chara.actionAvailable = false;
                StartCoroutine(OpenDoor());
            }
        }
    }

    IEnumerator OpenDoor()
    {
        Debug.Log("Ouverture porte");
        BattleManager.instance.OpenRoom(index);
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        PlayerBattleManager.instance.ActivatePlayerBattleController(false);
        yield return new WaitForSeconds(0.5f);
        //Grid.instance.ResetUsableNode();
        Grid.instance.CreateGrid();
        PlayerBattleManager.instance.ActivatePlayerBattleController(true);
        PlayerBattleManager.instance.ShowDeplacement();
        gameObject.SetActive(false);

    }
}
