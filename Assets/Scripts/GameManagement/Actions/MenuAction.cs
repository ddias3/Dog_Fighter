using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class MenuAction : Action
	{
		private string testWords;

		public override void ActionStart()
		{
			testWords = "Run the Game";

			Debug.Log("MenuAction started");
		}

		public override void ActionUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				GameManager.SendMessage(this, "start SingleShipAction");
				GameManager.SendMessage(this, "remove from_action_list");
			}
		}

		public override void ActionFixedUpdate()
		{
			// do nothing;
		}

		public override void ActionOnGUI()
		{
			if (GUI.Button(new Rect(40, 40, Screen.width - 80, Screen.height - 80), testWords))
			{
				GameManager.SendMessage(this, "start SingleShipAction");
				GameManager.SendMessage(this, "remove from_action_list");
			}
		}
		
		public override void ReceiveMessage(Action action, string message)
		{
			// ignore messages.
		}
	}
}
