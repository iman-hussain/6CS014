using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public enum Movements {
	random, ordered
};

public class Unit : MonoBehaviour {


	public Movements movements;
	public float speed = 20;
	Vector3[] path;
	int targetIndex;
	Node myNode;
	Node targetNode;
	Vector3 targetPosition;
	bool walkLeft = false;
	bool searching = true;
	bool avoided = false;
	int yChangedCount = 0;
	int xChangedCount = 0;
	Vector3 treePos;
	Vector3 millPos;
	bool foundTree = false;
	bool foundMill = false;
	bool treeCut = false;

	public float xMax;
	public float zMax;
	public float xMin;
	public float zMin;

	private float x;
	private float z;

	private float angle;
	private float time;
	int action = 0;


	//Pathfinding pathfinder = new Pathfinding();
	PathRequestManager prm = new PathRequestManager();
	int origY = 0;
	int previousX = 0, previousY = 0;


	public Rigidbody rb;
	int myId;
	Stopwatch sw = new Stopwatch();


	void Start() {
		if (movements == Movements.ordered) {


			sw.Start();
			UnityEngine.Debug.Log("my id is " + this.GetInstanceID());
			myId = this.gameObject.GetInstanceID();

			rb = GetComponent<Rigidbody>();
			Grid.instance.RegisterAgent(myId);

			myNode = Grid.instance.NodeFromWorldPoint(transform.position);
			targetNode = Grid.instance.NodeFromXY(myNode.gridX + 1, myNode.gridY);
			targetPosition = targetNode.worldPosition;
			targetPosition.y = transform.position.y;
			Grid.instance.DiscoveredNodes(myId, myNode);
		}
		else {

			sw.Start();
			UnityEngine.Debug.Log("my id is " + this.GetInstanceID());
			myId = this.gameObject.GetInstanceID();
			rb = GetComponent<Rigidbody>();
			Grid.instance.RegisterAgent(myId);

			myNode = Grid.instance.NodeFromWorldPoint(transform.position);
			Grid.instance.DiscoveredNodes(myId, myNode);
			x = Random.Range(-speed, speed);
			z = Random.Range(-speed, speed);
			angle = Mathf.Atan2(x, z) * (180 / 3.141592f) + 90;
			transform.localRotation = Quaternion.Euler(0, angle, 0);
		}

	}

