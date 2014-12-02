using UnityEngine;
using System.Collections.Specialized;

namespace DogFighter
{
	public class SingleShipControlAction : Action
	{
		public GameObject shipPrefab;

		public Texture2D throttleOverlay;
		public Texture2D throttleMarker;
		public Texture2D throttleBackdrop;

		public Texture2D speedometerTexture;

		public Texture2D crossHairTexture;
		public Texture2D laserLockOnCircleTexture;
		public Texture2D missileLockOnCircleTexture;

		public Texture2D playerIconTexture;

		private GameObject shipGameObject;
		private PlayerShip playerShip;

		private int playerNumber = 1;
		private int controllerNumber = 1;

//        private float barHeight = 80;
//        private float barWidth = 10;
//        private float barSpace = 9;
//        private float pinch = 0;

//        public Texture image;

		public AnimationCurve throttleAdjustCurve;

		private bool controlsEnabled = false;
		private Vector3 spawnLocation = Vector3.zero;
		private Quaternion spawnDirection = Quaternion.identity;

		private Camera playerCamera;
		private Vector3 localCameraPosition;

		private ListDictionary inputMap;
		private ControllerDirectInputHandler inputHandler;

        public GUIStyle throttleGuiStyle;
        public GUIStyle speedometerGuiStyle;

		public FlareScript flares;
		public MissileFireScript missiles;
		public LaserScript lasers;

		private PlayerShipPosition[] otherShipPositions;
		private Vector3[] otherShipScreenSpacePositions;

		public override void ActionStart()
		{
			SceneManager.SendMessageToAction(this, "DeathMatchAction", "get player_number");
			SceneManager.SendMessageToAction(this, "DeathMatchAction", "get controller_number");
			SceneManager.SendMessageToAction(this, "DeathMatchAction", "get spawn_point");

            SpawnShip();

			displayGUI = false;

			localCameraPosition = new Vector3(playerCamera.transform.localPosition.x,
			                                  playerCamera.transform.localPosition.y,
			                                  playerCamera.transform.localPosition.z);

			inputHandler = InputHandlerHolder.GetDirectInputHandler(controllerNumber);
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
		}

        private void SpawnShip()
        {
            shipGameObject = Instantiate(shipPrefab, spawnLocation, spawnDirection) as GameObject;

            playerShip = shipGameObject.GetComponent<PlayerShip>();
            playerShip.PassControllingActionName(Name);

            playerCamera = playerShip.GetComponentInChildren<Camera>();

			playerShip.PlayerNumber = playerNumber;

            SetupScreenValues(DataManager.GetNumberPlayers());

			SceneManager.SendMessageToAction(this, "DeathMatchAction", "spawn " + playerNumber);
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

        private const float DEATH_ANIMATION_LENGTH = 3f;
        private bool deathAnimation = false;
        public AnimationCurve zoomDistance;
        private float deathTime = 0f;
        private Vector3 collisionPoint;
        private Vector3 collisionDistancedPoint;

        public GameObject explosionPrefab;
        public GameObject explosionGameObject;

		private bool displayGUI;

		public override void ActionUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Alpha0))
				controller = false;

            if (deathAnimation)
            {
                playerCamera.transform.position = Vector3.Lerp(collisionPoint, collisionDistancedPoint, zoomDistance.Evaluate(deathTime / DEATH_ANIMATION_LENGTH));
                playerCamera.transform.LookAt(collisionPoint);

                if (deathTime > DEATH_ANIMATION_LENGTH)
                {
                    deathAnimation = false;
                    DestroyObject(playerCamera.gameObject);
                    DestroyObject(explosionGameObject);

                    throttleOutput = throttlePrecise = 0f;

                    SpawnShip();

					if (controlsEnabled)
						displayGUI = true;

                    deathAnimation = false;
                }

                deathTime += Time.deltaTime;
            }
            else if (controlsEnabled)
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

			if (inputHandler.GetButtonDown("Back_Button"))
			{
				SceneManager.SendMessageToAction(this, "DeathMatchAction", "show_scoreboard " + playerNumber);
				if (controlsEnabled && !deathAnimation)
					displayGUI = false;
			}

			if (inputHandler.GetButtonUp("Back_Button"))
			{
				SceneManager.SendMessageToAction(this, "DeathMatchAction", "hide_scoreboard " + playerNumber);
				if (controlsEnabled && !deathAnimation)
					displayGUI = true;
			}
			if (inputHandler.GetButtonDown ("Left_Bumper")) {
				flares.Fire(playerShip.transform, playerShip.rigidbody.velocity);
			}

			for (int n = 0; n < otherShipPositions.Length; ++n)
			{
				otherShipScreenSpacePositions[n] = playerCamera.WorldToScreenPoint(otherShipPositions[n].position);
			}

