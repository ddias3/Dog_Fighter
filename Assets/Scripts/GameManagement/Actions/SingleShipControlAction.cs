using UnityEngine;
using System.Collections.Specialized;

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
		private Vector3 spawnLocation = Vector3.zero;
		private Quaternion spawnDirection = Quaternion.identity;

		private Camera playerCamera;
		private Vector3 localCameraPosition;

		private ListDictionary inputMap;
		private ControllerDirectInputHandler inputHandler;

		public override void ActionStart()
		{
			SceneManager.SendMessageToAction(this, "DeathMatchAction", "get player_number");
			SceneManager.SendMessageToAction(this, "DeathMatchAction", "get spawn_point");
			shipGameObject = Instantiate(shipPrefab, spawnLocation, spawnDirection) as GameObject;
			playerShip = shipGameObject.GetComponent<PlayerShip>();
			playerCamera = playerShip.GetComponentInChildren<Camera>();
			localCameraPosition = new Vector3(playerCamera.transform.localPosition.x,
			                                  playerCamera.transform.localPosition.y,
			                                  playerCamera.transform.localPosition.z);

			inputHandler = InputHandlerHolder.GetDirectInputHandler(playerNumber);
			inputMap = new ListDictionary();
			switch (DataManager.GetControllerSetup(playerNumber))
			{
			case 0:
				inputMap.Add("Throttle", "Left_Vertical");
				inputMap.Add("Roll", "Left_Horizontal");
				inputMap.Add("Pitch", "Right_Vertical");
				inputMap.Add("Yaw", "Right_Horizontal");
				break;

			case 1:
				inputMap.Add("Throttle", "Right_Vertical");
				inputMap.Add("Roll", "Right_Horizontal");
				inputMap.Add("Pitch", "Left_Vertical");
				inputMap.Add("Yaw", "Left_Horizontal");
				break;
            
            case 2:
                inputMap.Add("Throttle", "Right_Vertical");
                inputMap.Add("Roll", "Left_Horizontal");
                inputMap.Add("Pitch", "Left_Vertical");
                inputMap.Add("Yaw", "Right_Horizontal");
                break;

			case 3:
				inputMap.Add("Throttle", "Left_Vertical");
				inputMap.Add("Roll", "Right_Horizontal");
				inputMap.Add("Pitch", "Right_Vertical");
				inputMap.Add("Yaw", "Left_Horizontal");
				break;
			}

			invertPitchScalar = (DataManager.GetInvertPitch(playerNumber) > 0) ? -1 : 1;
			invertYawScalar = (DataManager.GetInvertYaw(playerNumber) > 0) ? -1 : 1;
			invertRollScalar = (DataManager.GetInvertRoll(playerNumber) > 0) ? -1 : 1;

			SetupScreenValues(DataManager.GetNumberPlayers());
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
		private float invertPitchScalar = 0f;
		private float invertYawScalar = 0f;
		private float invertRollScalar = 0f;

		public override void ActionUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Alpha0))
				controller = false;

			if (inputHandler.GetButtonDown("Back_Button"))
				SceneManager.SendMessageToAction(this, "DeathMatchAction", "show_scoreboard " + playerNumber);

			if (inputHandler.GetButtonUp("Back_Button"))
				SceneManager.SendMessageToAction(this, "DeathMatchAction", "hide_scoreboard " + playerNumber);

			if (controlsEnabled)
			{
				float pitch;
				float yaw;
				float roll;
				float afterburnerInput;

				if (controller)
				{
					pitch = invertPitchScalar * inputHandler.GetAxis(inputMap["Pitch"] as string);
					yaw = invertYawScalar * inputHandler.GetAxis(inputMap["Yaw"] as string);
					roll = invertRollScalar * inputHandler.GetAxis(inputMap["Roll"] as string);

					throttleAdjustInput = inputHandler.GetAxis(inputMap["Throttle"] as string);
					afterburnerInput = inputHandler.GetAxis("Left_Trigger");
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
				playerShip.Pitch = -pitch;
				playerShip.Yaw = yaw;
				playerShip.Roll = -roll;
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
				playerShip.Throttle = throttleOutput = 0;
				playerShip.Pitch = 0;
				playerShip.Yaw = 0;
				playerShip.Roll = 0;
				break;
			case "get":
				switch (messageTokens[1])
				{
				case "camera_transform":
					((DeathMatchAction)action).PassCameraTransform(playerNumber, playerCamera.transform);
					break;
				}
				break;
			case "reset_camera":
				playerCamera.transform.parent = shipGameObject.transform;
				playerCamera.transform.localPosition = localCameraPosition;
				break;
			}
		}

		public int PlayerNumber
		{
			get { return playerNumber; }
			set { playerNumber = value; }
		}

		public void PassSpawnPoint(Vector3 location, Quaternion direction)
		{
			spawnLocation = location;
			spawnDirection = direction;
//			Debug.Log(spawnLocation + " | " + spawnDirection);
		}

		private int screenWidth;
		private int screenHeight;
		private int screenLeftStart;
		private int screenTopStart;
		private int screenHorizontalStep;
		private int screenVerticalStep;

		private void SetupScreenValues(int numberPlayers)
		{
			switch (numberPlayers)
			{
			case 1:
				screenWidth = Screen.width;
				screenHeight = Screen.height;
				screenLeftStart = 0;
				screenTopStart = 0;
				screenHorizontalStep = Screen.width / 32;
				screenVerticalStep = Screen.height / 24;
				break;
			case 2:
				screenWidth = Screen.width;
				screenHeight = Screen.height / 2;
				screenHorizontalStep = Screen.width / 32;
				screenVerticalStep = Screen.height / 16;
				screenLeftStart = 0;
				switch (playerNumber)
				{
				case 1:
					playerCamera.rect = new Rect(0, 0.5f, 1, 0.5f);
					playerShip.stationaryFieldOfView = 55;
					playerShip.maxSpeedFieldOfView = 70;
					screenTopStart = 0;
					break;
				case 2:
					playerCamera.rect = new Rect(0, 0, 1, 0.5f);
					playerShip.stationaryFieldOfView = 55;
					playerShip.maxSpeedFieldOfView = 70;
					screenTopStart = screenHeight;
					break;
				}
				break;
			case 3:
				screenWidth = Screen.width / 2;
				screenHeight = Screen.height / 2;
				screenHorizontalStep = Screen.width / 24;
				screenVerticalStep = Screen.height / 16;
				switch (playerNumber)
				{
				case 1:
					playerCamera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
					screenTopStart = 0;
					screenLeftStart = 0;
					break;
				case 2:
					playerCamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
					screenTopStart = 0;
					screenLeftStart = screenWidth;
					break;
				case 3:
					playerCamera.rect = new Rect(0, 0, 0.5f, 0.5f);
					screenTopStart = screenHeight;
					screenLeftStart = 0;
					break;
				}
				break;
			case 4:
				screenWidth = Screen.width / 2;
				screenHeight = Screen.height / 2;
				screenHorizontalStep = Screen.width / 24;
				screenVerticalStep = Screen.height / 16;
				switch (playerNumber)
				{
				case 1:
					playerCamera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
					screenTopStart = 0;
					screenLeftStart = 0;
					break;
				case 2:
					playerCamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
					screenTopStart = 0;
					screenLeftStart = screenWidth;
					break;
				case 3:
					playerCamera.rect = new Rect(0, 0, 0.5f, 0.5f);
					screenTopStart = screenHeight;
					screenLeftStart = 0;
					break;
				case 4:
					playerCamera.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
					screenTopStart = screenHeight;
					screenLeftStart = screenWidth;
					break;
				}
				break;
			}
		}
	}
}
