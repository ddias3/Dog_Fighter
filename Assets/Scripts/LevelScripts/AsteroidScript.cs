using UnityEngine;
using System.Collections;

public class AsteroidScript : MonoBehaviour {
	
	Vector3 rotate;
	
	// Use this for initialization
	void Start () {
		Vector3 force = new Vector3(Random.value-.5f,Random.value-.5f,Random.value-.5f);
		force.Normalize();
		rigidbody.AddForce(force*10000000);
		rotate = new Vector3(Random.value-.5f,Random.value-.5f,Random.value-.5f);
		rotate.Normalize();
		rotate = rotate/10f;
		rigidbody.angularVelocity = rotate;
	}
}
