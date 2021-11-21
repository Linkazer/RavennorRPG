﻿using System.Collections;
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
	int heapIndex;

	//public CaseGameObj nodeAnim;

	public bool usableNode;

	private List<RuntimeSpellEffect> effectsOnNode = new List<RuntimeSpellEffect>();
	private List<RuntimeBattleCharacter> casterList = new List<RuntimeBattleCharacter>();
	private List<SpellEffectScriptables> effectsScriptables = new List<SpellEffectScriptables>();

	public bool HasCharacterOn => chara != null && chara.IsAlive;

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
		heapIndex = 0;
	}

	public void EnterNode(RuntimeBattleCharacter newChara)
	{
		chara = newChara;
		for (int i = 0; i < effectsOnNode.Count; i++)
		{
			Debug.Log("Enter in effect");
			BattleManager.instance.ApplyEffects(effectsScriptables[i], 0, casterList[i], chara);
			BattleManager.instance.ResolveEffect(effectsOnNode[i].effet, worldPosition, worldPosition, EffectTrigger.EnterNode, 1);
		}
	}

	public void ExitNode(RuntimeBattleCharacter charaToExit)
	{
		for (int i = 0; i < effectsOnNode.Count; i++)
		{
			Debug.Log("Exit in effect");
			BattleManager.instance.ResolveEffect(effectsOnNode[i].effet, worldPosition, worldPosition, EffectTrigger.ExitNode, 1);
			charaToExit.RemoveEffect(effectsOnNode[i].effet);
		}
	}

	public void AddEffect(SpellEffectScriptables eff, RuntimeBattleCharacter caster)
    {
		effectsScriptables.Add(eff);

		RuntimeSpellEffect runEffet = new RuntimeSpellEffect(
		eff.effet,
		0,
		eff.duree,
		caster
		);

		effectsOnNode.Add(runEffet);
		casterList.Add(caster);

		Debug.Log(eff.effet.nom);
		Debug.Log(caster);
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
