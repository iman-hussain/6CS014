using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour{
	public GameObject MiscPrefab; //Game object to spawn
	public Vector3 centre; //Centre of the spawn box
	public Vector3 size; //Spawn area size
	public int ObstacleCount; //Public inspector where you can add the amount of objects

	void Start() {
		SpawnMisc(); //Spawns on play
	}

	void Update() {

	}

	public void SpawnMisc() {
		for (int i = 0; i < ObstacleCount; i++) {  //Loops through the amount of obstacles in obstacle count and instantiates them
			Vector3 pos = centre + new Vector3(Random.Range(-size.x / 2, size.x / 2), Random.Range(-size.y / 2, size.y / 2), Random.Range(-size.z / 2, size.z / 2));
			//^ Randomises position
			Instantiate(MiscPrefab, pos, Quaternion.identity);
			//Takes assigned MiscPrefab, sets it's position to the random and gives it identity
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = new Color(0, 0, 225); //Grid to show where it's going to spawn
		Gizmos.DrawWireCube(centre, size); //Draws grid
	}
}