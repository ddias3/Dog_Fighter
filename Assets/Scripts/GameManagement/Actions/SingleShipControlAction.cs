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

		private float throttle = 0f;
		private bool controller = true;
		public override void ActionUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Return))
				controller = !controller;

			float pitch;
			float yaw;
			float roll;

			if (controller)
			{
				pitch = Input.GetAxis("Right_Vertical");
				yaw = Input.GetAxis("Right_Horizontal");
				roll = -Input.GetAxis("Left_Horizontal");
				throttle -= 2f * Input.GetAxis("Left_Vertical") * TimeStack.deltaTime;
			}
			else
			{
				pitch = Input.GetAxis("Pitch");
				roll = -0.75f * Input.GetAxis("Yaw");
				yaw = -2*Input.GetAxis("Roll");
				throttle += 2f * Input.GetAxis("Throttle") * TimeStack.deltaTime;
			}

			if (throttle > 4f)
				throttle = 4f;
			if (throttle < -1f)
				throttle = -1f;

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
