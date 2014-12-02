using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class SelectNumberMenuAction : Action, IReceiveControllerMenuInputHandler
	{
//		private int numberPlayers;
		
		private ControllerMenuInputHandler[] inputHandlers;
		private MenuCursorDataWrapper[] menuCursors;
		
		public GUIStyle guiStyle;
		public Transform cameraPivot;
		public AnimationCurve slerpEasing;
		public Quaternion originalDirection;
		public Quaternion forwardRotatedDirection;
		public Quaternion backwardRotatedDirection;

		private int playersPlaying;
		
		public override void ActionStart()
		{
//			numberPlayers = DataManager.GetNumberPlayers();

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
			backwardRotatedDirection = Quaternion.Euler(0, -5, 0) * originalDirection;

			playersPlaying = DataManager.GetNumberPlayers();

			time = 0f;
			switchingMenu = 0;
		}
		
		private int switchingMenu = 0;
		private int playerSelected = 0;
		private float time = 0f;
		private const float TIME_ANIMATING = 0.5f;
		
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
							if (menuCursors[n].menuItemSelected == 0)
							{
								playersPlaying -= 1;
								if (playersPlaying < 2)
									playersPlaying = 4;
							}
						}
						
						if (inputHandlers[n].GetAxisKeyDown("Left_Horizontal_Right"))
						{
							if (menuCursors[n].menuItemSelected == 0)
							{
								playersPlaying += 1;
								if (playersPlaying > 4)
									playersPlaying = 2;
							}
						}

						if (inputHandlers[n].GetAxisKeyDown("Left_Vertical_Down"))
						{
							menuCursors[n].menuItemSelected += 1;
							menuCursors[n].menuItemSelected %= 3;
						}
						
						if (inputHandlers[n].GetAxisKeyDown("Left_Vertical_Up"))
						{
							menuCursors[n].menuItemSelected -= 1;
							if (menuCursors[n].menuItemSelected < 0)
								menuCursors[n].menuItemSelected += 3;
						}
						
						if (inputHandlers[n].GetButtonDown("Confirm_Button") ||
						    inputHandlers[n].GetButtonDown("Start_Button"))
						{
							if (menuCursors[n].menuItemSelected == 1)
							{
								switchingMenu = 1;
								DataManager.SetNumberPlayers(playersPlaying);
							}
							else if (menuCursors[n].menuItemSelected == 2)
							{
								switchingMenu = -1;
								DataManager.SetNumberPlayers(playersPlaying);
							}
						}
						
						if (inputHandlers[n].GetButtonDown("Cancel_Button"))
						{
							if (menuCursors[n].menuItemSelected == 2)
							{
								switchingMenu = -1;
								DataManager.SetNumberPlayers(playersPlaying);

								menuCursors[n].menuItemSelected = 0;
							}
							else
							{
								menuCursors[n].menuItemSelected = 2;
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
				break;
			case 1:
				time += Time.deltaTime;
				cameraPivot.rotation = Quaternion.Slerp(originalDirection, forwardRotatedDirection, slerpEasing.Evaluate(time / TIME_ANIMATING));
				
				if (time > TIME_ANIMATING)
				{
					cameraPivot.rotation = forwardRotatedDirection;

					SceneManager.SendMessage(this, "run JoinMenuAction");
					SceneManager.SendMessage(this, "remove from_action_list");
				}
				break;
			case -1:
				time += Time.deltaTime;
				cameraPivot.rotation = Quaternion.Slerp(originalDirection, backwardRotatedDirection, slerpEasing.Evaluate(time / TIME_ANIMATING));
				
				if (time > TIME_ANIMATING)
				{
					cameraPivot.rotation = backwardRotatedDirection;

					SceneManager.SendMessage(this, "run MainMenuAction");
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
				GUI.Label(new Rect(screenMidHorizontal, screenMidVertical + -1 * screenVerticalDistance, Screen.width, 100), "Players: < " + playersPlaying + " >", guiStyle);
				GUI.Label(new Rect(screenMidHorizontal, screenMidVertical + 0 * screenVerticalDistance, Screen.width, 100), "Continue", guiStyle);
				GUI.Label(new Rect(screenMidHorizontal, screenMidVertical + 1 * screenVerticalDistance, Screen.width, 100), "Back", guiStyle);

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
			
		}

		public void PassControllerMenuInputHandlers(ControllerMenuInputHandler[] inputHandlers)
		{
			this.inputHandlers = inputHandlers;
		}
	}
}

