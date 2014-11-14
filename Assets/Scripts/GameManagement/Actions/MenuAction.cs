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
			DataManager.GetNumberPlayers();
		}

		public override void ActionUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				Application.LoadLevel("Map1Scene");
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
				Application.LoadLevel("Map1Scene");
			}
		}
		
		public override void ReceiveMessage(Action action, string message)
		{
			// ignore messages.
		}
	}
}
