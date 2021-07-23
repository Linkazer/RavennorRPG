using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node : IHeapItem<Node> {

	public bool walkable;//, HasCharacterOn;
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

	public bool HasCharacterOn => chara != null && chara.GetCurrentHps() > 0;

	public Node(bool _walkable, bool _hasCharacter, RuntimeBattleCharacter newChara, Vector3 _worldPos, int _gridX, int _gridY) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
		//hasCharacterOn = _hasCharacter;
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
				Debug.Log("Update Effect time");
				if (effectsOnNode[i].currentCooldown >= 0)
				{
					effectsOnNode[i].currentCooldown--;

					if (effectsOnNode[i].currentCooldown <= 0)
					{
						effectsOnNode.RemoveAt(i);
						casterList.RemoveAt(i);
						i--;
					}
				}
			}

			if (HasCharacterOn && BattleManager.instance.GetCurrentTurnChara() == chara)
			{
				Debug.Log("New Effect application");
				BattleManager.instance.ResolveEffect(effectsOnNode[i].effet, worldPosition, worldPosition, EffectTrigger.BeginTurn, 1);
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
