using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class SetupGameMenuAction : Action
	{
		private int numberPlayers;
		
		private ControllerMenuInputHandler[] inputHandlers;
		private MenuCursorDataWrapper[] menuCursors;
		
		public GUIStyle guiStyle;
		public Transform cameraPivot;
		public AnimationCurve slerpEasing;
		public Quaternion originalDirection;
		public Quaternion forwardRotatedDirection;
		public Quaternion backwardRotatedDirection;

		public override void ActionStart()
		{
			numberPlayers = DataManager.GetNumberPlayers();
			
			if (null == inputHandlers && null == menuCursors)
			{
				inputHandlers = InputHandlerHolder.GetMenuInputHandlers();
				menuCursors = new MenuCursorDataWrapper[4];
				for (int n = 0; n < 4; ++n)
				{
					menuCursors[n] = new MenuCursorDataWrapper(n + 1, 0);
				}
			}
			
			CalculateGUIValues();
			
			originalDirection = cameraPivot.rotation;
			forwardRotatedDirection = Quaternion.Euler(0, 30, 0) * originalDirection;
			backwardRotatedDirection = Quaternion.Euler(0, 135, 0) * originalDirection;

			for (int n = 0; n < gameLengthMinutes.Length; ++n)
				if (gameLengthMinutes[n] == DataManager.GetGameLengthMinutes())
					gameLengthMinutesIndex = n;

			gameMode = DataManager.GetGameMode();
			mapId = DataManager.GetMapId();

			time = 0f;
			switchingMenu = 0;
		}
		
		private enum JOIN_STATES
		{
			NOT_JOINED,
			JOINED,
			READY,
		};
		
		private int switchingMenu = 0;
		private int internalState = 0;
		private int playerSelected = 0;
		private float time = 0f;
		private const float TIME_ANIMATING = 0.5f;
		private int[] gameLengthMinutes = { 1, 2, 3, 5, 7, 10, 15, 20 };
		private int gameLengthMinutesIndex;
		private int mapId;
		private int gameMode;
		public override void ActionUpdate()
		{
			switch (switchingMenu)
			{
			case 0:
				for (int n = 0; n < 4; ++n)
				{
					if (DataManager.GetPlayerActive(n+1))
					{
						if (inputHandlers[n].GetAxisKeyDown("Left_Horizontal_Left"))
						{
							switch (menuCursors[n].menuItemSelected)
							{
							case 2:
								--gameLengthMinutesIndex;
								if (gameLengthMinutesIndex < 0)
									gameLengthMinutesIndex += gameLengthMinutes.Length;
								DataManager.SetGameLengthMinutes(gameLengthMinutes[gameLengthMinutesIndex]);
								break;
							}
						}
						
						if (inputHandlers[n].GetAxisKeyDown("Left_Horizontal_Right"))
						{
							switch (menuCursors[n].menuItemSelected)
							{
							case 2:
								++gameLengthMinutesIndex;
								gameLengthMinutesIndex %= gameLengthMinutes.Length;
								DataManager.SetGameLengthMinutes(gameLengthMinutes[gameLengthMinutesIndex]);
								break;
							}
						}
						
						if (inputHandlers[n].GetAxisKeyDown("Left_Vertical_Down"))
						{
							menuCursors[n].menuItemSelected += 1;
							menuCursors[n].menuItemSelected %= 5;
						}
						
						if (inputHandlers[n].GetAxisKeyDown("Left_Vertical_Up"))
						{
							menuCursors[n].menuItemSelected -= 1;
							if (menuCursors[n].menuItemSelected < 0)
								menuCursors[n].menuItemSelected += 5;
						}
						
						if (inputHandlers[n].GetButtonDown("Confirm_Button") ||
						    inputHandlers[n].GetButtonDown("Start_Button"))
						{
							if (menuCursors[n].menuItemSelected == 0)
							{
								switchingMenu = 1;
							}
							else if (menuCursors[n].menuItemSelected == 4)
							{
								switchingMenu = -1;
							}
						}
						
						if (inputHandlers[n].GetButtonDown("Cancel_Button"))
						{
							if (menuCursors[n].menuItemSelected == 4)
							{
								switchingMenu = -1;

								menuCursors[n].menuItemSelected = 0;
							}
							else
							{
								menuCursors[n].menuItemSelected = 4;
							}
						}
					}
					else
					{
						if (inputHandlers[n].GetButtonDown("Confirm_Button") ||
						    inputHandlers[n].GetButtonDown("Cancel_Button") ||
						    inputHandlers[n].GetButtonDown("Start_Button"))
						{
							DataManager.SetPlayerActive(n+1, true);
							switchingMenu = -1;
						}
					}
				}
				break;
			case 1:
				time += Time.deltaTime;
				cameraPivot.rotation = Quaternion.Slerp(originalDirection, forwardRotatedDirection, slerpEasing.Evaluate(time / TIME_ANIMATING));
				
				if (time > TIME_ANIMATING)
				{
					cameraPivot.rotation = forwardRotatedDirection;
					
					Application.LoadLevel("Map1Scene");
				}
				break;
			case -1:
				time += Time.deltaTime;
				cameraPivot.rotation = Quaternion.Slerp(originalDirection, backwardRotatedDirection, slerpEasing.Evaluate(time / TIME_ANIMATING));
				
				if (time > TIME_ANIMATING)
				{
					cameraPivot.rotation = backwardRotatedDirection;
					
					SceneManager.SendMessage(this, "run JoinMenuAction");
					SceneManager.SendMessage(this, "remove from_action_list");
				}
				break;
			}
		}
		
		public override void ActionFixedUpdate()
		{
			// do nothing;
		}
		
		public override void ActionOnGUI()
		{
			if (switchingMenu == 0)
			{
				GUI.Label(new Rect(screenHorizontalDistance * 2, 2 * screenVerticalDistance, Screen.width, 100), "Start Match", guiStyle);
				GUI.Label(new Rect(screenHorizontalDistance * 2, 3 * screenVerticalDistance, Screen.width, 100), "Game:", guiStyle);
				GUI.Label(new Rect(screenHorizontalDistance * 2, 4 * screenVerticalDistance, Screen.width, 100), "Time:", guiStyle);
				GUI.Label(new Rect(screenHorizontalDistance * 2, 5 * screenVerticalDistance, Screen.width, 100), "Map:", guiStyle);
				GUI.Label(new Rect(screenHorizontalDistance * 2, 6 * screenVerticalDistance, Screen.width, 100), "Back", guiStyle);

				GUI.Label(new Rect(screenHorizontalDistance * 6, 3 * screenVerticalDistance, Screen.width, 100), "Death Match", guiStyle);
				GUI.Label(new Rect(screenHorizontalDistance * 6, 4 * screenVerticalDistance, Screen.width, 100), gameLengthMinutes[gameLengthMinutesIndex].ToString() + " min.", guiStyle);
				GUI.Label(new Rect(screenHorizontalDistance * 6, 5 * screenVerticalDistance, Screen.width, 100), "Asteroid Field", guiStyle);

				for (int n = 0; n < 4; ++n)
					if (DataManager.GetPlayerActive(n + 1))
						GUI.Label(new Rect(screenHorizontalDistance * 2 - (2 + n) * Screen.width / 36, (menuCursors[n].menuItemSelected + 2) * screenVerticalDistance, Screen.width, 100), " >", guiStyle);
			}
			else if (switchingMenu == 1)
			{
				GUI.Label(new Rect(screenHorizontalDistance, Screen.height - 2 * screenVerticalDistance, Screen.width, Screen.height), "Loading", guiStyle);
			}
		}
		
		private int screenMidHorizontal;
		private int screenMidVertical;
		private int screenVerticalDistance;
		private int screenHorizontalDistance;
		private int fontSize;
		private void CalculateGUIValues()
		{
			fontSize = (int)(Screen.width / 1280f * 84);
			guiStyle.fontSize = fontSize;
			screenMidHorizontal = Screen.width / 2;
			screenMidVertical = Screen.height / 2;
			screenVerticalDistance = Screen.height / 10;
			screenHorizontalDistance = Screen.width / 16;
		}
		
		public override void ReceiveMessage(Action action, string message)
		{
			
		}
		
		public void PassControllerMenuInputHandlers(ControllerMenuInputHandler[] inputHandlers)
		{
			this.inputHandlers = inputHandlers;
		}
	}
}

