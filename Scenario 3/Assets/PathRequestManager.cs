using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PathRequestManager : MonoBehaviour {

	Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
	PathRequest currentPathRequest;

	static PathRequestManager instance;
	Pathfinding pathfinding;

	bool isProcessingPath;

	void Awake() {
		instance = this;
		pathfinding = GetComponent<Pathfinding>();
	}

	public static void RequestPath(int id, Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback) {
		Debug.Log("request a path");
		PathRequest newRequest = new PathRequest(id, pathStart, pathEnd, callback);
		instance.pathRequestQueue.Enqueue(newRequest);
		instance.TryProcessNext();
	}

	void TryProcessNext() {
		if (!isProcessingPath && pathRequestQueue.Count > 0) {
			Debug.Log("Processing Next Path");
			currentPathRequest = pathRequestQueue.Dequeue();
			isProcessingPath = true;
			pathfinding.StartFindPath(currentPathRequest.id, currentPathRequest.pathStart, currentPathRequest.pathEnd);
		}
	}

	public void FinishedProcessingPath(Vector3[] path, bool success) {
		Debug.Log("Finished Processing Path");
		currentPathRequest.callback(path, success);
		isProcessingPath = false;
		TryProcessNext();
	}

	struct PathRequest {
		public Vector3 pathStart;
		public Vector3 pathEnd;
		public Action<Vector3[], bool> callback;
		public int id;

		public PathRequest(int _id, Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback) {
			Debug.Log("Path Requested");
			pathStart = _start;
			pathEnd = _end;
			callback = _callback;
			id = _id;
		}

	}
}
