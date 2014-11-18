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
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles+rotate);
	}
	
	void OnCollisionEnter(Collision collision){
		rotate *= -1f;
	}
}
