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

        private float barHeight = 80;
        private float barWidth = 10;
        private float barSpace = 9;
        private float pinch = 0;

        public Texture image;

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
				throttle -= 2f * Input.GetAxis("Left_Vertical") * Time.deltaTime;
			}
			else
			{
				pitch = Input.GetAxis("Pitch");
				roll = -Input.GetAxis("Yaw");
				yaw = -Input.GetAxis("Roll");
				throttle += Input.GetAxis("Throttle") * Time.deltaTime;
			}

			if (throttle > 1f)
				throttle = 1f;
			if (throttle < -0.2f)
				throttle = -0.2f;

			if (Input.GetKeyDown(KeyCode.Alpha1))
				throttle = 0f;

			playerShip.Throttle = throttle;
			playerShip.Pitch = pitch;
			playerShip.Yaw = yaw;
			playerShip.Roll = roll;

            if (Input.GetKeyDown(KeyCode.Escape))
                Application.LoadLevel("MenuScene");
		}
		
		public override void ActionFixedUpdate()
		{
		
		}

		public override void ActionOnGUI()
		{
            for (float i=0; i<playerShip.Speed/50; i++) {
                if (i > 0 && i < 5) { pinch += 5; }
                else if (i > 7) { pinch -= 5; }
                else if (i == 0) { pinch = 0; }
                else { pinch = 25;}
                GUI.Box (new Rect(Screen.width - (barHeight - pinch/2), Screen.height - (barSpace*i + barWidth), barHeight - pinch, barWidth), image);
            }
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
