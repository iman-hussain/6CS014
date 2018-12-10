using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public enum Scenario {
	unknown
}

public enum Agents { agent1, agent2, agent3, master };


public class Grid : MonoBehaviour {
	public static Grid instance;
	public bool displayGridGizmos;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public TerrainType[] walkableRegions;
	Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
	LayerMask walkableMask;

	public Scenario scenario;
	public Agents agents;
	Dictionary<int, Node[,]> agentGrid = new Dictionary<int, Node[,]>();
	Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	void Awake() {
		instance = this;
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		foreach (TerrainType region in walkableRegions) {
			walkableMask.value |= region.terrainMask.value;
			walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
		}

		CreateGrid();
	}



	public Node[,] GetGridFromID(int id) {
		return agentGrid[id];
	}

	public void RegisterAgent(int id) {
		Node[,] nArray = new Node[gridSizeX, gridSizeY];
		CreateGrid(ref nArray);
		agentGrid.Add(id, nArray);
	}

	public int MaxSize {
		get {
			return gridSizeX * gridSizeY;
		}
	}

	public int GridSizeX {
		get {
			return gridSizeX;
		}

		set {
			gridSizeX = value;
		}
	}

	public int GridSizeY {
		get {
			return gridSizeY;
		}

		set {
			gridSizeY = value;
		}
	}

	public void CreateGrid() {
		CreateGrid(ref grid);
	}



	void CreateGrid(ref Node[,] grid) {
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

				bool discovered = true;

				if (scenario == Scenario.unknown) {
					walkable = false;
					discovered = false;
				}

				int movementPenalty = 0;

				//if (walkable)
				//{
				Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 100, walkableMask)) {
					walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
				}
				//}

				grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty, discovered);
			}
		}
	}
	/*
	public exploreGrid()
	{
	    //Scan the bounds of the model and create a grid
	    Node bounds = renderer.bounds;
	    //Work out the top left corner
	    topLeftCorner = bounds.center - bounds.extents + new Vector3(0, bounds.size.y, 0);
	    //Calculate the dimensions of the grid map
	    width = Mathf.RoundToInt(bounds.size.x / cellSize);
	    height = Mathf.RoundToInt(bounds.size.z / cellSize);

	    //Scan for walkable terrain in each cell
	    for (int x = 0; x < gridSizeX; x++)
	    {
		for (int y = 0; y < gridSizeY; y++)
		{

		    //Get the position for a ray
		    var currentPosition = topLeftCorner + new Vector3(x * cellSize, 0, y * cellSize);
		    RaycastHit hit;

		    //Create a cell for the grid
		    var cell = new GridCell();
		    cells[x, y] = cell;

		    //Cast the ray
		    if (Physics.Raycast(currentPosition, -Vector3.up, out hit, bounds.size.y))
		    {

			//The height of the highest item in the cell
			cell.height = hit.point.y;

			//Test if the thing we hit was walkable
			if (((1 << hit.collider.gameObject.layer) & walkableLayer) != 0)
			{
			    //Flag the cell as walkable
			    cell.walkable = true;
			}
		    }

		}
	    }
	}
	*/
	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	public List<Node> GetNeighbours(int id, Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(agentGrid[id][checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	public Node NodeFromXY(int x, int y) {
		return grid[x, y];
	}

	public void DiscoveredNode(int id, int x, int y) {
		agentGrid[id][x, y].discovered = true;
		agentGrid[id][x, y].walkable = !(Physics.CheckSphere(grid[x, y].worldPosition, nodeRadius, unwalkableMask));

		grid[x, y].discovered = true;
		grid[x, y].walkable = !(Physics.CheckSphere(grid[x, y].worldPosition, nodeRadius, unwalkableMask));

	}

	public void DiscoveredNode(int id, int x, int y, bool _walkable) {
		agentGrid[id][x, y].discovered = true;
		agentGrid[id][x, y].walkable = _walkable;

		grid[x, y].discovered = true;
		grid[x, y].walkable = _walkable;

	}

	public void DiscoveredNodes(int id, Node node) {
		List<Node> neighbours = GetNeighbours(node);
		foreach (Node n in neighbours) {
			DiscoveredNode(id, n.gridX, n.gridY);
		}
	}


	public Node NodeFromWorldPoint(Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid[x, y];
	}

	public Node NodeFromWorldPoint(int id, Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return agentGrid[id][x, y];
	}

	void OnDrawGizmos() {

		Node[,] drawGrid = grid;
		if (agentGrid.Count > 0) {
			List<int> list = new List<int>(agentGrid.Keys);

			if (agents == Agents.agent1) {
				drawGrid = agentGrid[list[0]];
			}
			else if (agents == Agents.agent2) {
				drawGrid = agentGrid[list[1]];
			}
			else if (agents == Agents.agent3) {
				drawGrid = agentGrid[list[2]];
			}
		}

		Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
		if (drawGrid != null && displayGridGizmos) {
			foreach (Node n in drawGrid) {
				Gizmos.color = (n.walkable) ? Color.white : Color.red;


				Gizmos.color = (n.discovered) ? Color.green : Color.grey;
				if (n.discovered) {
					if (agents == Agents.agent1) {
						Gizmos.color = new Color(208,32,144,1f);
					}
					else if (agents == Agents.agent2) {
						Gizmos.color = new Color(0, 0, 255, 1f);
					}
					else if (agents == Agents.agent3) {
						Gizmos.color = Color.blue;
					}
				}


				if (n.walkable == false && n.discovered) {
					Gizmos.color = Color.red;
				}

				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
			}
		}
	}

	[System.Serializable]
	public class TerrainType {
		public LayerMask terrainMask;
		public int terrainPenalty;
	}


}