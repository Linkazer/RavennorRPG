﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	public class NodeFeedback
    {
		public Node attachedNode;
		public SpriteRenderer usedSprite;

		public NodeFeedback(Node n, SpriteRenderer s)
        {
			attachedNode = n;
			usedSprite = s;
        }
    }

	public static Grid instance;

	public bool displayGridGizmos;
	public LayerMask unwalkableMask, visionMasks, characterMasks;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	Node[,] grid;

	public GameObject caseSprite;
	public Transform caseParent;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	[SerializeField]
	private List<NodeFeedback> usedNodeFeedback = new List<NodeFeedback>();
	[SerializeField]
	private List<SpriteRenderer> freeNodeFeedback;
	public GameObject parentFree;

	public int values;

	[ContextMenu("Set Nodes feedbacks Objects")]
	public void SetFreeNodes()
    {
		freeNodeFeedback.Clear();

		foreach(Transform child in parentFree.transform)
        {
			freeNodeFeedback.Add(child.gameObject.GetComponent<SpriteRenderer>());
        }
    }

	void Awake() {
		instance = this;
		nodeDiameter = nodeRadius*2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
		CreateGrid();

		foreach(SpriteRenderer g in freeNodeFeedback)
        {
			g.GetComponent<SpriteRenderer>().enabled = false;
        }
	}

	public int MaxSize {
		get {
			return gridSizeX * gridSizeY;
		}
	}

	public void CreateGrid() {
		if (grid == null)
		{
			grid = new Node[gridSizeX, gridSizeY];
		}
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.up * gridWorldSize.y/2;

		for (int x = 0; x < gridSizeX; x ++) {
			for (int y = 0; y < gridSizeY; y ++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics2D.OverlapCircle(worldPoint,nodeRadius*0.2f,unwalkableMask));
				bool blockVision = (!walkable && !(Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.2f, visionMasks)));
				RuntimeBattleCharacter newChara = null;
				if (Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.2f, characterMasks))
				{
					newChara = (Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.2f, characterMasks)).GetComponent<RuntimeBattleCharacter>();
				}

				if (grid[x, y] != null)
				{
					grid[x, y].SetNode(walkable, blockVision, newChara, worldPoint, x, y);
				}
				else
                {
					grid[x, y] = new Node(walkable, blockVision, newChara, worldPoint, x, y); ;
				}
			}
		}

		ResetNodeFeedback();
	}

	public Node GetNode(int x, int y)
    {
		return grid[x, y];
    }

	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX,checkY]);
				}
			}
		}

		return neighbours;
	}

	public Node GetRandomNeighbours(Node node)
    {
		List<Node> neighbours = GetNeighbours(node);
		return neighbours[Random.Range(0, neighbours.Count)];
    }
	
	public Node NodeFromWorldPoint(Vector3 worldPosition) {
		float percentX = (worldPosition.x + nodeRadius + gridWorldSize.x/2) / gridWorldSize.x;
		float percentY = (worldPosition.y + nodeRadius + gridWorldSize.y/2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX) * percentX)-1;
		int y = Mathf.RoundToInt((gridSizeY) * percentY)-1;

		return grid[x,y];
	}

	public Node GetRandomNodePosition()
	{
		return grid[Random.Range(0, gridSizeX), Random.Range(0, gridSizeY)];
	}
	
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,gridWorldSize.y,1));
		if (grid != null && displayGridGizmos) {
			foreach (Node n in grid) {
				bool redColor = n.walkable;
				Gizmos.color = redColor?Color.white:Color.red;
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter/2));
			}
		}
	}

	public void SetUsableNodes(List<Node> toUse, Color wantedColor)
    {
		ResetUsableNode();
		ResetNodeFeedback();
		foreach (Node n in toUse)
        {
			if (n.walkable)
			{
				n.usableNode = true;
				SetNodeFeedback(n.worldPosition, wantedColor, 6);
			}
        }
    }

	public void ShowZone(Node startNode, List<Vector2Int> wantedVectors, Color wantedColor)
    {
		List<Node> wantedNodes = GetZoneFromNode(startNode, wantedVectors);

		ResetNodeColor(wantedColor);

		foreach (Node n in wantedNodes)
        {
			SetNodeFeedback(n.worldPosition, wantedColor,7);
		}
    }

	public void HideZone()
    {
		ResetNodeColor(Color.red);
	}

	public void ShowZone(Vector2 startNode, List<Vector2Int> wantedVectors, Color wantedColor)
	{
		Node startN = NodeFromWorldPoint(startNode);
		ShowZone(startN, wantedVectors, wantedColor);
	}

	public void ShowZone(Vector3[] wantedPos, Color wantedColor)
    {
		ResetNodeColor(wantedColor);

		foreach (Vector3 v in wantedPos)
		{
			SetNodeFeedback(NodeFromWorldPoint(v).worldPosition, wantedColor, 7);
		}
	}

	public void ResetUsableNode()
    {
		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				grid[x, y].usableNode = false;
			}
		}

		ResetNodeFeedback();
	}

	public void SetAllUsableNodes()
    {
		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				grid[x, y].usableNode = true;
			}
		}

		ResetNodeFeedback();
	}

	public List<Node> GetZoneFromNode(Node startNode, List<Vector2Int> wantedVectors)
    {
		List<Node> toReturn = new List<Node>();
		int offSetX = startNode.gridX;
		int offSetY = startNode.gridY;
		foreach(Vector2Int vect in wantedVectors)
        {
			Node nextNode = grid[offSetX + vect.x, offSetY + vect.y];
			if (nextNode.walkable)
			{
				toReturn.Add(nextNode);
			}
		}
		return toReturn;
    }

	public List<Node> GetZoneFromPosition(Vector2 startPos, List<Vector2Int> wantedVectors)
	{
		Node startNode = NodeFromWorldPoint(startPos);
		List<Node> toReturn = new List<Node>();
		int offSetX = startNode.gridX;
		int offSetY = startNode.gridY;
		foreach (Vector2Int vect in wantedVectors)
		{
			Node nextNode = grid[offSetX + vect.x, offSetY + vect.y];
			if (nextNode.walkable)
			{
				toReturn.Add(nextNode);
			}
		}
		return toReturn;
	}

	public NodeFeedback SetNodeFeedback(Vector2 position, Color newColor, int layerIndex)
    {
		if (freeNodeFeedback.Count > 0)
		{
			freeNodeFeedback[0].transform.position = position;
			freeNodeFeedback[0].enabled = true;
			freeNodeFeedback[0].color = newColor;
			freeNodeFeedback[0].sortingOrder = layerIndex;

			NodeFeedback toAdd = new NodeFeedback(NodeFromWorldPoint(position), freeNodeFeedback[0]);

			usedNodeFeedback.Add(toAdd);
			freeNodeFeedback.RemoveAt(0);

			return toAdd;
		}
		return null;
    }


	public void ResetNodeColor(Color wantedColor)
	{
		for (int i = 0; i < usedNodeFeedback.Count; i++)
		{
			SpriteRenderer spriteRnd = usedNodeFeedback[i].usedSprite;
			if (spriteRnd.color == wantedColor)
			{
				spriteRnd.enabled = false;
				freeNodeFeedback.Add(spriteRnd);
				usedNodeFeedback.RemoveAt(i);
				i--;
			}
		}
	}

	public void ResetNodeFeedback(NodeFeedback toReset)
	{
		toReset.usedSprite.enabled = false;
		freeNodeFeedback.Add(toReset.usedSprite);
		usedNodeFeedback.Remove(toReset);
	}

	public void ResetNodeFeedback()
    {
		while(usedNodeFeedback.Count > 0)
		{
			ResetNodeFeedback(usedNodeFeedback[0]);
        }
    }

	/*public void ResetNodeFeedback(List<Node> toReset)
    {
		List<NodeFeedback> listToReset = new List<NodeFeedback>();

		for(int i = 0; i < usedNodeFeedback.Count; i++)
        {
			if(toReset.Contains(usedNodeFeedback[i].attachedNode))
            {
				listToReset.Add(usedNodeFeedback[i]);
            }
        }

		for(int i = 0; i < listToReset.Count; i++)
        {
			ResetNodeFeedback(listToReset[i]);
        }
    }


	public void ResetNodeFeedback(Node toReset)
    {
		for (int i = 0; i < usedNodeFeedback.Count; i++)
		{
			if (toReset == usedNodeFeedback[i].attachedNode)
			{
				ResetNodeFeedback(usedNodeFeedback[i]);
				i--;
			}
		}
	}*/
}