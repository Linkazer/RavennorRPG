using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node : IHeapItem<Node> {
	
	public bool walkable, hasCharacterOn;
	public RuntimeBattleCharacter chara;
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;

	public int gCost;
	public int hCost;
	public Node parent;
	int heapIndex;

	//public CaseGameObj nodeAnim;

	public bool usableNode;

	private List<RuntimeSpellEffect> effectsOnNode = new List<RuntimeSpellEffect>();
	private List<RuntimeBattleCharacter> casterList = new List<RuntimeBattleCharacter>();
	
	public Node()
    {

    }

	public Node(bool _walkable, bool _hasCharacter, RuntimeBattleCharacter newChara, Vector3 _worldPos, int _gridX, int _gridY) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
		hasCharacterOn = _hasCharacter;
		chara = newChara;
		//nodeAnim = newCase.GetComponent<CaseGameObj>();

		BattleManager.TurnBeginEvent.AddListener(UpdateNode);
	}

	public void AddEffect(RuntimeSpellEffect eff, RuntimeBattleCharacter caster)
    {
		effectsOnNode.Add(eff);
		casterList.Add(caster);
    }

	public void UpdateNode()
    {
		for(int i = 0; i < effectsOnNode.Count;i++)
        {
			if (BattleManager.instance.GetCurrentTurnChara() == casterList[i])
			{
				Debug.Log("New Effect application");

				if (hasCharacterOn)
				{
					BattleManager.instance.ResolveEffect(effectsOnNode[i].effet, worldPosition, EffectTrigger.BeginTurn);
				}

				effectsOnNode[i].currentCooldown--;

				if (effectsOnNode[i].currentCooldown <= 0)
				{
					effectsOnNode.RemoveAt(i);
					casterList.RemoveAt(i);
					i--;
				}
			}
        }
    }

	public int fCost {
		get {
			return gCost + hCost;
		}
	}

	public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}

	public int CompareTo(Node nodeToCompare) {
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}
