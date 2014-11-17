using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class DeathMatchAction : Action
	{
		public override void ActionStart()
		{
			SceneManager.SendMessage(this, "run_named SingleShipControlAction SingleShipControlActionP1");
			SceneManager.SendMessageToAction(this, "SingleShipControlActionP1", "set player_number 1");
		}
		
		public override void ActionUpdate()
		{
			
		}
		
		public override void ActionFixedUpdate()
		{
			// do nothing
		}
		
		public override void ActionOnGUI()
		{
			// maybe we'll do something here.
		}
		
		public override void ReceiveMessage(Action action, string message)
		{
			
		}
	}
}
