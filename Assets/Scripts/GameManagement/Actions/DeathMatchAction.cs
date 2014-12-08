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
		private ScoreboardDisplayDataWrapper extraScoreboardData;
		private bool[] showScoreboard;
		private bool showScoreboardOverWholeScreen = false;
		private int endingState = 0;

		private PlayerStatistics[] playerStats;
		private int[] scoreboardDisplayOrder;
		private ScoreboardDisplayDataWrapper[] scoreboardDisplayDataWrappers;

		private ControllerMenuInputHandler[] inputHandlers;
		private int[] controllerMap;

		private PlayerShipPosition[] allShipPositions;
		private Transform[] allShipTransforms;

		public override void ActionStart()
		{
			showScoreboardInExtraScreen = false;

            SceneManager.SendMessage(this, "run SoundPlayerAction");

//			DataManager.SetNumberPlayers(4);

			numberPlayers = 0;
			inputHandlers = new ControllerMenuInputHandler[4];
			controllerMap = new int[4];
			for (int n = 0; n < 4; ++n)
			{
				if (DataManager.GetPlayerPlaying(n + 1))
				{
					inputHandlers[numberPlayers] = InputHandlerHolder.GetMenuInputHandler(n + 1);
					controllerMap[numberPlayers] = n + 1;
					++numberPlayers;
				}
			}

			DataManager.SetNumberPlayers(numberPlayers);

			if (numberPlayers == 3)
			{
				extraCamera.enabled = true;
				extraCamera.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
//				showScoreboardInExtraScreen = true;
				extraScoreboardData = new ScoreboardDisplayDataWrapper(4, 4);
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

			showScoreboard = new bool[numberPlayers];
			for (int n = 0; n < numberPlayers; ++n)
			{
				showScoreboard[n] = false;
			}

			playerStats = new PlayerStatistics[numberPlayers];
			scoreboardDisplayOrder = new int[numberPlayers];
			scoreboardDisplayDataWrappers = new ScoreboardDisplayDataWrapper[numberPlayers];
			for (int n = 0; n < numberPlayers; ++n)
			{
				playerStats[n] = new PlayerStatistics("P" + (n+1));
				scoreboardDisplayOrder[n] = n;
				scoreboardDisplayDataWrappers[n] = new ScoreboardDisplayDataWrapper(numberPlayers, n + 1);
			}

			for (int n = 0; n < numberPlayers; ++n)
			{
				SceneManager.SendMessage(this, "instantiate_named_from_prefab SingleShipControlAction SingleShipControlAction_P" + (n + 1));
			}

			allShipPositions = new PlayerShipPosition[numberPlayers];
			allShipTransforms = new Transform[numberPlayers];
			PlayerShip[] allShips = FindObjectsOfType<PlayerShip>();
			for (int n = 0; n < numberPlayers; ++n)
				allShipTransforms[n] = allShips[n].gameObject.GetComponent<Transform>();

			for (int n = 0; n < numberPlayers; ++n)
			{
				allShipPositions[n] = ScriptableObject.CreateInstance<PlayerShipPosition>();
				allShipPositions[n].shipNumber = allShips[n].PlayerNumber;
				allShipPositions[n].active = true;
				allShipPositions[n].position = allShipTransforms[n].position;
			}

			for (int n = 0; n < numberPlayers; ++n)
			{
				SceneManager.SendMessageToAction(this, "SingleShipControlAction_P" + (n + 1), "pass other_ships");
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
			scoreboardGuiStyle.fontSize = (int)(Screen.width / 1280f * 64);
			if (numberPlayers > 1)
				smallerScoreboardGuiStyle.fontSize = (int)(Screen.width / 1280f * 32);
			else
				smallerScoreboardGuiStyle.fontSize = (int)(Screen.width / 1280f * 64);
			endingGuiStyle.fontSize = (int)(Screen.width / 1280f * 156);

			showScoreboardOverWholeScreen = false;
			endingState = 0;

			inputHandlers = InputHandlerHolder.GetMenuInputHandlers();
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
				UpdateShipPositionReferences();
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
						showScoreboard[n] = false;

					for (int n = 0; n < numberPlayers; ++n)
						SceneManager.SendMessageToAction(this, "SingleShipControlAction_P" + (n+1), "disable_controls");

					time = 0f;
				}
				UpdateShipPositionReferences();
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
					for (int n = 0; n < numberPlayers; ++n)
					{
						if (inputHandlers[n].GetButtonDown("Confirm_Button"))
						{
							DataManager.SetReturningFromGame(true);
							Application.LoadLevel("MenuScene");
						}
					}
				}
				break;
			}

			time += Time.deltaTime;
		}

		private void UpdateShipPositionReferences()
		{
			for (int n = 0; n < allShipPositions.Length; ++n)
			{
				if (allShipPositions[n].active)
				{
					allShipPositions[n].position = allShipTransforms[n].position;
				}
			}
		}
		
		public override void ActionFixedUpdate()
		{
			// do nothing
		}
		
		public GUIStyle timeGuiStyle;
		public GUIStyle scoreboardGuiStyle;
		public GUIStyle smallerScoreboardGuiStyle;
		public GUIStyle endingGuiStyle;
		public GUIStyle continueGuiStyle;

		public override void ActionOnGUI()
		{
			if (showScoreboardOverWholeScreen)
			{
				GUI.DrawTexture(new Rect(Screen.width / 24, Screen.height / 20, Screen.width - Screen.width / 12, Screen.height - Screen.height / 10), scoreboardBackgroundTexture);
				GUI.Label(new Rect(Screen.width / 6 + Screen.width / 5, Screen.height / 10, Screen.width / 16, Screen.height / 16), "Kills", scoreboardGuiStyle);
				GUI.Label(new Rect(Screen.width / 6 + 2 * Screen.width / 5, Screen.height / 10, Screen.width / 16, Screen.height / 16), "Assists", scoreboardGuiStyle);
				GUI.Label(new Rect(Screen.width / 6 + 3 * Screen.width / 5, Screen.height / 10, Screen.width / 16, Screen.height / 16), "Deaths", scoreboardGuiStyle);

				for (int n = 0; n < numberPlayers; ++n)
				{
					GUI.Label(new Rect(Screen.width / 6, Screen.height / 10 + (n + 1) * Screen.height / 7, Screen.width / 16, Screen.height / 16), playerStats[scoreboardDisplayOrder[n]].Name, scoreboardGuiStyle);
					GUI.Label(new Rect(Screen.width / 6 + Screen.width / 5, Screen.height / 10 + (n + 1) * Screen.height / 7, Screen.width / 16, Screen.height / 16), playerStats[scoreboardDisplayOrder[n]].Kills.ToString(), scoreboardGuiStyle);
					GUI.Label(new Rect(Screen.width / 6 + 2 * Screen.width / 5, Screen.height / 10 + (n + 1) * Screen.height / 7, Screen.width / 16, Screen.height / 16), playerStats[scoreboardDisplayOrder[n]].Assists.ToString(), scoreboardGuiStyle);
					GUI.Label(new Rect(Screen.width / 6 + 3 * Screen.width / 5, Screen.height / 10 + (n + 1) * Screen.height / 7, Screen.width / 16, Screen.height / 16), playerStats[scoreboardDisplayOrder[n]].Deaths.ToString(), scoreboardGuiStyle);
				}
			}
			else if (showScoreboardInExtraScreen)
			{
				ScoreboardDisplayDataWrapper dataWrapper = extraScoreboardData;

				GUI.DrawTexture(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 24,
				                         dataWrapper.screenTopStart + dataWrapper.screenHeight / 20,
				                         dataWrapper.screenWidth - dataWrapper.screenWidth / 12,
				                         dataWrapper.screenHeight - dataWrapper.screenHeight / 10),
				                scoreboardBackgroundTexture);
				GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + dataWrapper.screenWidth / 5,
				                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10,
				                   dataWrapper.screenWidth / 16,
				                   dataWrapper.screenWidth / 16),
				          "Kills", smallerScoreboardGuiStyle);
				GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + 2 * dataWrapper.screenWidth / 5,
				                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10,
				                   dataWrapper.screenWidth / 16,
				                   dataWrapper.screenHeight / 16),
				          "Assists", smallerScoreboardGuiStyle);
				GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + 3 * dataWrapper.screenWidth / 5,
				                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10,
				                   dataWrapper.screenWidth / 16,
				                   dataWrapper.screenHeight / 16),
				          "Deaths", smallerScoreboardGuiStyle);
				
				for (int n = 0; n < numberPlayers; ++n)
				{
					GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6,
					                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10 + (n + 1) * dataWrapper.screenHeight / 7,
					                   dataWrapper.screenWidth / 16,
					                   dataWrapper.screenHeight / 16),
					          playerStats[scoreboardDisplayOrder[n]].Name, smallerScoreboardGuiStyle);
					GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + dataWrapper.screenWidth / 5,
					                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10 + (n + 1) * dataWrapper.screenHeight / 7,
					                   dataWrapper.screenWidth / 16,
					                   dataWrapper.screenHeight / 16),
					          playerStats[scoreboardDisplayOrder[n]].Kills.ToString(), smallerScoreboardGuiStyle);
					GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + 2 * dataWrapper.screenWidth / 5,
					                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10 + (n + 1) * dataWrapper.screenHeight / 7,
					                   dataWrapper.screenWidth / 16,
					                   dataWrapper.screenHeight / 16),
					          playerStats[scoreboardDisplayOrder[n]].Assists.ToString(), smallerScoreboardGuiStyle);
					GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + 3 * dataWrapper.screenWidth / 5,
					                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10 + (n + 1) * dataWrapper.screenHeight / 7,
					                   dataWrapper.screenWidth / 16,
					                   dataWrapper.screenHeight / 16),
					          playerStats[scoreboardDisplayOrder[n]].Deaths.ToString(), smallerScoreboardGuiStyle);
				}
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
					GUI.DrawTexture(new Rect(Screen.width / 2 - Screen.width / 20, Screen.height / 2 - Screen.height / 32, Screen.width / 10, Screen.height / 16), timeBackgroundTexture, ScaleMode.StretchToFill);
					GUI.Label(new Rect(0, 0, Screen.width, Screen.height), minutesLeft.ToString() + ":" + secondsDisplay, timeGuiStyle);
					break;
				case 3:
					GUI.DrawTexture(new Rect(3 * Screen.width / 4 - Screen.width / 20, Screen.height / 2 + Screen.height / 128, Screen.width / 10, Screen.height / 14), timeBackgroundTexture, ScaleMode.StretchToFill);
					GUI.Label(new Rect(Screen.width / 2, Screen.height / 36 + Screen.height / 2, Screen.width / 2, Screen.height / 36), minutesLeft.ToString() + ":" + secondsDisplay, timeGuiStyle);
					break;
				case 4:
					GUI.DrawTexture(new Rect(3 * Screen.width / 4 - Screen.width / 20, Screen.height / 2 - Screen.height / 32, Screen.width / 10, Screen.height / 16), timeBackgroundTexture, ScaleMode.StretchToFill);
					GUI.Label(new Rect(Screen.width / 2, 0, Screen.width, Screen.height), minutesLeft.ToString() + ":" + secondsDisplay, timeGuiStyle);
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
					GUI.Label(new Rect(0, Screen.height - Screen.height / 6, Screen.width, Screen.height / 6), "Press Confirm to continue", continueGuiStyle);
					break;
				}
				break;
			}

			for (int playerIndex = 0; playerIndex < numberPlayers; ++playerIndex)
			{
				if (showScoreboard[playerIndex])
				{
					ScoreboardDisplayDataWrapper dataWrapper = scoreboardDisplayDataWrappers[playerIndex];

					GUI.DrawTexture(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 24,
					                         dataWrapper.screenTopStart + dataWrapper.screenHeight / 20,
					                         dataWrapper.screenWidth - dataWrapper.screenWidth / 12,
					                         dataWrapper.screenHeight - dataWrapper.screenHeight / 10),
					                scoreboardBackgroundTexture);
					GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + dataWrapper.screenWidth / 5,
					                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10,
					                   dataWrapper.screenWidth / 16,
					                   dataWrapper.screenWidth / 16),
					          "Kills", smallerScoreboardGuiStyle);
					GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + 2 * dataWrapper.screenWidth / 5,
					                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10,
					                   dataWrapper.screenWidth / 16,
					                   dataWrapper.screenHeight / 16),
					          "Assists", smallerScoreboardGuiStyle);
					GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + 3 * dataWrapper.screenWidth / 5,
					                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10,
					                   dataWrapper.screenWidth / 16,
					                   dataWrapper.screenHeight / 16),
					          "Deaths", smallerScoreboardGuiStyle);
					
					for (int n = 0; n < numberPlayers; ++n)
					{
						GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6,
						                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10 + (n + 1) * dataWrapper.screenHeight / 7,
						                   dataWrapper.screenWidth / 16,
						                   dataWrapper.screenHeight / 16),
						          playerStats[scoreboardDisplayOrder[n]].Name, smallerScoreboardGuiStyle);
						GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + dataWrapper.screenWidth / 5,
						                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10 + (n + 1) * dataWrapper.screenHeight / 7,
						                   dataWrapper.screenWidth / 16,
						                   dataWrapper.screenHeight / 16),
						          playerStats[scoreboardDisplayOrder[n]].Kills.ToString(), smallerScoreboardGuiStyle);
						GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + 2 * dataWrapper.screenWidth / 5,
						                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10 + (n + 1) * dataWrapper.screenHeight / 7,
						                   dataWrapper.screenWidth / 16,
						                   dataWrapper.screenHeight / 16),
						          playerStats[scoreboardDisplayOrder[n]].Assists.ToString(), smallerScoreboardGuiStyle);
						GUI.Label(new Rect(dataWrapper.screenLeftStart + dataWrapper.screenWidth / 6 + 3 * dataWrapper.screenWidth / 5,
						                   dataWrapper.screenTopStart + dataWrapper.screenHeight / 10 + (n + 1) * dataWrapper.screenHeight / 7,
						                   dataWrapper.screenWidth / 16,
						                   dataWrapper.screenHeight / 16),
						          playerStats[scoreboardDisplayOrder[n]].Deaths.ToString(), smallerScoreboardGuiStyle);
					}
				}
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
				case "random_spawn_point":
				{
					Transform transform = spawnPoints[Random.Range(0, spawnPoints.Length)];
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
				case "controller_number":
					((SingleShipControlAction)action).ControllerNumber = controllerMap[((SingleShipControlAction)action).PlayerNumber - 1];
					break;
                case "transform":
                {
                    int shipNumber = int.Parse(messageTokens[2]);
                    int index = 0;
                    for (; index < allShipPositions.Length; ++index)
                    {
                        if (allShipPositions[index].shipNumber == shipNumber)
                            break;
                    }
                    ((SingleShipControlAction)action).PassTransform(allShipTransforms[index]);
                }
                    break;
				}
				break;
			case "start_intro":
				gameModeState = 1;
				time = 0f;
				break;
			case "show_scoreboard":
				if (gameModeState == 2)
					showScoreboard[int.Parse(messageTokens[1]) - 1] = true;
				break;
			case "hide_scoreboard":
				if (gameModeState == 2)
					showScoreboard[int.Parse(messageTokens[1]) - 1] = false;
				break;
			case "kill":
			{
                if (gameModeState == 2)
                {
				    int playerNumber = int.Parse(messageTokens[1]);
                    
                    playerStats[playerNumber - 1].IncrementKills();
                }
			}
				break;
			case "assist":
			{
                if (gameModeState == 2)
                {
				    int playerNumber = int.Parse(messageTokens[1]);

                    playerStats[playerNumber - 1].IncrementAssists();
                }
			}
				break;
			case "death":
			{
                if (gameModeState == 2)
                {
    				int playerNumber = int.Parse(messageTokens[1]);
                    playerStats[playerNumber - 1].IncrementDeaths();

					int allShipIndex = -1;
					for (int n = 0; n < allShipPositions.Length; ++n)
						if (allShipPositions[n].shipNumber == playerNumber)
							allShipIndex = n;

					allShipPositions[allShipIndex].active = false;
					allShipTransforms[allShipIndex] = null;
                }
			}
				break;
			case "spawn":
			{
				if (gameModeState == 2)
				{
					int playerNumber = int.Parse(messageTokens[1]);

					int allShipIndex = -1;
					for (int n = 0; n < allShipPositions.Length; ++n)
						if (allShipPositions[n].shipNumber == playerNumber)
							allShipIndex = n;
					
					allShipPositions[allShipIndex].active = true;
					allShipTransforms[allShipIndex] = ((SingleShipControlAction)action).GetShipTransform();
					allShipPositions[allShipIndex].position = allShipTransforms[allShipIndex].position;
				}
			}
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

		public PlayerShipPosition[] GetOtherShipPositionReferences(int playerNumber)
		{
			if (numberPlayers == 1)
				return new PlayerShipPosition[0];
			else
			{
				PlayerShipPosition[] otherShips = new PlayerShipPosition[allShipPositions.Length - 1];
				for (int n = 0, m = 0; n < allShipPositions.Length; ++n)
				{
					if (allShipPositions[n].shipNumber != playerNumber)
					{
						otherShips[m] = allShipPositions[n];
						++m;
					}
				}
				return otherShips;
			}
		}

		private sealed class PlayerStatistics
		{
			private string playerName;
			private int kills;
			private int deaths;
			private int assists;
			public PlayerStatistics(string playerName)
			{
				this.playerName = playerName;
				ResetScore();
			}

			public void ResetScore()
			{
				kills = deaths = assists = 0;
			}

			public void IncrementKills()
			{
				++kills;
			}

			public void IncrementDeaths()
			{
				++deaths;
			}

			public void IncrementAssists()
			{
				++assists;
			}

			public int Kills
			{
				get { return kills; }
			}

			public int Deaths
			{
				get { return deaths; }
			}

			public int Assists
			{
				get { return assists; }
			}

			public string Name
			{
				get { return playerName; }
			}
		}

		private sealed class ScoreboardDisplayDataWrapper
		{
			public int screenWidth = -1;
			public int screenHeight = -1;
			public int screenLeftStart = -1;
			public int screenTopStart = -1;
//			public int screenHorizontalStep = -1;
//			public int screenVerticalStep = -1;

			public ScoreboardDisplayDataWrapper(int numberPlayers, int thisPlayerNumber)
			{
				CalculateValues(numberPlayers, thisPlayerNumber);
			}

			public void CalculateValues(int numberPlayers, int thisPlayerNumber)
			{
				switch (numberPlayers)
				{
				case 1:
					screenWidth = Screen.width;
					screenHeight = Screen.height;
					screenLeftStart = 0;
					screenTopStart = 0;
//					screenHorizontalStep = Screen.width / 32;
//					screenVerticalStep = Screen.height / 24;
					break;
				case 2:
					screenWidth = Screen.width / 2;
					screenHeight = Screen.height / 2;
//					screenHorizontalStep = Screen.width / 32;
//					screenVerticalStep = Screen.height / 16;
					screenLeftStart = Screen.width / 4;
					switch (thisPlayerNumber)
					{
					case 1:
						screenTopStart = 0;
						break;
					case 2:
						screenTopStart = screenHeight;
						break;
					}
					break;
				case 3:
					screenWidth = Screen.width / 2;
					screenHeight = Screen.height / 2;
//					screenHorizontalStep = Screen.width / 24;
//					screenVerticalStep = Screen.height / 16;
					switch (thisPlayerNumber)
					{
					case 1:
						screenTopStart = 0;
						screenLeftStart = 0;
						break;
					case 2:
						screenTopStart = 0;
						screenLeftStart = screenWidth;
						break;
					case 3:
						screenTopStart = screenHeight;
						screenLeftStart = 0;
						break;
					}
					break;
				case 4:
					screenWidth = Screen.width / 2;
					screenHeight = Screen.height / 2;
//					screenHorizontalStep = Screen.width / 24;
//					screenVerticalStep = Screen.height / 16;
					switch (thisPlayerNumber)
					{
					case 1:
						screenTopStart = 0;
						screenLeftStart = 0;
						break;
					case 2:
						screenTopStart = 0;
						screenLeftStart = screenWidth;
						break;
					case 3:
						screenTopStart = screenHeight;
						screenLeftStart = 0;
						break;
					case 4:
						screenTopStart = screenHeight;
						screenLeftStart = screenWidth;
						break;
					}
					break;
				}
			}
		}
	}
}
