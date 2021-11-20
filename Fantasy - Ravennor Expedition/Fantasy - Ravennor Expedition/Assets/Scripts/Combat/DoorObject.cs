using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorObject : MonoBehaviour
{
    [SerializeField]
    private List<int> indexs;

    [SerializeField] private List<Vector2Int> usedPositions;

    private List<Node> currentNode = new List<Node>();

    [SerializeField]
    private Animator anim;

    private void Start()
    {
        for(int i = 0; i < usedPositions.Count; i++)
        {
            Vector3 newPos = new Vector2(0, 0.08f) + new Vector2(0.16f, 0.16f) * usedPositions[i];
            currentNode.Add(Grid.instance.NodeFromWorldPoint(transform.position + newPos));
        }
    }

    private void OnMouseDown()
    {
        RuntimeBattleCharacter chara = BattleManager.instance.GetCurrentTurnChara();

        for(int i = 0; i < currentNode.Count; i++)
        {
            Debug.Log(chara.currentNode.worldPosition.ToString("F4"));
            Debug.Log(currentNode[i].worldPosition.ToString("F4"));
            if (Pathfinding.instance.GetDistance(chara.currentNode, currentNode[i]) < 15)
            {
                anim.Play("UseObject");
                //chara.UseAction(true);
                foreach (int r in indexs)
                {
                    BattleManager.instance.OpenRoom(r);
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
