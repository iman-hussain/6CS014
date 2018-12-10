using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomUnit : MonoBehaviour {




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

	//Pathfinding pathfinder = new Pathfinding();
	PathRequestManager prm = new PathRequestManager();
	int origY = 0;
	int previousX = 0, previousY = 0;


	public Rigidbody rb;
	int myId;


	void Start() {


		rb = GetComponent<Rigidbody>();
		Grid.instance.RegisterAgent(myId);

		myNode = Grid.instance.NodeFromWorldPoint(transform.position);
		Grid.instance.DiscoveredNodes(myId, myNode);
		x = Random.Range(-speed, speed);
		z = Random.Range(-speed, speed);
		angle = Mathf.Atan2(x, z) * (180 / 3.141592f) + 90;
		transform.localRotation = Quaternion.Euler(0, angle, 0);

	}

	private void Update() {
		if (searching) {
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

	private void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag==("Rocks")){

			Debug.Log("i hit a rock");
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






	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag==("Sawmill")) {
			Debug.Log("i hit a Sawmill");
			millPos = Grid.instance.NodeFromWorldPoint(other.gameObject.transform.position).worldPosition;
			millPos.y = transform.position.y;
			foundMill = true;

			Grid.instance.DiscoveredNode(myId, Grid.instance.NodeFromWorldPoint(other.gameObject.transform.position).gridX, Grid.instance.NodeFromWorldPoint(other.gameObject.transform.position).gridY);
			Grid.instance.DiscoveredNodes(myId, Grid.instance.NodeFromWorldPoint(other.gameObject.transform.position));

			if (foundMill == true && foundTree == true) {
				if (treeCut == false) {
					PathRequestManager.RequestPath(myId, transform.position, treePos, OnPathFound);
					searching = false;
					Debug.Log("done2");

				}
				else {
					Debug.Log("done");
				}
			}

		}


		if (other.gameObject.tag==("Tree")) {
			Debug.Log("i hit a Tree");
			treePos = Grid.instance.NodeFromWorldPoint(other.gameObject.transform.position).worldPosition;
			treePos.y = transform.position.y;
			foundTree = true;
			Grid.instance.DiscoveredNode(myId, Grid.instance.NodeFromWorldPoint(other.gameObject.transform.position).gridX, Grid.instance.NodeFromWorldPoint(other.gameObject.transform.position).gridY);
			Grid.instance.DiscoveredNodes(myId, Grid.instance.NodeFromWorldPoint(other.gameObject.transform.position));

			if (foundMill == true && foundTree == true) {
				if (treeCut == false) {
					other.gameObject.SetActive(false);
					searching = false;
					treeCut = true;
					Debug.Log("cutting tree");
					PathRequestManager.RequestPath(myId, transform.position, millPos, OnPathFound);
				}
			}
		}
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
		if (pathSuccessful) {
			Debug.Log("Calculating path");
			path = newPath;
			targetIndex = 0;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath() {
		Debug.Log(path.Length);
		Vector3 currentWaypoint = path[0];
		speed = 100;
		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex++;
				if (targetIndex >= path.Length) {
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}

			transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
			yield return null;

		}
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
