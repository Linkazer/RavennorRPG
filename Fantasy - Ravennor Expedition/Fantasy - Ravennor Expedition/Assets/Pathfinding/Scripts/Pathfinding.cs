using UnityEngine;
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
			pathSuccess = SetAllNodes(startNode, targetNode, isForNextTurn);
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

	/*public void SetAllNodes(Node startNode)
    {
		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		int fullSet = grid.MaxSize;
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while (fullSet > 0 && openSet.Count>0)
		{
			Node currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);

			foreach (Node neighbour in grid.GetNeighbours(currentNode))
			{
				if ((!neighbour.walkable || neighbour.hasCharacterOn) || closedSet.Contains(neighbour))
				{
					continue;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newMovementCostToNeighbour;
					//neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;

					if (!openSet.Contains(neighbour))
					{
						openSet.Add(neighbour);
						fullSet--;
					}
				}
			}
		}
	}*/

	public bool SetAllNodes(Node startNode, Node targetNode, bool isForNextTurn)
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
				if ((!neighbour.walkable || (!isForNextTurn && neighbour != targetNode && neighbour.HasCharacterOn)) || closedSet.Contains(neighbour))
				{
					continue;
				}

				if (targetNode != null && neighbour == targetNode)
				{
					neighbour.gCost = currentNode.gCost + GetDistance(currentNode, neighbour);
					neighbour.parent = currentNode;
					Debug.Log(currentNode);
					return true;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
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
			if (currentNode.HasCharacterOn && currentNode != endNode)
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

		bool gotAllNodes = true;

		while (gotAllNodes)
		{
			if (openSet.currentItemCount > 0)
			{
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);

				foreach (Node neighbour in grid.GetNeighbours(currentNode))
				{
					if (((!neighbour.walkable || neighbour.HasCharacterOn) && pathCalcul) || closedSet.Contains(neighbour))
					{
						continue;
					}

					//neighbour.parent = currentNode;

					int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newMovementCostToNeighbour;

						neighbour.parent = currentNode;

						if ((GetDistance(startNode, neighbour) <= distance && !pathCalcul) || (neighbour.fCost <= distance && pathCalcul))
						{
							openSet.Add(neighbour);
							usableNode.Add(neighbour);
							fullSet--;
						}
					}

					/*if (openSet.currentItemCount < 1)
					{
						gotAllNodes = false;
						break;
					}*/
				}
			}
			else
            {
				gotAllNodes = false;
			}
		}

		return usableNode;
	}
	
	public IEnumerator TestAffichageGetDistance (Node startNode, float distance, bool pathCalcul)
    {
		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		List<Node> usableNode = new List<Node>();
		int fullSet = grid.MaxSize;
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);
		usableNode.Add(startNode);

		bool gotAllNodes = true;

		while (gotAllNodes)
		{
			Node currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);

			foreach (Node neighbour in grid.GetNeighbours(currentNode))
			{
				if ((!neighbour.walkable || (neighbour.HasCharacterOn && pathCalcul)) || closedSet.Contains(neighbour))
				{
					continue;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || (GetDistance(startNode, neighbour) < distance && !pathCalcul) || (neighbour.fCost < distance && pathCalcul))
				{
					if (!openSet.Contains(neighbour) && GetDistance(startNode, neighbour) < distance)
					{
						openSet.Add(neighbour);
						usableNode.Add(neighbour);
						fullSet--;
					}
				}

				if (GetDistance(startNode, neighbour) > distance)
				{
					gotAllNodes = false;
					break;
				}
			}
		}

		foreach (Node n in usableNode)
        {
			//n.nodeAnim.SetFeedbackColor(Color.red);
			yield return new WaitForSeconds(0.2f);
		}
	}
}
