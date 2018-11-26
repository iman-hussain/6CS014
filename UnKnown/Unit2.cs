using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit2 : MonoBehaviour {

	Node self;
	Node target;
	Vector3 targetPosition;
	bool movingLeft = false;

	public float speed = 5f;
	// Use this for initialization
	void Start () {
		self = Grid.instance.NodeFromWorldPoint(transform.position); //This identifies home position of the seeker.
		target = Grid.instance.FromXY(self.gridX+1, self.gridY); //Requests grid and the movement. 
									//Keeps y the same and changes x by one to move up
		targetPosition = target.worldPosition; //Sets the reference to local
	}
	
	// Update is called once per frame
	void Update () {
		if(Vector3.Distance(transform.position,targetPosition)<0.1f){ 
			self = target;
			target = Grid.instance.FromXY((movingLeft)?self.gridX-1:self.gridX+1, self.gridY);

			if(target.gridX == Grid.instance.GridSizeX-1){
				target = Grid.instance.FromXY(self.gridX-1, self.gridY+1);
				movingLeft = true;
			}

			if(target.gridX == 0){
				target = Grid.instance.FromXY(self.gridX+1, self.gridY+1);
				movingLeft = false;
			}

			targetPosition = target.worldPosition;
			targetPosition.y = transform.position.y;
			
		}
		transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed*Time.deltaTime);
		Grid.instance.DiscoverNodes(transform.position);
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag =="Fences"){
			Debug.Log("Oopsie I've bumped into a fence");
			//transform.Translate (0,0,-1);
			//transform.Rotate(0,0,180);
		}
		if(other.gameObject.tag == "Rocks"){
			Debug.Log("Bumped into a rock");
		}
	}
}