	private void Update() {
		switch (action) {
			case 1:
				action = 0;
				UnityEngine.Debug.Log("Chopping");
				break;
			case 2:

				StartCoroutine(Wait());
				UnityEngine.Debug.Log("transmitting and waiting");
				action = 0;
				break;
			case 3:
				UnityEngine.Debug.Log("Delivering information");
				break;
			case 4:
				UnityEngine.Debug.Log("assisting the other");
				break;
			default:
				//UnityEngine.Debug.Log("Searching");



				if (searching) {
					if (movements == Movements.ordered) {

						if (Vector3.Distance(transform.position, targetPosition) > 0.1f) {

							transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
						}
						else {
							if (avoided) {
								myNode = Grid.instance.NodeFromWorldPoint(transform.position);

								//myNode = targetNode;

								if (yChangedCount == 0) {
									//Debug.Log("here 1");
									avoided = false;
									xChangedCount = 0;
									return;
								}

								if (myNode.gridX == targetNode.gridX) {
									if (avoided && (((myNode.gridX > previousX) && walkLeft == false) || ((myNode.gridX < previousX) && walkLeft == true))) {
										xChangedCount += 1;
									}
								}
								//Debug.Log("here 2");
								CheckNeighbourInDirection(myNode.gridX, myNode.gridY + 1);
								if (Grid.instance.NodeFromXY(myNode.gridX, myNode.gridY + 1).walkable) {
									if (xChangedCount > 0) {
										targetNode = Grid.instance.NodeFromXY(myNode.gridX, myNode.gridY + 1);
										targetPosition = targetNode.worldPosition;
										targetPosition.y = transform.position.y;
										yChangedCount -= 1;
										//Debug.Log("here 3");
										return;
									}
								}
								CheckNeighbourInDirection((walkLeft) ? myNode.gridX - 1 : myNode.gridX + 2, myNode.gridY);
								if (Grid.instance.NodeFromXY((walkLeft) ? myNode.gridX - 1 : myNode.gridX + 2, myNode.gridY).walkable) {
									targetNode = Grid.instance.NodeFromXY((walkLeft) ? myNode.gridX - 1 : myNode.gridX + 1, myNode.gridY);
									targetPosition = targetNode.worldPosition;
									targetPosition.y = transform.position.y;
									//yChangedCount -= 1;
									//Debug.Log("here 4");
									return;
								}

								CheckNeighbourInDirection(myNode.gridX, myNode.gridY - 1);
								if (Grid.instance.NodeFromXY(myNode.gridX, myNode.gridY - 1).walkable) {
									targetNode = Grid.instance.NodeFromXY(myNode.gridX, myNode.gridY - 1);
									targetPosition = targetNode.worldPosition;
									targetPosition.y = transform.position.y;
									yChangedCount += 1;
									//Debug.Log("here 5");
									return;
								}

								return;
							}

							myNode = targetNode;
							GetTargetPosition();
							previousX = myNode.gridX;
							previousY = myNode.gridY;
						}
					}
					else {
						time += Time.deltaTime;

						if (transform.localPosition.x > xMax) {
							x = Random.Range(-speed, 0.0f);
							angle = Mathf.Atan2(x, z) * (180 / 3.141592f) + 90;
							transform.localRotation = Quaternion.Euler(0, angle, 0);
							time = 0.0f;
						}
						if (transform.localPosition.x < xMin) {
							x = Random.Range(0.0f, speed);
							angle = Mathf.Atan2(x, z) * (180 / 3.141592f) + 90;
							transform.localRotation = Quaternion.Euler(0, angle, 0);
							time = 0.0f;
						}
						if (transform.localPosition.z > zMax) {
							z = Random.Range(-speed, 0.0f);
							angle = Mathf.Atan2(x, z) * (180 / 3.141592f) + 90;
							transform.localRotation = Quaternion.Euler(0, angle, 0);
							time = 0.0f;
						}
						if (transform.localPosition.z < zMin) {
							z = Random.Range(0.0f, speed);
							angle = Mathf.Atan2(x, z) * (180 / 3.141592f) + 90;
							transform.localRotation = Quaternion.Euler(0, angle, 0);
							time = 0.0f;
						}


						if (time > 1.0f) {
							x = Random.Range(-speed, speed);
							z = Random.Range(-speed, speed);
							angle = Mathf.Atan2(x, z) * (180 / 3.141592f) + 90;
							transform.localRotation = Quaternion.Euler(0, angle, 0);
							time = 0.0f;
						}

						transform.localPosition = new Vector3(transform.localPosition.x + x, transform.localPosition.y, transform.localPosition.z + z);
						myNode = Grid.instance.NodeFromWorldPoint(transform.position);
						Grid.instance.DiscoveredNodes(myId, myNode);

					}
					myNode = Grid.instance.NodeFromWorldPoint(transform.position);
					Grid.instance.DiscoveredNodes(myId, myNode);

				}
				break;
		}

	}




	void CheckNeighbourInDirection(int x, int y) {
		Node n = Grid.instance.NodeFromXY(x, y);
		if (n.discovered) return;

		RaycastHit hit;
		Vector3 ext = new Vector3(Grid.instance.nodeRadius, Grid.instance.nodeRadius, Grid.instance.nodeRadius);
		if (Physics.BoxCast(n.worldPosition - (Vector3.up * 5f), ext, Vector3.up, out hit, Quaternion.identity, 20, Grid.instance.unwalkableMask)) {
			if (hit.collider.transform == this.transform) return;
			Grid.instance.DiscoveredNode(myId, n.gridX, n.gridY, false);
		}
		else {
			Grid.instance.DiscoveredNode(myId, n.gridX, n.gridY, true);
		}
	}

	void GetTargetPosition() {
		if (myNode.gridX == Grid.instance.GridSizeX - 1 && myNode.gridY == Grid.instance.GridSizeY - 1) {
			targetNode = myNode;
			targetPosition = transform.position;
			return;
		}

		int targetX = myNode.gridX;
		int targetY = myNode.gridY;

		if (walkLeft == false) {
			targetX = myNode.gridX + 1;
		}
		else {
			targetX = myNode.gridX - 1;
		}

		if (targetX >= Grid.instance.GridSizeX) {
			targetX = myNode.gridX;
			targetY = myNode.gridY + 1;
			walkLeft = true;
		}
		else if (targetX < 0) {
			targetX = myNode.gridX;
			targetY = myNode.gridY + 1;
			walkLeft = false;
		}

		targetNode = Grid.instance.NodeFromXY(targetX, targetY);
		targetPosition = targetNode.worldPosition;
		targetPosition.y = transform.position.y;
		Grid.instance.DiscoveredNode(myId, myNode.gridX, myNode.gridY, true);
	}

