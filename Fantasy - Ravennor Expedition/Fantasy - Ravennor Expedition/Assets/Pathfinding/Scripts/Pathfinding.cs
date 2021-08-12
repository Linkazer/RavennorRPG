﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pathfinding : MonoBehaviour {
	
	PathRequestManager requestManager;
	Grid grid;
	public static Pathfinding instance;
	
	void Awake() {
		instance = this;
		requestManager = GetComponent<PathRequestManager>();
		grid = GetComponent<Grid>();
	}
	
	
	public void StartFindPath(Vector3 startPos, Vector3 targetPos, int maxDistance, bool isForNextTurn) {
		StartCoroutine(FindPath(startPos,targetPos, maxDistance, isForNextTurn));
	}
	
	IEnumerator FindPath(Vector3 startPos, Vector3 targetPos, int maxDistance, bool isForNextTurn) {

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;
		
		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);

		if (targetNode.walkable) {
			pathSuccess = SearchPath(startNode, targetNode, isForNextTurn);
		}
		yield return null;
		if (pathSuccess) {
			waypoints = RetracePath(startNode,targetNode, maxDistance);
		}
		if(waypoints.Length <= 0 || waypoints[0] == null)
        {
			pathSuccess = false;
        }
		requestManager.FinishedProcessingPath(waypoints,pathSuccess);
		
	}
	
	Vector3[] RetracePath(Node startNode, Node endNode, int maxDistance) {

		List<Node> path = GetPath(startNode, endNode, maxDistance);
		Vector3[] waypoints = GetVectorPath(path);
		Array.Reverse(waypoints);
		return waypoints;
		
	}
	
	Vector3[] GetVectorPath(List<Node> path) {
		List<Vector3> waypoints = new List<Vector3>();
		
		for (int i = 0; i < path.Count; i ++) {
				waypoints.Add(path[i].worldPosition);
		}
		return waypoints.ToArray();
	}

	public void SetAllNodes(Node startNode, float distance)
    {
		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while (openSet.Count > 0)
		{
			Node currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);

			foreach (Node neighbour in grid.GetNeighbours(currentNode))
			{
				if ((!neighbour.walkable || neighbour.HasCharacterOn) || closedSet.Contains(neighbour))
				{
					continue;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour <= distance)
				{
					if (neighbour.HasCharacterOn)
					{
						newMovementCostToNeighbour += 50;
						neighbour.gCost = newMovementCostToNeighbour;
					}

					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.parent = currentNode;

						if (!openSet.Contains(neighbour))
						{
							openSet.Add(neighbour);
						}
					}
				}
			}
		}
	}

	public bool SearchPath(Node startNode, Node targetNode, bool isForNextTurn)
	{
		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while (openSet.Count > 0)
		{
			Node currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);

			foreach (Node neighbour in grid.GetNeighbours(currentNode))
			{
				if ((!neighbour.walkable || (!isForNextTurn && neighbour.HasCharacterOn && neighbour != targetNode)) || closedSet.Contains(neighbour))
				{
					continue;
				}

				if (targetNode != null && neighbour == targetNode)
				{
					neighbour.gCost = currentNode.gCost + GetDistance(currentNode, neighbour);
					neighbour.parent = currentNode;
					return true;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if(neighbour.HasCharacterOn)
                {
					newMovementCostToNeighbour += 50;
					if(neighbour.gCost < 50)
                    {
						neighbour.gCost = newMovementCostToNeighbour;
					}
				}

				if (newMovementCostToNeighbour <= neighbour.gCost || (isForNextTurn && !openSet.Contains(neighbour)))
				{
					neighbour.gCost = newMovementCostToNeighbour;
					if (targetNode != null)
					{
						neighbour.hCost = GetDistance(neighbour, targetNode);
					}
					neighbour.parent = currentNode;

					if (!openSet.Contains(neighbour))
					{
						openSet.Add(neighbour);
					}
				}
			}
		}
		return false;
	}

	public List<Node> GetPath(Node startNode, Node endNode, int maxDistance)
	{
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		int i = 0;
		while ((currentNode.gCost > maxDistance || currentNode.HasCharacterOn) && i < 100)
        {
			i++;
			if (currentNode.parent != null)
			{
				currentNode = currentNode.parent;
			}
			else
            {
				break;
            }
			//Debug.Log(currentNode.gCost + " + " + currentNode.hCost + " > " + maxDistance);
		}

		while (currentNode != startNode)
		{
			if(currentNode.parent == null)
            {
				path = new List<Node>();
				break;
			}
			else if (currentNode.HasCharacterOn && currentNode != endNode)
			{
				path = new List<Node>();
			}
			else
			{
				path.Add(currentNode);
			}
			currentNode = currentNode.parent;
		}

		return path;
	}
	
	public int GetDistance(Node nodeA, Node nodeB) {
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
		
		if (dstX > dstY)
			return 15*dstY + 10* (dstX-dstY);
		return 15*dstX + 10 * (dstY-dstX);
	}

	public List<Node> GetNodesWithMaxDistance(Node startNode, float distance, bool pathCalcul)
	{
		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		List<Node> usableNode = new List<Node>();
		int fullSet = grid.MaxSize;
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);
		usableNode.Add(startNode);

		while (openSet.currentItemCount > 0)
		{
			Node currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);

			foreach (Node neighbour in grid.GetNeighbours(currentNode))
			{
				if (((!neighbour.walkable || neighbour.HasCharacterOn) && pathCalcul) || closedSet.Contains(neighbour))
				{
					continue;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour <= distance)
				{
					if (neighbour.HasCharacterOn && pathCalcul)
					{
						newMovementCostToNeighbour += 50;
						neighbour.gCost = newMovementCostToNeighbour;
					}

					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.parent = currentNode;

						if (!openSet.Contains(neighbour))
						{
							openSet.Add(neighbour);
							usableNode.Add(neighbour);
						}
					}
				}

/*				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newMovementCostToNeighbour;

					neighbour.parent = currentNode;

					if ((GetDistance(startNode, neighbour) <= distance && !pathCalcul) || (neighbour.fCost <= distance && pathCalcul))
					{
						openSet.Add(neighbour);
						usableNode.Add(neighbour);
						fullSet--;
					}
				}*/
			}
		}

		return usableNode;
	}
}
