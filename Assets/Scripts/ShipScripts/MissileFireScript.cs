using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class MissileFireScript : MonoBehaviour {

		public float coolDown=5f;
		public GameObject missile;
		private float lastFired;
		private float ownTime;
		
		private GameObject target;
		
		void Start () {
			lastFired = -999f;
			ownTime = 0f;
		}
		
		void Update(){
			ownTime += Time.deltaTime;
		}
		
		public float Fire(Transform t, Vector3 v) {
			if(ownTime-lastFired>coolDown){
				lastFired = ownTime;
				Vector3 missileLoc = new Vector3(0,-1,1);
				missileLoc = t.rotation * missileLoc;
				missileLoc += t.position;
				GameObject missileFired = Instantiate(missile,missileLoc,t.rotation) as GameObject;
				missileFired.rigidbody.velocity = v;
				MissileScript mScript = missileFired.GetComponent<MissileScript>();
				mScript.SetTarget(target);
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