			if (inputHandler.GetButtonDown ("Right_Bumper")) {
				missiles.Fire(playerShip.transform, playerShip.rigidbody.velocity);
			}
			if (inputHandler.GetAxis("Right_Trigger") > 0.5f) {
				lasers.Fire(playerShip.transform);
			}
		}
		
		public override void ActionFixedUpdate()
		{
			// do nothing
		}

		public override void ActionOnGUI()
		{
//            for (float i=0; i<playerShip.Speed/50; i++) {
//                if (i > 0 && i < 5) { pinch += 5; }
//                else if (i > 7) { pinch -= 5; }
//                else if (i == 0) { pinch = 0; }
//                else { pinch = 25;}
//                GUI.Box (nyew Rect(Screen.width - (barHeight - pinch/2), Screen.height - (barSpace*i + barWidth), barHeight - pinch, barWidth), image);
//            }

			if (displayGUI)
			{
				float markerHeight = (float)(screenHeight - screenVerticalStep + screenTopStart) + (float)(screenVerticalStep * 0.66f) - (float)(throttleOutput * (screenHeight / 8));
				float zeroMarker = (float)(screenHeight - screenVerticalStep + screenTopStart) + (float)(screenVerticalStep * 0.56f);
				float numEdge = (float)screenWidth * 0.005f + screenLeftStart;

				GUI.DrawTexture(new Rect(screenLeftStart, screenHeight - screenVerticalStep + screenTopStart, screenHorizontalStep, screenVerticalStep), throttleBackdrop);
				GUI.DrawTexture(new Rect(screenLeftStart, (int)markerHeight, screenHorizontalStep, screenVerticalStep / 24), throttleMarker);
				GUI.DrawTexture(new Rect(screenLeftStart, screenHeight - screenVerticalStep + screenTopStart, screenHorizontalStep, screenVerticalStep), throttleOverlay);

				GUI.Label(new Rect(numEdge, zeroMarker, screenHorizontalStep, screenVerticalStep), "0", throttleGuiStyle);
				GUI.Label(new Rect(numEdge * 1.001f, zeroMarker - (screenHeight / 8), screenHorizontalStep, screenVerticalStep), "1", throttleGuiStyle);

				GUI.DrawTexture(new Rect(screenLeftStart + screenWidth - screenWidth / 8,
				                         screenTopStart + screenHeight - screenHeight / 8,
				                         screenWidth / 8,
				                         screenHeight / 8),
				                speedometerTexture, ScaleMode.StretchToFill);
				GUI.Label(new Rect(screenLeftStart + screenWidth - screenWidth / 8,
				                   screenTopStart + screenHeight - screenHeight / 8,
				                   screenWidth / 9,
				                   screenHeight / 8),
				          Mathf.RoundToInt(playerShip.Speed).ToString(), speedometerGuiStyle);

				GUI.DrawTexture(new Rect(screenLeftStart + screenWidthInternalOffset + hudScreenWidth / 2 - crossHairTextureWidth / 2,
				                         screenTopStart + hudScreenHeight / 2 - crossHairTextureHeight / 2,
				                         crossHairTextureWidth,
				                         crossHairTextureHeight),
				                crossHairTexture, ScaleMode.StretchToFill);

				GUI.DrawTexture(new Rect(screenLeftStart + screenWidthInternalOffset + hudScreenWidth / 2 - laserLockOnTextureWidth / 2,
				                         screenTopStart + hudScreenHeight / 2 - laserLockOnTextureHeight / 2,
				                         laserLockOnTextureWidth,
				                         laserLockOnTextureHeight),
				                laserLockOnCircleTexture, ScaleMode.StretchToFill);

				GUI.DrawTexture(new Rect(screenLeftStart + screenWidthInternalOffset + hudScreenWidth / 2 - missileLockOnTextureWidth / 2,
				                         screenTopStart + hudScreenHeight / 2 - missileLockOnTextureHeight / 2,
				                         missileLockOnTextureWidth,
				                         missileLockOnTextureHeight),
				                missileLockOnCircleTexture, ScaleMode.StretchToFill);

				for (int n = 0; n < otherShipScreenSpacePositions.Length; ++n)
				{
					GUI.DrawTexture(new Rect(/*screenLeftStart + */otherShipScreenSpacePositions[n].x - playerIconTexture.width / 2,
					                         /*screenTopStart + */Screen.height - otherShipScreenSpacePositions[n].y - playerIconTexture.height / 2,
					                         playerIconTexture.width / 2,
					                         playerIconTexture.height / 2),
					                playerIconTexture, ScaleMode.StretchToFill);
				}
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
				displayGUI = true;
				controlsEnabled = true;
				break;
			case "disable_controls":
				displayGUI = false;
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
            case "event":
                switch (messageTokens[1])
                {
                case "crash":
                    SetupDeathAnimation();
                    SceneManager.SendMessageToAction(this, "DeathMatchAction", "death " + playerNumber);
                    break;
                }
                break;
			case "pass":
				switch (messageTokens[1])
				{
				case "other_ships":
					otherShipPositions = ((DeathMatchAction)action).GetOtherShipPositionReferences(playerNumber);
					otherShipScreenSpacePositions = new Vector3[otherShipPositions.Length];
					break;
				}
				break;
			}
		}

		public int PlayerNumber
		{
			get { return playerNumber; }
			set { playerNumber = value; }
		}

		public int ControllerNumber
		{
			get { return controllerNumber; }
			set { controllerNumber = value; }
		}

		public void PassSpawnPoint(Vector3 location, Quaternion direction)
		{
			spawnLocation = location;
			spawnDirection = direction;
//			Debug.Log(spawnLocation + " | " + spawnDirection);
		}

		public Transform GetShipTransform()
		{
			return playerShip.transform;
		}

        public void SetupDeathAnimation()
        {
            playerCamera.transform.parent = null;
            deathTime = 0f;
            deathAnimation = true;
			displayGUI = false;
            collisionPoint = playerShip.GetCollisionPoint();
            collisionDistancedPoint = playerShip.GetCollisionNormal() * 75f + collisionPoint;

            DestroyObject(playerShip.gameObject);
            explosionGameObject = Instantiate(explosionPrefab, collisionPoint, Quaternion.Euler(playerShip.GetCollisionNormal())) as GameObject;
        }

		private int screenWidth;
		private int screenHeight;
		private int screenLeftStart;
		private int screenTopStart;
		private int screenHorizontalStep;
		private int screenVerticalStep;

		private int screenWidthInternalOffset;
		private int hudScreenWidth;
		private int hudScreenHeight;
		private int crossHairTextureWidth;
		private int crossHairTextureHeight;
		private int laserLockOnTextureWidth;
		private int laserLockOnTextureHeight;
		private int missileLockOnTextureWidth;
		private int missileLockOnTextureHeight;
		private int playerIconTextureWidth;
		private int playerIconTextureHeight;

		private float screenPixelWidth;
		private float screenPixelHeight;

		private void SetupScreenValues(int numberPlayers)
		{
			switch (numberPlayers)
			{
			case 1:
				screenWidth = hudScreenWidth = Screen.width;
				screenHeight = hudScreenHeight = Screen.height;
				screenLeftStart = 0;
				screenTopStart = 0;
				screenHorizontalStep = Screen.width / 28;
				screenVerticalStep = Screen.height / 16;
				screenWidthInternalOffset = 0;
				break;
			case 2:
				screenWidth = Screen.width;
				screenHeight = Screen.height / 2;
				hudScreenWidth = screenWidth / 2;
				hudScreenHeight = screenHeight;
				screenHorizontalStep = Screen.width / 28;
				screenVerticalStep = Screen.height / 8;
				screenLeftStart = 0;
				screenWidthInternalOffset = Screen.width / 4;
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
				screenWidth = hudScreenWidth = Screen.width / 2;
				screenHeight = hudScreenHeight = Screen.height / 2;
				screenHorizontalStep = Screen.width / 20;
				screenVerticalStep = Screen.height / 8;
				screenWidthInternalOffset = 0;
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
				screenWidth = hudScreenWidth = Screen.width / 2;
				screenHeight = hudScreenHeight = Screen.height / 2;
				screenHorizontalStep = Screen.width / 20;
				screenVerticalStep = Screen.height / 8;
				screenWidthInternalOffset = 0;
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

			screenPixelWidth = playerCamera.pixelWidth;
			screenPixelHeight = playerCamera.pixelHeight;

            throttleGuiStyle.fontSize = (int)(screenHeight / 720f * 28);
            speedometerGuiStyle.fontSize = (int)(screenHeight / 720f * 72);

			crossHairTextureWidth = (int)(hudScreenWidth / 1280f * 16);
			crossHairTextureHeight = (int)(hudScreenHeight / 720f * 16);

			laserLockOnTextureWidth = (int)(hudScreenWidth / 1280f * 128);
			laserLockOnTextureHeight = (int)(hudScreenHeight / 720f * 128);

			missileLockOnTextureWidth = (int)(hudScreenWidth / 1280f * 256);
			missileLockOnTextureHeight = (int)(hudScreenHeight / 720f * 256);

			playerIconTextureWidth = (int)(hudScreenWidth / 1280f * 16);
			playerIconTextureHeight = (int)(hudScreenHeight / 720f * 16);;
		}
	}
}
