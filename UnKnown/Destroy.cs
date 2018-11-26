using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour {

	void OnTrEnter(Collider col){
		if(col.gameObject.name == "Target"){
			Destroy(col.gameObject);
		}
	}
}
