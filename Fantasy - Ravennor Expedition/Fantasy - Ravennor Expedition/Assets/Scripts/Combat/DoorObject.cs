using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorObject : MonoBehaviour
{
    [SerializeField]
    private List<int> indexs;

    private Node currentNode;

    [SerializeField]
    private Animator anim;

    private void Start()
    {
        currentNode = Grid.instance.NodeFromWorldPoint(transform.position);
    }

    private void OnMouseDown()
    {
        RuntimeBattleCharacter chara = BattleManager.instance.GetCurrentTurnChara();

        if (chara.CanDoAction())
        {
            Node n = Grid.instance.NodeFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (Pathfinding.instance.GetDistance(chara.currentNode, n) < 15)
            {
                anim.Play("UseObject");
                chara.UseAction(true);
                foreach (int i in indexs)
                {
                    BattleManager.instance.OpenRoom(i);
                }
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
                PlayerBattleManager.instance.ActivatePlayerBattleController(false);
            }
        }
    }

    public void DestroyDoor()
    {
        Grid.instance.CreateGrid();
        PlayerBattleManager.instance.ActivatePlayerBattleController(true);
        PlayerBattleManager.instance.ShowDeplacement();
        gameObject.SetActive(false);
    }
}
