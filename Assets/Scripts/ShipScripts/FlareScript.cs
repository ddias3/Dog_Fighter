using UnityEngine;
using System.Collections;

public class FlareScript : MonoBehaviour {

	public float coolDown=7f;
	private float lastFired;
	
	void Start () {
		lastFired = -999f;
	}
	
	float Fire() {
		if(Time.time-lastFired<coolDown){
			lastFired = Time.time;
			//also actually fire
			return lastFired;
		}
	}
}