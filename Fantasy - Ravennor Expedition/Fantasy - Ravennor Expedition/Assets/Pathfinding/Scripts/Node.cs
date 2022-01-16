using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node : IHeapItem<Node> {

	public bool walkable;//, HasCharacterOn;
	public bool blockVision;
	public RuntimeBattleCharacter chara;
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;

	public int gCost;
	public int hCost;
	public Node parent;
	public Node children;
	int heapIndex;

	//public CaseGameObj nodeAnim;

	public bool usableNode;

	private List<RuntimeSpellEffect> effectsOnNode = new List<RuntimeSpellEffect>();
	private List<RuntimeBattleCharacter> casterList = new List<RuntimeBattleCharacter>();
	private List<SpellEffectScriptables> effectsScriptables = new List<SpellEffectScriptables>();

	public bool HasCharacterOn => chara != null && chara.IsAlive;

	public bool HasSameEffect(RuntimeSpellEffect effectToCheck, RuntimeBattleCharacter casterToCheck)
	{
		for (int i = 0; i < effectsOnNode.Count; i++)
		{
			if (effectToCheck.effet.nom == effectsOnNode[i].effet.nom && casterList[i] == casterToCheck)
			{
				return true;
			}
		}
		return false;
	}

	public Node(bool _walkable, bool _blockVision, RuntimeBattleCharacter newChara, Vector3 _worldPos, int _gridX, int _gridY) {

		SetNode(_walkable, _blockVision, newChara, _worldPos, _gridX, _gridY);
		//BattleManager.TurnBeginEvent += UpdateNode;
	}

	public void SetNode(bool _walkable, bool _blockVision, RuntimeBattleCharacter newChara, Vector3 _worldPos, int _gridX, int _gridY)
	{
		walkable = _walkable;
		blockVision = _blockVision;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
		chara = newChara;

		gCost = 0;
		hCost = 0;
		parent = null;
		children = null;
		heapIndex = 0;
	}

	public void EnterNode(RuntimeBattleCharacter newChara, Node previousNode)
	{
		chara = newChara;
		for (int i = 0; i < effectsOnNode.Count; i++)
		{
			if (!previousNode.HasSameEffect(effectsOnNode[i], casterList[i]) || !newChara.ContainsEffect(effectsOnNode[i].effet))
			{
				Debug.Log("Enter in effect");
				BattleManager.instance.ApplyEffects(effectsScriptables[i], 0, casterList[i], newChara);
				newChara.ResolveSpecifiedEffect(effectsOnNode[i], EffectTrigger.EnterNode);
			}
		}
	}

	public void ExitNode(RuntimeBattleCharacter charaToExit, Node nextNode)
	{
		for (int i = 0; i < effectsOnNode.Count; i++)
		{
			if (!nextNode.HasSameEffect(effectsOnNode[i], casterList[i]))
			{
				Debug.Log("Exit in effect");
				charaToExit.ResolveSpecifiedEffect(effectsOnNode[i], EffectTrigger.ExitNode);
				charaToExit.RemoveEffect(effectsOnNode[i].effet);
			}
		}
	}

	public void AddEffect(SpellEffectScriptables eff, RuntimeSpellEffect runEffet, RuntimeBattleCharacter caster)
    {
		effectsScriptables.Add(eff);

		effectsOnNode.Add(runEffet);
		casterList.Add(caster);
	}

	public void UpdateNode(RuntimeBattleCharacter charaTurn)
    {
		for(int i = 0; i < effectsOnNode.Count;i++)
        {
			if (charaTurn == casterList[i])
			{
				Debug.Log("Update Effect time");
				if (effectsOnNode[i].currentCooldown >= 0)
				{
					effectsOnNode[i].currentCooldown--;

					if (effectsOnNode[i].currentCooldown <= 0)
					{
						effectsOnNode.RemoveAt(i);
						casterList.RemoveAt(i);
						effectsScriptables.RemoveAt(i);
						i--;
					}
				}
			}

			if (HasCharacterOn && BattleManager.instance.GetCurrentTurnChara() == chara)
			{
				chara.ResolveSpecifiedEffect(effectsOnNode[i], EffectTrigger.BeginTurn);
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
