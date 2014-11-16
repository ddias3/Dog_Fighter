using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class SingleShipControlAction : Action
	{
		public GameObject shipPrefab;

		private GameObject shipGameObject;
		private PlayerShip playerShip;

		private int playerNumber = 1;

		public override void ActionStart()
		{
			shipGameObject = Instantiate(shipPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			playerShip = shipGameObject.GetComponent<PlayerShip>();
		}

		float throttle = 0f;
		public override void ActionUpdate()
		{
			float pitch = -Input.GetAxis("Pitch");
			float yaw = Input.GetAxis("Yaw");
			float roll = Input.GetAxis("Roll");

			throttle += Input.GetAxis("Mouse ScrollWheel");

			playerShip.Throttle = throttle;
			playerShip.Pitch = pitch;
			playerShip.Yaw = yaw;
			playerShip.Roll = roll;
		}
		
		public override void ActionFixedUpdate()
		{
		
		}

		public override void ActionOnGUI()
		{

		}
		
		public override void ReceiveMessage(Action action, string message)
		{
			string[] messageTokens = message.Split(' ');

			switch (messageTokens[0])
			{
			case "set":
				switch (messageTokens[1])
				{
				case "player_number":
					playerNumber = int.Parse(messageTokens[2]);
					break;
				}
				break;
			}
		}
	}
}
