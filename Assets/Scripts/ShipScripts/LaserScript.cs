using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class LaserScript : MonoBehaviour {

		public float coolDown = .75f;
		public float timeVisible = .5f;
		public float shotCost = 30f;
		public float chargeRate = 4f;
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
			laser.SetVertexCount(2);
			laser.SetWidth(.5f,.5f);
			laser.SetColors(new Color(0f,1f,0f,.75f),new Color(0f,1f,0f,.75f));
		}
		
		void Update(){
			ownTime += Time.deltaTime;
			if(ownTime-lastFired>timeVisible){
				laser.enabled = false;
			}
		}
		
		void FixedUpdate(){
			if(charge<100f){
				charge += chargeRate/10f;
			}
		}

		public float Fire(Transform t) {
			if (ownTime - lastFired > coolDown && charge >= shotCost)
			{
				Vector3 fireFrom = new Vector3(0,-1,5);
				fireFrom = t.rotation * fireFrom;
				fireFrom += t.position;
				Vector3 fireAt;
				if (target != null)
				{
					fireAt = target.transform.position;
				}
				else
				{
					fireAt = new Vector3(0, 0, 1000);
					fireAt = t.rotation * fireAt;
					fireAt += t.position;
					RaycastHit hit = new RaycastHit();
					if(Physics.Raycast(fireFrom,fireAt-fireFrom,out hit)){
						fireAt = hit.point;
					}
				}
				lastFired = ownTime;
				charge -= shotCost;
				laser.SetPosition(0,fireFrom);
				laser.SetPosition(1,fireAt);
				laser.enabled = true;
			}
			return charge;
		}

		float Fire() {
			if(Time.time-lastFired<coolDown){
				lastFired = Time.time;
				//also actually fire
				return lastFired;
			}
			else{
				return -1f;
			}
		}

		public void SetTarget(GameObject newTarget){
			target = newTarget;
		}
	}
}
