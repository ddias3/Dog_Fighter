using UnityEngine;
using System.Collections;

public class AsteroidScript : MonoBehaviour {
	
	Vector3 rotate;
	
	// Use this for initialization
	void Start () {
		Vector3 force = new Vector3(Random.value-.5f,Random.value-.5f,Random.value-.5f);
		force.Normalize();
		rigidbody.AddForce(force*50000000/((Random.value*9f)+1));
		rotate = new Vector3(Random.value-.5f,Random.value-.5f,Random.value-.5f);
		rotate.Normalize();
		rotate = rotate/(2.5f*((Random.value*3f)+1f));
		rigidbody.angularVelocity = rotate;
	}
}
