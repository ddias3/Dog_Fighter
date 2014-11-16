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
			float pitch = Input.GetAxis("Right_Vertical");
			float yaw = Input.GetAxis("Right_Horizontal");
			float roll = -Input.GetAxis("Left_Horizontal");

			throttle -= 2f * Input.GetAxis("Left_Vertical") * TimeStack.deltaTime;
			if (throttle > 1f)
				throttle = 1f;
			if (throttle < -0.2f)
				throttle = -0.2f;

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
