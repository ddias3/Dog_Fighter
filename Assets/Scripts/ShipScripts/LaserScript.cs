using UnityEngine;
using System.Collections;

public class LaserScript : MonoBehaviour {

	public float coolDown = .5f;
	public float timeVisible = .5f;
	public float shotCost = 10f;
	public float chargeRate = 5f;
	public LineRenderer laser;
	private float lastFired;
	private float ownTime;
	private float charge;
	
	private GameObject target;

	void Start () {
		lastFired = -999f;
		ownTime = 0f;
		charge = 100f;
		laser.enabled = false;
		laser.SetVertexCount(1);
		laser.SetWidth(.5f,.5f);
		laser.SetColors(new Color(0f,1f,0f,.75f),new Color(0f,1f,0f,.75f));
	}
	
	void Update(){
		if(ownTime-lastFired>timeVisible){
			laser.enabled = false;
		}
	}
	
	void FixedUpdate(){
		charge += chargeRate/10f;
	}

	public float Fire(Transform t) {
		if(ownTime-lastFired>coolDown && charge>=shotCost){
			Vector3 fireAt;
			if(target != null){
				fireAt = target.transform.position;
			}
			else{
				fireAt = new Vector3(0,0,1000);
				fireAt = t.rotation * fireAt;
				fireAt += t.position;
			}
			lastFired = ownTime;
			charge -= shotCost;
			laser.SetPosition(0,t.position);
			laser.SetPosition(1,fireAt);
			laser.enabled = true;
			return charge;
		}
		else{
			return -1f;
		}
	}
	
	public void SetTarget(GameObject newTarget){
		target = newTarget;
	}
	
}
