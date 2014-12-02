using UnityEngine;
using System.Collections;

public class MissileScript : MonoBehaviour {
	
	private GameObject target;
	
	void Update () {
		if(target != null){
			Vector3 heading = target.transform.position - transform.position;
			Vector3 direction = heading / heading.magnitude;
			Vector3 currDir = transform.TransformDirection(new Vector3(0,0,1));
			transform.rotation = Quaternion.LookRotation(Vector3.Slerp(currDir,direction,.5f));
		}
	}
	
	public void SetTarget(GameObject newTarget){
		target = newTarget;
	}
	
}
