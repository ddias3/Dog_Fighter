using UnityEngine;
using System.Collections;

public class FlareScript : MonoBehaviour {

	public float coolDown=7f;
	public GameObject flare;
	private float lastFired;
	
	void Start () {
		lastFired = -999f;
	}
	
	public float Fire(Transform t) {
		if(Time.time-lastFired<coolDown){
			lastFired = Time.time;
			//also actually fire
			Vector3 flareLeft = new Vector3(-2,0,0);
			flareLeft = t.rotation * flareLeft;
			Vector3 flareRight = new Vector3(2,0,0);
			flareRight = t.rotation * flareRight;
			Instantiate(flare,flareLeft,t.rotation);
			Instantiate(flare,flareRight,t.rotation);
			return lastFired;
		}
		else{
			return -1f;
		}
	}
}