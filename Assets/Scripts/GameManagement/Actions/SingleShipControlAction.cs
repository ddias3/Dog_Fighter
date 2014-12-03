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

        public Texture2D laserLockOnCircleRedTexture;
        public Texture2D missileLockOnCircleRedTexture;

		public Texture2D playerIconTexture;
        public Texture2D playerIconRedTexture;
		public Texture2D playerIconEdgeTexture;

		public Texture2D healthBackground;
		public Texture2D goodHealth;
		public Texture2D okHealth;
		public Texture2D badHealth;
		public Texture2D HealthColor;

		public Texture2D weaponBackground;
		public Texture2D rocketIcon;
		public Texture2D laserIcon;
		public Texture2D flareIcon;
		public Texture2D readyBar;
		public Texture2D waitBar;

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

		public GUIStyle shipIconNameGuiStyle;
		public GUIStyle shipIconDistanceGuiStyle;

		public FlareScript flares;
		public MissileFireScript missiles;
		public LaserScript lasers;

		private PlayerShipPosition[] otherShipPositions;
		private Vector3[] otherShipScreenSpacePositions;
		private bool[] otherShipDrawIconNormal;
		private Vector3[] otherShipDrawEdgeIconPositions;
		private float[] otherShipDrawIconEdgeAngles;
        private LockOnDataWrapper[] otherShipLockOnDataWrappers;

        private bool anyLaserLockOn = false;
        private bool anyMissileLockOn = false;

		private string[] shipIconDistanceString;
		private string[] shipIconNameString;

        private const int MISSILE_LOCK_ON_SCREEN_SIZE = 256;
        private const int LASER_LOCK_ON_SCREEN_SIZE = 128;

        private const float LASER_LOCK_ON_TIME = 0.5f;
        private const float MISSILE_LOCK_ON_TIME = 1.5f;
        private const float LASER_LOSE_LOCK_COMPLETE_TIME = 1f;
        private const float MISSILE_LOSE_LOCK_COMPLETE_TIME = 3f;
        private float INVERSE_LASER_LOCK_ON_TIME;
        private float INVERSE_MISSILE_LOCK_ON_TIME;
        private float INVERSE_LASER_LOSE_LOCK_COMPLETE_TIME;
        private float INVERSE_MISSILE_LOSE_LOCK_COMPLETE_TIME;

		public override void ActionStart()
		{
            INVERSE_LASER_LOCK_ON_TIME = 1f / LASER_LOCK_ON_TIME;
            INVERSE_MISSILE_LOCK_ON_TIME = 1f / MISSILE_LOCK_ON_TIME;
            INVERSE_LASER_LOSE_LOCK_COMPLETE_TIME = 1f / LASER_LOSE_LOCK_COMPLETE_TIME;
            INVERSE_MISSILE_LOSE_LOCK_COMPLETE_TIME = 1f / MISSILE_LOSE_LOCK_COMPLETE_TIME;

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
        private GameObject explosionGameObject;

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

                if (inputHandler.GetButtonDown ("Left_Bumper")) {
                    flares.Fire(playerShip.transform, playerShip.rigidbody.velocity);
                }

                if (inputHandler.GetButtonDown ("Right_Bumper")) {
                    missiles.SetTarget(GetMissileLockedOnShipTransform());
                    missiles.Fire(playerShip.transform, playerShip.rigidbody.velocity);
                }
                if (inputHandler.GetAxis("Right_Trigger") > 0.5f) {
                    lasers.SetTarget(GetLaserLockedOnShipTransform());
                    lasers.Fire(playerShip.transform);
                }
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

			for (int n = 0; n < otherShipPositions.Length; ++n)
			{
				otherShipScreenSpacePositions[n] = playerCamera.WorldToScreenPoint(otherShipPositions[n].position);
				otherShipScreenSpacePositions[n].y = Screen.height - otherShipScreenSpacePositions[n].y;

				if (null != playerShip)
				{
					int distance = (int)((playerShip.transform.position - otherShipPositions[n].position).magnitude);
					if (distance > 1000)
						shipIconDistanceString[n] = (distance / 1000).ToString() + "." + (distance % 1000 / 100).ToString() + "km"; 
					else
						shipIconDistanceString[n] = distance.ToString() + "m";
				}

				if (otherShipScreenSpacePositions[n].z < 0 ||
				    otherShipScreenSpacePositions[n].x < screenLeftStart ||
				    otherShipScreenSpacePositions[n].x > screenLeftStart + screenWidth ||
				    otherShipScreenSpacePositions[n].y < screenTopStart ||
				    otherShipScreenSpacePositions[n].y > screenTopStart + screenHeight)
				{
					otherShipDrawIconNormal[n] = false;
					//TODO: FIX THIS SECTION.
//					if (otherShipScreenSpacePositions[n].z > 0)
//					{
//						Vector3 globalDirection = otherShipPositions[n].position - (Vector3.Dot(otherShipPositions[n].position, playerShip.transform.forward) * playerShip.transform.forward);
//						Vector3 localDirection = playerShip.transform.InverseTransformDirection(globalDirection);
//						localDirection.Normalize();
//
//						otherShipDrawEdgeIconPositions[n] = localDirection * (hudScreenWidth * 0.9f);
//
//						otherShipDrawIconEdgeAngles[n] = Mathf.Atan((float)(otherShipDrawEdgeIconPositions[n].y) / otherShipDrawEdgeIconPositions[n].x) * 57.2957795f;
//					}
//					else
//					{
//						Vector2 direction = new Vector2(otherShipScreenSpacePositions[n].x,
//						                                otherShipScreenSpacePositions[n].y);
//						direction.Normalize();
//						
//						otherShipDrawEdgeIconPositions[n] = direction * (hudScreenWidth * 0.9f);
//
//						otherShipDrawIconEdgeAngles[n] = Mathf.Atan((float)(otherShipDrawEdgeIconPositions[n].y) / otherShipDrawEdgeIconPositions[n].x) * 57.2957795f;
//
//						otherShipDrawEdgeIconPositions[n].x += screenLeftStart;
//						otherShipDrawEdgeIconPositions[n].y += screenTopStart;
//					}
				}
				else
				{
					otherShipDrawIconNormal[n] = true;
				}
			}

            anyLaserLockOn = false;
            anyMissileLockOn = false;
            for (int n = 0; n < otherShipScreenSpacePositions.Length; ++n)
            {
                if (otherShipPositions[n].active && null != playerShip)
                {
                    Vector2 localScreenMid = new Vector2(screenLeftStart + screenWidth / 2, screenTopStart + screenHeight / 2);

                    Vector2 positionToMid = new Vector2(otherShipScreenSpacePositions[n].x - localScreenMid.x,
                                                          otherShipScreenSpacePositions[n].y - localScreenMid.y);

                    otherShipLockOnDataWrappers[n].distanceFromMid = positionToMid.magnitude;

                    if (otherShipScreenSpacePositions[n].z > 0 &&
                        (otherShipPositions[n].position - playerShip.transform.position).sqrMagnitude < (2500 * 2500) &&
                        otherShipLockOnDataWrappers[n].distanceFromMid < laserLockOnTextureWidth * 0.5f)
                    {
                        otherShipLockOnDataWrappers[n].laserLockOn += INVERSE_LASER_LOCK_ON_TIME * Time.deltaTime;

                        if (otherShipLockOnDataWrappers[n].laserLockOn >= 1f)
                        {
                            otherShipLockOnDataWrappers[n].laserLockOnReady = true;
                            otherShipLockOnDataWrappers[n].laserLockOn = 1f;
                            anyLaserLockOn = true;
                        }
                    }
                    else
                    {
                        otherShipLockOnDataWrappers[n].laserLockOnReady = false;

                        otherShipLockOnDataWrappers[n].laserLockOn -= INVERSE_LASER_LOSE_LOCK_COMPLETE_TIME * Time.deltaTime;

                        if (otherShipLockOnDataWrappers[n].laserLockOn <= 0f)
                        {
                            otherShipLockOnDataWrappers[n].laserLockOnReady = false;
                            otherShipLockOnDataWrappers[n].laserLockOn = 0f;
                        }
                    }

                    if (otherShipScreenSpacePositions[n].z > 0 &&
                        (otherShipPositions[n].position - playerShip.transform.position).sqrMagnitude < (4000 * 4000) &&
                        otherShipLockOnDataWrappers[n].distanceFromMid < missileLockOnTextureWidth * 0.5f)
                    {
                        otherShipLockOnDataWrappers[n].missileLockOn += INVERSE_MISSILE_LOCK_ON_TIME * Time.deltaTime;

                        if (otherShipLockOnDataWrappers[n].missileLockOn >= 1f)
                        {
                            otherShipLockOnDataWrappers[n].missileLockOnReady = true;
                            otherShipLockOnDataWrappers[n].missileLockOn = 1f;
                            anyMissileLockOn = true;
                        }
                    }
                    else
                    {
                        otherShipLockOnDataWrappers[n].missileLockOnReady = false;
                        
                        otherShipLockOnDataWrappers[n].missileLockOn -= INVERSE_MISSILE_LOSE_LOCK_COMPLETE_TIME * Time.deltaTime;
                        
                        if (otherShipLockOnDataWrappers[n].missileLockOn <= 0f)
                        {
                            otherShipLockOnDataWrappers[n].missileLockOnReady = false;
                            otherShipLockOnDataWrappers[n].missileLockOn = 0f;
                        }
                    }
                }
                else
                {
                    otherShipLockOnDataWrappers[n].distanceFromMid = 0f;
                    otherShipLockOnDataWrappers[n].laserLockOn = 0f;
                    otherShipLockOnDataWrappers[n].laserLockOnReady = false;
                    otherShipLockOnDataWrappers[n].missileLockOn = 0f;
                    otherShipLockOnDataWrappers[n].missileLockOnReady = false;
                }
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
				//offset valus for throttle gui
				float markerHeight = (float)(hudScreenHeight - (hudScreenHeight / 5) + screenTopStart) + (((float)hudScreenHeight / 5) * 0.66f) - (float)(throttleOutput * (hudScreenHeight / 10));
				float zeroMarker = (float)(hudScreenHeight - (hudScreenHeight / 5) + screenTopStart) + (((float)hudScreenHeight / 5) * 0.58f);
				float numEdge = (float)screenWidth * 0.005f + screenLeftStart;

				//throttle gui
				GUI.DrawTexture(new Rect(screenLeftStart, hudScreenHeight - (hudScreenHeight / 5) + screenTopStart, hudScreenWidth / 12, hudScreenHeight / 5),
				                throttleBackdrop, ScaleMode.StretchToFill);
				GUI.DrawTexture(new Rect(screenLeftStart, (int)markerHeight, hudScreenWidth / 12, hudScreenHeight / 64),
				                throttleMarker, ScaleMode.StretchToFill);
				GUI.DrawTexture(new Rect(screenLeftStart, hudScreenHeight - (hudScreenHeight / 5) + screenTopStart, hudScreenWidth / 12, hudScreenHeight / 5),
				                throttleOverlay, ScaleMode.StretchToFill);

				GUI.Label(new Rect(numEdge, zeroMarker, screenHorizontalStep, screenVerticalStep), "0", throttleGuiStyle);
				GUI.Label(new Rect(numEdge * 1.001f, zeroMarker - (hudScreenHeight / 10), screenHorizontalStep, screenVerticalStep), "1", throttleGuiStyle);

				//speedometer gui
				GUI.DrawTexture(new Rect(screenLeftStart + screenWidth - hudScreenWidth / 8,
				                         screenTopStart + screenHeight - hudScreenHeight / 8,
				                         hudScreenWidth / 8,
				                         hudScreenHeight / 8),
				                speedometerTexture, ScaleMode.StretchToFill);
				GUI.Label(new Rect(screenLeftStart + screenWidth - hudScreenWidth / 8,
				                   screenTopStart + screenHeight - hudScreenHeight / 8,
				                   hudScreenWidth / 9,
				                   hudScreenHeight / 8),
				          Mathf.RoundToInt(playerShip.Speed).ToString(), speedometerGuiStyle);

				//crosshair gui
				GUI.DrawTexture(new Rect(screenLeftStart + screenWidthInternalOffset + hudScreenWidth / 2 - crossHairTextureWidth / 2,
				                         screenTopStart + hudScreenHeight / 2 - crossHairTextureHeight / 2,
				                         crossHairTextureWidth,
				                         crossHairTextureHeight),
				                crossHairTexture, ScaleMode.StretchToFill);

                if (anyLaserLockOn)
                    GUI.DrawTexture(new Rect(screenLeftStart + screenWidthInternalOffset + hudScreenWidth / 2 - laserLockOnTextureWidth / 2,
                                             screenTopStart + hudScreenHeight / 2 - laserLockOnTextureHeight / 2,
                                             laserLockOnTextureWidth,
                                             laserLockOnTextureHeight),
                                    laserLockOnCircleRedTexture, ScaleMode.StretchToFill);
                else
    				GUI.DrawTexture(new Rect(screenLeftStart + screenWidthInternalOffset + hudScreenWidth / 2 - laserLockOnTextureWidth / 2,
    				                         screenTopStart + hudScreenHeight / 2 - laserLockOnTextureHeight / 2,
    				                         laserLockOnTextureWidth,
    				                         laserLockOnTextureHeight),
    				                laserLockOnCircleTexture, ScaleMode.StretchToFill);

                if (anyMissileLockOn)
    				GUI.DrawTexture(new Rect(screenLeftStart + screenWidthInternalOffset + hudScreenWidth / 2 - missileLockOnTextureWidth / 2,
    				                         screenTopStart + hudScreenHeight / 2 - missileLockOnTextureHeight / 2,
    				                         missileLockOnTextureWidth,
    				                         missileLockOnTextureHeight),
    				                missileLockOnCircleRedTexture, ScaleMode.StretchToFill);
                else
                    GUI.DrawTexture(new Rect(screenLeftStart + screenWidthInternalOffset + hudScreenWidth / 2 - missileLockOnTextureWidth / 2,
                                             screenTopStart + hudScreenHeight / 2 - missileLockOnTextureHeight / 2,
                                             missileLockOnTextureWidth,
                                             missileLockOnTextureHeight),
                                    missileLockOnCircleTexture, ScaleMode.StretchToFill);

				for (int n = 0; n < otherShipScreenSpacePositions.Length; ++n)
				{
					if (otherShipDrawIconNormal[n])
					{
                        if (otherShipLockOnDataWrappers[n].laserLockOnReady || otherShipLockOnDataWrappers[n].missileLockOnReady)
                        {
    						GUI.DrawTexture(new Rect(otherShipScreenSpacePositions[n].x - playerIconTextureWidth / 2,
    						                         otherShipScreenSpacePositions[n].y - playerIconTextureHeight / 2,
    						                         playerIconTextureWidth,
    						                         playerIconTextureHeight),
    						                playerIconRedTexture, ScaleMode.StretchToFill);
                        }
                        else
                        {
                            GUI.DrawTexture(new Rect(otherShipScreenSpacePositions[n].x - playerIconTextureWidth / 2,
                                                     otherShipScreenSpacePositions[n].y - playerIconTextureHeight / 2,
                                                     playerIconTextureWidth,
                                                     playerIconTextureHeight),
                                            playerIconTexture, ScaleMode.StretchToFill);
                        }

						GUI.Label(new Rect(otherShipScreenSpacePositions[n].x - playerIconTextureWidth / 2,
						                   otherShipScreenSpacePositions[n].y - 1.5f * playerIconTextureHeight,
						                   screenWidth,
						                   screenHeight),
						          shipIconNameString[n], shipIconNameGuiStyle);
						GUI.Label(new Rect(otherShipScreenSpacePositions[n].x + playerIconTextureWidth,
						                   otherShipScreenSpacePositions[n].y,
						                   screenWidth,
						                   screenHeight),
						          shipIconDistanceString[n], shipIconDistanceGuiStyle);
					}
					else
					{
						//TODO: FIX THIS SECTION.
//						Matrix4x4 matrixBackup = GUI.matrix;
//						GUIUtility.RotateAroundPivot(otherShipDrawIconEdgeAngles[n], new Vector2(screenLeftStart + screenWidth / 2, screenTopStart + screenHeight / 2));
//						GUI.DrawTexture(new Rect(screenWidth + otherShipDrawEdgeIconPositions[n].x - playerIconEdgeTextureWidth,
//						                         screenHeight + otherShipDrawEdgeIconPositions[n].y - playerIconEdgeTextureHeight,
//						                         playerIconEdgeTextureWidth,
//						                         playerIconEdgeTextureHeight),
//						                playerIconEdgeTexture, ScaleMode.StretchToFill);
//						GUI.matrix = matrixBackup;
//
//						GUI.Label(new Rect(screenWidth + otherShipDrawEdgeIconPositions[n].x - playerIconTextureWidth / 2,
//						                   screenHeight + otherShipDrawEdgeIconPositions[n].y - 2 * playerIconTextureHeight,
//						                   screenWidth,
//						                   screenHeight),
//						          shipIconNameString[n], shipIconNameGuiStyle);
//						GUI.Label(new Rect(screenWidth + otherShipDrawEdgeIconPositions[n].x + playerIconTextureWidth,
//						                   screenHeight + otherShipDrawEdgeIconPositions[n].y,
//						                   screenWidth,
//						                   screenHeight),
//						          shipIconDistanceString[n], shipIconDistanceGuiStyle);
					}

//                    if (otherShipLockOnDataWrappers[n].laserLockOnReady)
//                        GUI.Label(new Rect(screenLeftStart + 80 * n, screenTopStart, 500, 100), "lLoc: On");
//                    else
//                        GUI.Label(new Rect(screenLeftStart + 80 * n, screenTopStart, 500, 100), "lLoc: Off");
//
//                    GUI.Label(new Rect(screenLeftStart + 80 * n, screenTopStart + 20, 500, 100), "lLoc#: " + otherShipLockOnDataWrappers[n].laserLockOn);
//
//                    if (otherShipLockOnDataWrappers[n].missileLockOnReady)
//                        GUI.Label(new Rect(screenLeftStart + 80 * n, screenTopStart + 40, 500, 100), "mLoc: On");
//                    else
//                        GUI.Label(new Rect(screenLeftStart + 80 * n, screenTopStart + 40, 500, 100), "mLoc: Off");
//
//                    GUI.Label(new Rect(screenLeftStart + 80 * n, screenTopStart + 60, 500, 100), "mLoc#: " + otherShipLockOnDataWrappers[n].missileLockOn);
				}

				//health gui
				GUI.DrawTexture(new Rect(screenLeftStart, screenTopStart, hudScreenWidth/5, hudScreenHeight/15),
				                healthBackground, ScaleMode.StretchToFill);

				float health = playerShip.ShipHealth;
				float barOffset = hudScreenHeight/45;
				float barLength = (((hudScreenWidth/5) - (barOffset*2))/100) * health;

				if(health > 50)
					HealthColor = goodHealth;
				else if (health > 20)
					HealthColor = okHealth;
				else
					HealthColor = badHealth;

				GUI.DrawTexture(new Rect(screenLeftStart + barOffset, screenTopStart + barOffset, barLength, barOffset),
				                		 HealthColor, ScaleMode.StretchToFill);


				//weapons gui
				GUI.DrawTexture(new Rect(screenLeftStart + screenWidth - ((float)hudScreenWidth * 0.125f), screenTopStart, hudScreenWidth/8, hudScreenHeight/8),
				                weaponBackground, ScaleMode.StretchToFill);

				float segment = (((float)hudScreenHeight)/8)/3;
				float iconOffset = segment/5;
				float iconSize = segment - (iconOffset*2);
                float leftWeaponWall = screenLeftStart + screenWidth - ((float)hudScreenWidth * 0.125f) + iconOffset;

				GUI.DrawTexture(new Rect(leftWeaponWall, screenTopStart + iconOffset, iconSize, iconSize),
				                rocketIcon, ScaleMode.StretchToFill);
				GUI.DrawTexture(new Rect(leftWeaponWall, screenTopStart + segment + iconOffset, iconSize, iconSize),
				                laserIcon, ScaleMode.StretchToFill);
				GUI.DrawTexture(new Rect(leftWeaponWall, screenTopStart + segment*2 + iconOffset, iconSize, iconSize),
				                flareIcon, ScaleMode.StretchToFill);

				float leftChargeWall = leftWeaponWall + iconSize + iconOffset;
				float fullChargeWidth = ((float)hudScreenWidth/8) - (iconOffset*4) - iconSize;
				float chargeOffset = iconOffset*1.01f;
				float chargeHeight = iconSize - chargeOffset;
				float rocketCharge = missiles.getCharge();
				float laserCharge = lasers.getCharge();
				float flareCharge = flares.getCharge();

				GUI.DrawTexture(new Rect(leftChargeWall, screenTopStart + chargeOffset, fullChargeWidth*rocketCharge, chargeHeight),
				                readyBar, ScaleMode.StretchToFill);
				GUI.DrawTexture(new Rect(leftChargeWall, screenTopStart + chargeOffset + segment, fullChargeWidth*laserCharge, chargeHeight),
				                readyBar, ScaleMode.StretchToFill);
				GUI.DrawTexture(new Rect(leftChargeWall, screenTopStart + chargeOffset + segment*2, fullChargeWidth*flareCharge, chargeHeight),
				                readyBar, ScaleMode.StretchToFill);

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
					otherShipDrawIconNormal = new bool[otherShipPositions.Length];
					shipIconDistanceString = new string[otherShipPositions.Length];
					shipIconNameString = new string[otherShipPositions.Length];
					for (int n = 0; n < shipIconNameString.Length; ++n)
						shipIconNameString[n] = "P" + otherShipPositions[n].shipNumber;
					otherShipDrawEdgeIconPositions = new Vector3[otherShipPositions.Length];
					otherShipDrawIconEdgeAngles = new float[otherShipPositions.Length];
                    otherShipLockOnDataWrappers = new LockOnDataWrapper[otherShipPositions.Length];
                    for (int n = 0; n < otherShipLockOnDataWrappers.Length; ++n)
                        otherShipLockOnDataWrappers[n] = new LockOnDataWrapper(otherShipPositions[n]);
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
		private int playerIconEdgeTextureWidth;
		private int playerIconEdgeTextureHeight;

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
				screenHeight = hudScreenHeight = Screen.height / 2;
				hudScreenWidth = screenWidth / 2;
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

			shipIconNameGuiStyle.fontSize = (int)(screenHeight / 720f * 32);
			shipIconDistanceGuiStyle.fontSize = (int)(screenHeight / 720f * 28);

			crossHairTextureWidth = (int)(hudScreenWidth / 1280f * 20);
			crossHairTextureHeight = (int)(hudScreenHeight / 720f * 20);

			laserLockOnTextureWidth = (int)(hudScreenWidth / 1280f * LASER_LOCK_ON_SCREEN_SIZE);
			laserLockOnTextureHeight = (int)(hudScreenHeight / 720f * LASER_LOCK_ON_SCREEN_SIZE);

			missileLockOnTextureWidth = (int)(hudScreenWidth / 1280f * MISSILE_LOCK_ON_SCREEN_SIZE);
			missileLockOnTextureHeight = (int)(hudScreenHeight / 720f * MISSILE_LOCK_ON_SCREEN_SIZE);

			playerIconEdgeTextureWidth = playerIconTextureWidth = (int)(hudScreenWidth / 1280f * 32);
			playerIconEdgeTextureHeight = playerIconTextureHeight = (int)(hudScreenHeight / 720f * 32);

		}

        private Transform laserLockedOnTransform;
        private Transform missileLockedOnTransform;
        private Transform lockedOnTransform;
        private Transform GetLaserLockedOnShipTransform()
        {
            if (otherShipPositions.Length == 0)
                return null;

            int outputIndex = -1;
            float closestDistance = Mathf.Infinity;
            for (int n = 0; n < otherShipPositions.Length; ++n)
            {
                if (otherShipLockOnDataWrappers[n].laserLockOnReady && 
                    otherShipLockOnDataWrappers[n].distanceFromMid < closestDistance)
                    outputIndex = n;
            }

            if (outputIndex == -1)
                return null;

            SceneManager.SendMessageToAction(this, "DeathMatchAction", "get transform " + otherShipPositions[outputIndex].shipNumber);
            return laserLockedOnTransform = lockedOnTransform;
        }

        private Transform GetMissileLockedOnShipTransform()
        {
            if (otherShipPositions.Length == 0)
                return null;
            
            int outputIndex = -1;
            float closestDistance = Mathf.Infinity;
            for (int n = 0; n < otherShipPositions.Length; ++n)
            {
                if (otherShipLockOnDataWrappers[n].missileLockOnReady && 
                    otherShipLockOnDataWrappers[n].distanceFromMid < closestDistance)
                    outputIndex = n;
            }
            
            if (outputIndex == -1)
                return null;
            
            SceneManager.SendMessageToAction(this, "DeathMatchAction", "get transform " + otherShipPositions[outputIndex].shipNumber);
            return missileLockedOnTransform = lockedOnTransform;
        }

        public void PassTransform(Transform lockedOnTransform)
        {
            this.lockedOnTransform = lockedOnTransform;
        }
        
        private sealed class LockOnDataWrapper
        {
            public PlayerShipPosition lockedOnShip;

            public float distanceFromMid;

            public float laserLockOn;
            public float missileLockOn;

            public bool laserLockOnReady;
            public bool missileLockOnReady;

            public LockOnDataWrapper(PlayerShipPosition shipPositionReference)
            {
                lockedOnShip = shipPositionReference;
                laserLockOn = missileLockOn = 0f;
                laserLockOnReady = missileLockOnReady = false;
            }
        }
	}
}
