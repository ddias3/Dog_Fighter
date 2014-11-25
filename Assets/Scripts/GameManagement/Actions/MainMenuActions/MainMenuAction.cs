using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class MainMenuAction : Action
	{
		private int numberPlayers;

		private ControllerMenuInputHandler[] inputHandlers;
		private MenuCursorDataWrapper[] menuCursors;

		public GUIStyle guiStyle;
		public Transform cameraPivot;
		public AnimationCurve slerpEasing;
		public Quaternion originalDirection;
		public Quaternion rotatedDirection;

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
			rotatedDirection = Quaternion.Euler(0, 5, 0) * originalDirection;

			time = 0f;
			switchingMenu = false;
		}

		private bool switchingMenu = false;
		private int playerThatSelected = 0;
		private float time = 0f;
		private const float TIME_ANIMATING = 0.5f;

		public override void ActionUpdate()
		{
			if (!switchingMenu)
			{
				for (int n = 0; n < 4; ++n)
				{
					if (DataManager.GetPlayerActive(n+1))
					{
						if (inputHandlers[n].GetAxisKeyDown("Left_Vertical_Down"))
						{
							menuCursors[n].menuItemSelected += 1;
							menuCursors[n].menuItemSelected %= 4;
						}

						if (inputHandlers[n].GetAxisKeyDown("Left_Vertical_Up"))
						{
							menuCursors[n].menuItemSelected -= 1;
							if (menuCursors[n].menuItemSelected < 0)
								menuCursors[n].menuItemSelected += 4;
						}

						if (inputHandlers[n].GetButtonDown("Confirm_Button"))
						{
							if (menuCursors[n].menuItemSelected == 0)
							{
								switchingMenu = true;
								playerThatSelected = n;
							}
						}

						if (inputHandlers[n].GetButtonDown("Cancel_Button"))
						{
							Debug.Log("Player " + inputHandlers[n].PlayerNumber + " pressed B");
						}

						if (inputHandlers[n].GetButtonDown("Start_Button"))
						{
							if (menuCursors[n].menuItemSelected == 0)
							{
								switchingMenu = true;
								playerThatSelected = n;
							}
						}
					}
					else
					{
						if (inputHandlers[n].GetAxisKeyDown("Left_Vertical_Down") ||
						    inputHandlers[n].GetAxisKeyDown("Left_Vertical_Up") ||
						    inputHandlers[n].GetAxisKeyDown("Left_Horizontal_Left") ||
						    inputHandlers[n].GetAxisKeyDown("Left_Horizontal_Right") ||
						    inputHandlers[n].GetButtonDown("Confirm_Button") ||
							inputHandlers[n].GetButtonDown("Cancel_Button") ||
						    inputHandlers[n].GetButtonDown("Start_Button"))
						{
							DataManager.SetPlayerActive(n+1, true);
						}
					}
				}
			}
			else
			{
				time += Time.deltaTime;
				cameraPivot.rotation = Quaternion.Slerp(originalDirection, rotatedDirection, slerpEasing.Evaluate(time / TIME_ANIMATING));

				if (time > TIME_ANIMATING)
				{
					cameraPivot.rotation = rotatedDirection;

					switch (menuCursors[playerThatSelected].menuItemSelected)
					{
					case 0:
						SceneManager.SendMessage(this, "run SelectNumberMenuAction");
						SceneManager.SendMessage(this, "remove from_action_list");
						break;
					}
				}
			}
		}

		public override void ActionFixedUpdate()
		{
			// do nothing;
		}
		
		public override void ActionOnGUI()
		{
			if (!switchingMenu)
			{
				GUI.Label(new Rect(screenMidHorizontal, screenMidVertical + -1 * screenVerticalDistance, Screen.width, 100), "Play", guiStyle);
				GUI.Label(new Rect(screenMidHorizontal, screenMidVertical + 0 * screenVerticalDistance, Screen.width, 100), "Options", guiStyle);
				GUI.Label(new Rect(screenMidHorizontal, screenMidVertical + 1 * screenVerticalDistance, Screen.width, 100), "Credits", guiStyle);
				GUI.Label(new Rect(screenMidHorizontal, screenMidVertical + 2 * screenVerticalDistance, Screen.width, 100), "Exit", guiStyle);

				for (int n = 0; n < 4; ++n)
					if (DataManager.GetPlayerActive(n+1))
						GUI.Label(new Rect(screenMidHorizontal - (2 + n) * Screen.width / 36, screenMidVertical + (menuCursors[n].menuItemSelected - 1) * screenVerticalDistance, Screen.width, 100), ">", guiStyle);
			}
		}

		private int screenMidHorizontal;
		private int screenMidVertical;
		private int screenVerticalDistance;
		private int fontSize;
		private void CalculateGUIValues()
		{
			fontSize = (int)(Screen.width / 1280f * 104);
			guiStyle.fontSize = fontSize;
			screenMidHorizontal = Screen.width / 2;
			screenMidVertical = Screen.height / 2;
			screenVerticalDistance = Screen.height / 7;
		}
		
		public override void ReceiveMessage(Action action, string message)
		{
			// ignore messages
		}
	}
}