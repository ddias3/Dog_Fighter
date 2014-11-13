using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class SingleShipAction : Action
	{
		public override void ActionStart()
		{
			Debug.Log("SingleShipAction started");
		}
		
		public override void ActionUpdate()
		{

		}
		
		public override void ActionFixedUpdate()
		{
		
		}

		public override void ActionOnGUI()
		{
			GUI.Label(new Rect(0, 0, Screen.width, 20), "Ship Control");
		}
		
		public override void ReceiveMessage(Action action, string message)
		{
		
		}
	}
}