	private void OnTriggerEnter(Collider collision) {
		if (collision.gameObject.name.Contains("Rock") || collision.gameObject.name.Contains("Cube")) {

			UnityEngine.Debug.Log("i hit a rock");
			avoided = true;
			if (collision.gameObject.transform.position.x < transform.position.x) {
				walkLeft = true;
			}
			else {
				walkLeft = false;
			}

			myNode = Grid.instance.NodeFromWorldPoint(transform.position);
			previousX = myNode.gridX;
			previousY = myNode.gridY;

			int targetX = (walkLeft == false) ? myNode.gridX - 1 : myNode.gridX + 1;
			int targetY = myNode.gridY - 1;

			if (targetX >= Grid.instance.GridSizeX) {
				targetY -= 1;
			}
			else if (targetX < 0) {
				targetX = 0;
			}

			targetNode = Grid.instance.NodeFromXY(targetX, targetY);
			targetPosition = targetNode.worldPosition;
			targetPosition.y = transform.position.y;
			yChangedCount += 1;


		}







		if (collision.gameObject.name.Contains("Sawmill")) {
			UnityEngine.Debug.Log("i hit a Sawmill");
			millPos = Grid.instance.NodeFromWorldPoint(collision.gameObject.transform.position).worldPosition;
			millPos.y = transform.position.y;
			foundMill = true;

			Grid.instance.DiscoveredNode(myId, Grid.instance.NodeFromWorldPoint(collision.gameObject.transform.position).gridX, Grid.instance.NodeFromWorldPoint(collision.gameObject.transform.position).gridY);
			Grid.instance.DiscoveredNodes(myId, Grid.instance.NodeFromWorldPoint(collision.gameObject.transform.position));

			if (foundMill == true && foundTree == true) {
				if (treeCut == false) {
					PathRequestManager.RequestPath(myId, transform.position, treePos, OnPathFound);
					searching = false;
					UnityEngine.Debug.Log("done2");
					action = 1;

				}
				else {
					sw.Stop();
					UnityEngine.Debug.Log("done by " + myId);
					print("Task Complete: " + sw.ElapsedMilliseconds + " ms");
					UnityEngine.Debug.Log("TREEE IS GONE N");
					action = 2;	
				}
			}

		}


		if (collision.gameObject.name.Contains("Tree")) {

			UnityEngine.Debug.Log("i hit a Tree");
			treePos = Grid.instance.NodeFromWorldPoint(collision.gameObject.transform.position).worldPosition;
			treePos.y = transform.position.y;
			foundTree = true;
			Grid.instance.DiscoveredNode(myId, Grid.instance.NodeFromWorldPoint(collision.gameObject.transform.position).gridX, Grid.instance.NodeFromWorldPoint(collision.gameObject.transform.position).gridY);
			Grid.instance.DiscoveredNodes(myId, Grid.instance.NodeFromWorldPoint(collision.gameObject.transform.position));

			if (foundMill == true && foundTree == true) {
				if (treeCut == false) {
					collision.gameObject.SetActive(false);
					searching = false;
					treeCut = true;
					UnityEngine.Debug.Log("cutting tree");
					action = 1;
					PathRequestManager.RequestPath(myId, transform.position, millPos, OnPathFound);
				}

			}

		}

		if (collision.gameObject.tag=="Player") {
			UnityEngine.Debug.Log("sharing knowledge");
			Node[,] mygrid = Grid.instance.GetGridFromID(this.gameObject.GetInstanceID());
			Node[,] othergrid = Grid.instance.GetGridFromID(collision.gameObject.GetInstanceID());

			for (int x = 0; x < mygrid.GetLength(0); x++) {
				for (int y = 0; y < mygrid.GetLength(1); y++) {
					if (mygrid[x, y].discovered) {
						Grid.instance.DiscoveredNode(collision.gameObject.GetInstanceID(), x, y, mygrid[x, y].walkable);
						UnityEngine.Debug.Log("knowledge shared 1");
					}
					else if (othergrid[x, y].discovered) {
						UnityEngine.Debug.Log("knowledge shared 2");
						Grid.instance.DiscoveredNode(myId, x, y, mygrid[x, y].walkable);
					}
				}
			}
		}
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
		if (pathSuccessful) {
			UnityEngine.Debug.Log("Calculating path");
			path = newPath;
			targetIndex = 0;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath() {
		UnityEngine.Debug.Log(path.Length);
		Vector3 currentWaypoint = path[0];

		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex++;
				if (targetIndex >= path.Length) {
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}

			if (movements == Movements.random) {
				speed = 50;
			}

			transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
			yield return null;

		}
	}

	IEnumerator Wait() {
		rb.constraints = RigidbodyConstraints.FreezeAll;
		yield return new WaitForSeconds(5);


	}




	public void OnDrawGizmos() {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i - 1], path[i]);
				}
			}
		}
	}
}
