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

//			PlayerPrefs.DeleteAll();
//			PlayerPrefs.Save();
		}

		private bool switchingMenu = false;
		private int playerThatSelected = 0;
		private float time = 0f;
		private const float TIME_ANIMATING = 0.5f;

		private float[] rTriggers = {0f, 0f, 0f, 0f};
		private float[] lTriggers = {0f, 0f, 0f, 0f};

		public override void ActionUpdate()
		{
			if (!switchingMenu)
			{
				lTriggers[0] = Input.GetAxisRaw("Left_Trigger_P1");
				lTriggers[1] = Input.GetAxisRaw("Left_Trigger_P2");
				lTriggers[2] = Input.GetAxisRaw("Left_Trigger_P3");
				lTriggers[3] = Input.GetAxisRaw("Left_Trigger_P3");
				
				rTriggers[0] = Input.GetAxisRaw("Right_Trigger_P1");
				rTriggers[1] = Input.GetAxisRaw("Right_Trigger_P2");
				rTriggers[2] = Input.GetAxisRaw("Right_Trigger_P3");
				rTriggers[3] = Input.GetAxisRaw("Right_Trigger_P4");

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
							if (menuCursors[n].menuItemSelected != 3)
							{
								switchingMenu = true;
								playerThatSelected = n;

								switch (menuCursors[n].menuItemSelected)
								{
								case 0:
									rotatedDirection = Quaternion.Euler(0, 35, 0) * originalDirection;
									break;
								default:
									rotatedDirection = Quaternion.Euler(0, 5, 0) * originalDirection;
									break;
								}
							}
							else
							{
								Application.Quit();
							}
						}

						if (inputHandlers[n].GetButtonDown("Cancel_Button"))
						{
							menuCursors[n].menuItemSelected = 3;
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
						SceneManager.SendMessage(this, "run JoinMenuAction");
						SceneManager.SendMessage(this, "remove from_action_list");
						break;
					case 1:
						SceneManager.SendMessage(this, "run OptionsMenuAction");
						SceneManager.SendMessage(this, "remove from_action_list");
						break;
					case 2:
						SceneManager.SendMessage(this, "run CreditsMenuAction");
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
				GUI.Label(new Rect(0, 0, Screen.width, 100), "P1L: " + lTriggers[0].ToString());
				GUI.Label(new Rect(0, 20, Screen.width, 100), "P2L: " + lTriggers[1].ToString());
				GUI.Label(new Rect(0, 40, Screen.width, 100), "P3L: " + lTriggers[2].ToString());
				GUI.Label(new Rect(0, 60, Screen.width, 100), "P4L: " + lTriggers[3].ToString());
				GUI.Label(new Rect(0, 80, Screen.width, 100), "P1R: " + rTriggers[0].ToString());
				GUI.Label(new Rect(0, 100, Screen.width, 100), "P2R: " + rTriggers[1].ToString());
				GUI.Label(new Rect(0, 120, Screen.width, 100), "P3R: " + rTriggers[2].ToString());
				GUI.Label(new Rect(0, 140, Screen.width, 100), "P4R: " + rTriggers[3].ToString());
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
