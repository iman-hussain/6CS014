using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTrigger : MonoBehaviour {

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag =="Target"){
			Debug.Log("Cut the tree");
			Destroy(other.gameObject);
		}
	}
}
