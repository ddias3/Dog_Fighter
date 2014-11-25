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

		public AnimationCurve throttleAdjustCurve;

		private bool controlsEnabled = false;

		public override void ActionStart()
		{
			shipGameObject = Instantiate(shipPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			playerShip = shipGameObject.GetComponent<PlayerShip>();
		}

		private float throttleOutput = 0f;
		private float throttlePrecise = 0f;
		private float throttleAdjustInput = 0f;
		private float throttleAdjustInputMagnitude = 0f;

		private const float THROTTLE_ADJUST_CONSTANT = 1.75f;
//		private const float THROTTLE_ADJUST_DAMPENING_CONSTANT = 0.5f;
		private const float THROTTLE_DEAD_ZONE = 0.15f;
		private const float MAX_THROTTLE_DEAD_ZONE = 0.04f;
		private const float THROTTLE_DEAD_ZONE_DECREASE_SCALAR = 24f;
		private const float MAX_THROTTLE_DEAD_ZONE_INCREASE_SCALAR = 30f;

		private bool controller = true;

		public override void ActionUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				Application.LoadLevel("MenuScene");

			if (controlsEnabled)
			{
				float pitch;
				float yaw;
				float roll;
				float afterburnerInput;

				if (controller)
				{
					pitch = Input.GetAxis("Right_Vertical_P1");
					yaw = Input.GetAxis("Right_Horizontal_P1");
					roll = -Input.GetAxis("Left_Horizontal_P1");

					throttleAdjustInput = -Input.GetAxis("Left_Vertical_P1");
					afterburnerInput = Input.GetAxis("Left_Trigger_P1");
				}
				else
				{
					pitch = Input.GetAxis("Pitch");
					roll = -Input.GetAxis("Yaw");
					yaw = -Input.GetAxis("Roll");

					throttleAdjustInput = Input.GetAxis("Throttle");
					afterburnerInput = Input.GetAxis("Afterburner");
				}

				throttleAdjustInputMagnitude = Mathf.Abs(throttleAdjustInput);
				throttlePrecise += THROTTLE_ADJUST_CONSTANT * Mathf.Sign(throttleAdjustInput) * throttleAdjustCurve.Evaluate(Mathf.Abs(throttleAdjustInput)) * Time.deltaTime;

				if (throttlePrecise > 1f)
					throttlePrecise = 1f;
				else if (throttlePrecise < -0.3f)
					throttlePrecise = -0.3f;

				if (Mathf.Abs(throttlePrecise) < THROTTLE_DEAD_ZONE)
				{
					if (throttleAdjustInputMagnitude < 0.01f)
						throttlePrecise -= (throttlePrecise * THROTTLE_DEAD_ZONE_DECREASE_SCALAR) * Time.deltaTime;
					throttleOutput = 0f;
				}
				else if (Mathf.Abs(throttlePrecise) > 1f - MAX_THROTTLE_DEAD_ZONE)
				{
					if (throttleAdjustInputMagnitude < 0.01f)
						throttlePrecise += ((1f - throttlePrecise) * MAX_THROTTLE_DEAD_ZONE_INCREASE_SCALAR) * Time.deltaTime;
					throttleOutput = 1f;
				}
				else
					throttleOutput = throttlePrecise;

				if (Input.GetKeyDown(KeyCode.Alpha1))
					throttleOutput = throttlePrecise = 0f;

				if (afterburnerInput > 0.5f)
				{
					playerShip.Afterburner = true;
					if (Mathf.Abs(throttlePrecise) > Mathf.Abs(throttleAdjustInput))
						playerShip.DirectThrottleSet = throttlePrecise;
					else
						playerShip.DirectThrottleSet = throttleAdjustInput;
				}
				else
				{
					playerShip.Afterburner = false;
				}

				playerShip.Throttle = throttleOutput;
				playerShip.Pitch = pitch;
				playerShip.Yaw = yaw;
				playerShip.Roll = roll;
			}
		}
		
		public override void ActionFixedUpdate()
		{
			// do nothing
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
			case "enable_controls":
				controlsEnabled = true;
				break;
			case "disable_controls":
				controlsEnabled = false;
				break;
			}
		}
	}
}
