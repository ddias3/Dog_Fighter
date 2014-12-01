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

		public GameObject emptyGameObjectPrefab;

		private int numberPlayers;

		private int gameModeState = 0;
		public float time;
		private const float INTRO_LENGTH = 7f;
		private float inverseIntroLength;
		private Transform[] cameraTransforms;
		private Transform[] cameraPivotTransforms;
		private Vector3[] originalCameraPositions;
		private Vector3[] farCameraPositions;
		private Quaternion[] originalCameraRotations;
		private Quaternion[] farCameraRotations;
		public AnimationCurve zoomDistance;
		public AnimationCurve revolvingDistance;

		public Camera extraCamera;
		public Texture timeBackgroundTexture;
		public Texture scoreboardBackgroundTexture;
		private bool showScoreboardInExtraScreen = false;
		private bool showScoreboard = false;
		private bool showScoreboardOverWholeScreen = false;
		private int endingState = 0;

		public override void ActionStart()
		{
			numberPlayers = DataManager.GetNumberPlayers();
			if (numberPlayers == 3)
			{
				extraCamera.enabled = true;
				extraCamera.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
				showScoreboardInExtraScreen = true;
			}

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

			cameraTransforms = new Transform[numberPlayers];
			cameraPivotTransforms = new Transform[numberPlayers];
			originalCameraPositions = new Vector3[numberPlayers];
			farCameraPositions = new Vector3[numberPlayers];
			originalCameraRotations = new Quaternion[numberPlayers];
			farCameraRotations = new Quaternion[numberPlayers];
			for (int n = 0; n < cameraTransforms.Length; ++n)
			{
				SceneManager.SendMessageToAction(this, "SingleShipControlAction_P" + (n+1), "get camera_transform");
//				originalCameraPositions[n] = new Vector3(cameraTransforms[n].localPosition.x,
//				                                         cameraTransforms[n].localPosition.y,
//				                                         cameraTransforms[n].localPosition.z);
				originalCameraPositions[n] = new Vector3(0, 0, 0);
				originalCameraRotations[n] = new Quaternion(cameraTransforms[n].rotation.x,
				                                            cameraTransforms[n].rotation.y,
				                                            cameraTransforms[n].rotation.z,
				                                            cameraTransforms[n].rotation.w);
				farCameraPositions[n] = Vector3.forward * -75f;
				farCameraRotations[n] = cameraTransforms[n].rotation * Quaternion.Euler(0, -170, 0);

				cameraTransforms[n].parent = null;
				cameraPivotTransforms[n] = (Instantiate(emptyGameObjectPrefab, cameraTransforms[n].position, cameraTransforms[n].rotation) as GameObject).GetComponent<Transform>();
				cameraTransforms[n].parent = cameraPivotTransforms[n];

				cameraTransforms[n].localPosition = farCameraPositions[n];
				cameraPivotTransforms[n].rotation = farCameraRotations[n];
			}

			time = 0f;
			gameModeState = 1;
			inverseIntroLength = 1f / INTRO_LENGTH;

			minutesLeft = gameModeLength = DataManager.GetGameLengthMinutes();
			secondsLeft = 0;
			if (secondsLeft < 10)
				secondsDisplay = "0" + secondsLeft.ToString();
			else
				secondsDisplay = secondsLeft.ToString();

			timeGuiStyle.fontSize = (int)(Screen.width / 1280f * 48);
			continueGuiStyle.fontSize = (int)(Screen.width / 1280f * 32);
			scoreboardGuiStyle.fontSize = (int)(Screen.width / 1280f * 32);
			endingGuiStyle.fontSize = (int)(Screen.width / 1280f * 156);

			showScoreboardInExtraScreen = false;
			showScoreboard = false;
			showScoreboardOverWholeScreen = false;
			endingState = 0;
		}

		private int gameModeLength;
		private int minutesLeft;
		private int secondsLeft;
		private string secondsDisplay;
		public override void ActionUpdate()
		{
			switch (gameModeState)
			{
			case 1:
				if (time > INTRO_LENGTH)
				{
					for (int n = 0; n < numberPlayers; ++n)
					{
						cameraTransforms[n].parent = null;
						SceneManager.SendMessageToAction(this, "SingleShipControlAction_P" + (n+1), "reset_camera");
						Destroy(cameraPivotTransforms[n].gameObject);

						SceneManager.SendMessageToAction(this, "SingleShipControlAction_P" + (n+1), "enable_controls");
					}

					time = 0;
					gameModeState = 2;
				}
				else
				{
					for (int n = 0; n < cameraTransforms.Length; ++n)
					{
						cameraTransforms[n].localPosition = Vector3.Lerp(farCameraPositions[n], originalCameraPositions[n], zoomDistance.Evaluate(time * inverseIntroLength));
						cameraPivotTransforms[n].rotation = Quaternion.Slerp(farCameraRotations[n], originalCameraRotations[n], revolvingDistance.Evaluate(time * inverseIntroLength));
					}
				}
				break;
			case 2:
			{
				int secondsPast = (int)time;
				minutesLeft = gameModeLength - (secondsPast / 60 + 1);
				secondsLeft = 59 - (secondsPast % 60);
				if (secondsLeft < 10)
					secondsDisplay = "0" + secondsLeft.ToString();
				else
					secondsDisplay = secondsLeft.ToString();

				if (time > gameModeLength * 60f)
				{
					gameModeState = 3;

					for (int n = 0; n < numberPlayers; ++n)
						SceneManager.SendMessageToAction(this, "SingleShipControlAction_P" + (n+1), "disable_controls");

					time = 0f;
				}
			}
				break;
			case 3:
				if (time < 5)
				{
					endingState = 1;
				}
				else if (time < 8)
				{
					endingState = 2;
					showScoreboardOverWholeScreen = true;
				}
				else
				{
					endingState = 3;
				}
				break;
			}

			time += Time.deltaTime;
		}
		
		public override void ActionFixedUpdate()
		{
			// do nothing
		}
		
		public GUIStyle timeGuiStyle;
		public GUIStyle scoreboardGuiStyle;
		public GUIStyle endingGuiStyle;
		public GUIStyle continueGuiStyle;

		public override void ActionOnGUI()
		{
			if (showScoreboardOverWholeScreen)
			{
				GUI.DrawTexture(new Rect(Screen.width / 24, Screen.height / 20, Screen.width - Screen.width / 12, Screen.height - Screen.height / 10), scoreboardBackgroundTexture);
			}

			switch (gameModeState)
			{
			case 1:
			case 2:
				switch (numberPlayers)
				{
				case 1:
					GUI.DrawTexture(new Rect(Screen.width / 2 - Screen.width / 20, 0, Screen.width / 10, Screen.height / 16), timeBackgroundTexture, ScaleMode.StretchToFill);
					GUI.Label(new Rect(0, Screen.height / 72, Screen.width, Screen.height / 36), minutesLeft.ToString() + ":" + secondsDisplay, timeGuiStyle);
					break;
				case 2:
				case 4:
					GUI.DrawTexture(new Rect(Screen.width / 2 - Screen.width / 20, Screen.height / 2 - Screen.height / 32, Screen.width / 10, Screen.height / 16), timeBackgroundTexture, ScaleMode.StretchToFill);
					GUI.Label(new Rect(0, 0, Screen.width, Screen.height), minutesLeft.ToString() + ":" + secondsDisplay, timeGuiStyle);
					break;
				case 3:
					GUI.DrawTexture(new Rect(Screen.width / 4 - Screen.width / 20, Screen.height / 2, Screen.width / 10, Screen.height / 16), timeBackgroundTexture, ScaleMode.StretchToFill);
					GUI.Label(new Rect(Screen.width / 2, Screen.height / 36 + Screen.height / 2, Screen.width / 2, Screen.height / 36), minutesLeft.ToString() + ":" + secondsDisplay, timeGuiStyle);
					break;
				}
				break;
			case 3:
				switch (endingState)
				{
				case 1:
					GUI.DrawTexture(new Rect(Screen.width / 4, Screen.height / 2 - Screen.height / 12, Screen.width / 2, Screen.height / 6), timeBackgroundTexture, ScaleMode.StretchToFill);
					GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "End Match", endingGuiStyle);
					break;
				case 3:
					GUI.Label(new Rect(0, Screen.height - Screen.height / 36, Screen.width / 2, Screen.height / 36), "Press Confirm to continue", continueGuiStyle);
					break;
				}
				break;
			}
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
			case "start_intro":
				gameModeState = 1;
				time = 0f;
				break;
			}
		}

		public GameObject GetPrefab(string prefabName)
		{
			return singleShipControlActionPrefab;
		}

		public void PassCameraTransform(int playerNumber, Transform cameraTransform)
		{
			cameraTransforms[playerNumber - 1] = cameraTransform;
		}
	}
}
