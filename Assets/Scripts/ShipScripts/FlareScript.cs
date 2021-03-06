﻿using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class FlareScript : MonoBehaviour {

		public float coolDown=7f;
		public GameObject flare;
		private float lastFired;

		private float ownTime;
		
		void Start () {
			lastFired = -999f;
			ownTime = 0f;
		}

		void Update(){
			ownTime += Time.deltaTime;
		}

		public float Fire(Transform t, Vector3 v) {
			if(ownTime-lastFired>coolDown){
                SceneManager.SendMessageToAction(null, "SoundPlayerAction", "play flareLaunch");
				lastFired = ownTime;
				Vector3 flareLeft = new Vector3(-2,0,-2);
				flareLeft = t.rotation * flareLeft;
				flareLeft += t.position;
				Vector3 flareRight = new Vector3(2,0,-2);
				flareRight = t.rotation * flareRight;
				flareRight += t.position;
				GameObject fLeft = Instantiate(flare,flareLeft,t.rotation) as GameObject;
				GameObject fRight = Instantiate(flare,flareRight,t.rotation) as GameObject;
				fLeft.rigidbody.velocity = v;
				fLeft.rigidbody.AddForce(t.rotation*new Vector3(-1,0,-2)*50);
				fRight.rigidbody.velocity = v;
				fRight.rigidbody.AddForce(t.rotation*new Vector3(1,0,-2)*50);
			}
			return lastFired;
		}

		public float getCharge(){
			return Mathf.Clamp01((ownTime - lastFired) / coolDown);
		}
	}
}