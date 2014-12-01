using UnityEngine;
using System.Collections;

public class MissileScript : MonoBehaviour {

	public float coolDown=5f;
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