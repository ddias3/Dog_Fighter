using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DogFighter
{
	public class DeathMatchAction : Action, IPassPrefab
	{
		public GameObject singleShipControlActionPrefab;
		public Transform[] spawnPoints;
		private Transform[] spawnPointOutput;

		public override void ActionStart()
		{
			int numberPlayers = DataManager.GetNumberPlayers();
			Debug.Log("Players Playing: " + numberPlayers);

			spawnPointOutput = new Transform[numberPlayers];
			bool[] spawnPointUsed = new bool[spawnPoints.Length];
			for (int n = 0; n < spawnPointUsed.Length; ++n)
				spawnPointUsed[n] = false;
			for (int n = 0; n < spawnPointOutput.Length; ++n)
			{
				int spawnPointIndex;
				do {
					spawnPointIndex = Random.Range(0, spawnPoints.Length);
				} while (spawnPointUsed[spawnPointIndex]);
				spawnPointUsed[spawnPointIndex] = true;
				spawnPointOutput[n] = spawnPoints[spawnPointIndex];
			}

			for (int n = 0; n < numberPlayers; ++n)
			{
				SceneManager.SendMessage(this, "instantiate_named_from_prefab SingleShipControlAction SingleShipControlAction_P" + (n + 1));
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
			string[] messageTokens = message.Split(' ');

			switch (messageTokens[0])
			{
			case "get":
				switch (messageTokens[1])
				{
				case "spawn_point":
				{
					Transform transform = spawnPointOutput[((SingleShipControlAction)action).PlayerNumber - 1];
					((SingleShipControlAction)action).PassSpawnPoint(transform.position, transform.rotation);
					break;
				}
				case "player_number":
					switch (action.Name)
					{
					case "SingleShipControlAction_P1":
						((SingleShipControlAction)action).PlayerNumber = 1;
						break;
					case "SingleShipControlAction_P2":
						((SingleShipControlAction)action).PlayerNumber = 2;
						break;
					case "SingleShipControlAction_P3":
						((SingleShipControlAction)action).PlayerNumber = 3;
						break;
					case "SingleShipControlAction_P4":
						((SingleShipControlAction)action).PlayerNumber = 4;
						break;
					}
					break;
				}
				break;
			}
		}

		public GameObject GetPrefab(string prefabName)
		{
			return singleShipControlActionPrefab;
		}
	}
}
