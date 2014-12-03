using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class MissileScript : MonoBehaviour {
		
		private Transform target;
		
		void Start() {
			rigidbody.velocity += transform.rotation * new Vector3(0,0,300);
		}

		void FixedUpdate() {
			if(target != null){
				Vector3 heading = target.position - transform.position;
				Vector3 direction = heading / heading.magnitude;
				Vector3 currDir = transform.TransformDirection(new Vector3(0,0,1));
				transform.rotation = Quaternion.LookRotation(Vector3.Slerp(currDir,direction,.5f));
				rigidbody.velocity = transform.rotation * rigidbody.velocity;
			}

			RaycastHit hit = new RaycastHit ();
			Vector3 dir = rigidbody.velocity / rigidbody.velocity.magnitude;
			if(Physics.Raycast(transform.position,dir,out hit)){
				if(Vector3.Distance(hit.point, transform.position)<rigidbody.velocity.magnitude){
					//collision
					Destroy(this.gameObject);
				}
			}
		}
		
		public void SetTarget(Transform newTarget){
			target = newTarget;
		}
	}
}