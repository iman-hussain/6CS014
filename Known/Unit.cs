using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

	public Grid grid;
	Transform target;
	public Transform sawmill;
	public Transform tree;
	public float speed = 20;
	Vector3[] path;
	int targetIndex;
	int startIndex;

	void Start() {
		target = tree;
		PathRequestManager.RequestPath(transform.position,target.position, OnPathFound);
		/*
		List<Node> surrounding = grid.GetNeighbours(grid.NodeFromWorldPoint(transform.position));
		foreach (Node n in surrounding)
		{
		    grid.Discovered(n.gridX, n.gridY)
		}
		*/
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
		if (pathSuccessful) {
			path = newPath;
			targetIndex = 0;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath() {
		Vector3 currentWaypoint = path[0];
		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex ++;
				if (targetIndex >= path.Length) {
					Debug.Log("Found the tree");
					target = sawmill;
					PathRequestManager.RequestPath(transform.position,target.position, OnPathFound);
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}
			transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
			yield return null;
		}
	}

	public void OnDrawGizmos() {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}
		}
	}
}
