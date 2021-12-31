using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class RVN_InteractableObject : MonoBehaviour
{
    [SerializeField] private List<Vector2Int> usedPositions;

    protected List<Node> currentNode = new List<Node>();

    [SerializeField] private UnityEvent PlayOnInteract;

    protected abstract bool OnInteract(RuntimeBattleCharacter interactedCharacter);

    private void Start()
    {
        for (int i = 0; i < usedPositions.Count; i++)
        {
            Vector3 newPos = new Vector2(0, 0.08f) + new Vector2(0.16f, 0.16f) * usedPositions[i];
            currentNode.Add(Grid.instance.NodeFromWorldPoint(transform.position + newPos));
        }
    }

    private void OnMouseDown()
    {
        RuntimeBattleCharacter chara = BattleManager.instance.GetCurrentTurnChara();

        for (int i = 0; i < currentNode.Count; i++)
        {
            if (Pathfinding.instance.GetDistance(chara.currentNode, currentNode[i]) < 15)
            {
                if(OnInteract(chara))
                {
                    PlayOnInteract?.Invoke();
                }
            }
        }
    }
}
