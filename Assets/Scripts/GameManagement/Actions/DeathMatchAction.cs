using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class DeathMatchAction : Action, IPassPrefab
	{
		public GameObject singleShipControlActionPrefab;

		public override void ActionStart()
		{
			int numberPlayers = DataManager.GetNumberPlayers();
			Debug.Log("Players Playing: " + numberPlayers);

			for (int n = 0; n < numberPlayers; ++n)
			{
				SceneManager.SendMessage(this, "instantiate_named_from_prefab SingleShipControlAction SingleShipControlAction_P" + (n + 1));
				SceneManager.SendMessageToAction(this, "SingleShipControlAction_P" + (n + 1), "set player_number " + (n + 1));
			}
		}
		
		public override void ActionUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Return))
				SceneManager.SendMessageToAction(this, "SingleShipControlAction_P1", "enable_controls");
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

		public GameObject GetPrefab(string prefabName)
		{
			return singleShipControlActionPrefab;
		}
	}
}
