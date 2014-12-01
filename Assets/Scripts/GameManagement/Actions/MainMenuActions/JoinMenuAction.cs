using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class JoinMenuAction : Action
	{
		private int numberPlayers;
		
		private ControllerMenuInputHandler[] inputHandlers;
		private JoinCursorDataWrapper[] menuCursors;
		
		public GUIStyle guiStyle;
		public Transform cameraPivot;
		public AnimationCurve slerpEasing;
		public Quaternion originalDirection;
		public Quaternion forwardRotatedDirection;
		public Quaternion backwardRotatedDirection;
		
		private int playersPlaying;
		
		public override void ActionStart()
		{
			numberPlayers = DataManager.GetNumberPlayers();
			
			if (null == inputHandlers && null == menuCursors)
			{
				inputHandlers = InputHandlerHolder.GetMenuInputHandlers();
				menuCursors = new JoinCursorDataWrapper[4];
				for (int n = 0; n < 4; ++n)
				{
					menuCursors[n] = new JoinCursorDataWrapper(n + 1, 0, DataManager.GetControllerSetup(n+1),
					                                           DataManager.GetInvertPitch(n+1),
					                                           DataManager.GetInvertYaw(n+1),
					                                           DataManager.GetInvertRoll(n+1));
				}
			}
			
			CalculateGUIValues();
			
			originalDirection = cameraPivot.rotation;
			forwardRotatedDirection = Quaternion.Euler(0, -135, 0) * originalDirection;
			backwardRotatedDirection = Quaternion.Euler(0, -35, 0) * originalDirection;
			
			playersPlaying = DataManager.GetNumberPlayers();
			
			time = 0f;
			switchingMenu = 0;
		}

		public enum JoinStates
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
		private string[] controllerSetupNames =
		{
			"Aim on Right",
			"Aim on Left",
            "L - Control Stick",
			"R - Control Stick",
		};

		public override void ActionUpdate()
		{
			switch (switchingMenu)
			{
			case 0:
				for (int n = 0; n < 4; ++n)
				{
					switch (menuCursors[n].localState)
					{
					case JoinStates.NOT_JOINED:
						if (inputHandlers[n].GetButtonDown("Confirm_Button") ||
						    inputHandlers[n].GetButtonDown("Start_Button"))
						{
							menuCursors[n].menuItemSelected = 0;
							menuCursors[n].localState = JoinStates.JOINED;
							DataManager.SetPlayerActive(n + 1, true);
						}

						if (inputHandlers[n].GetButtonDown("Cancel_Button"))
						{
							switchingMenu = -1;
						}
						break;
					case JoinStates.JOINED:
						if (DataManager.GetPlayerActive(n+1))
						{
							if (inputHandlers[n].GetAxisKeyDown("Left_Horizontal_Left") ||
							    inputHandlers[n].GetAxisKeyDown("Right_Horizontal_Left"))
							{
								switch (menuCursors[n].menuItemSelected)
								{
								case 1:
									menuCursors[n].controlSetup -= 1;
									if (menuCursors[n].controlSetup < 0)
										menuCursors[n].controlSetup += controllerSetupNames.Length;
									break;
								case 2:
									if (menuCursors[n].invertPitch > 0)
										menuCursors[n].invertPitch = 0;
									else
										menuCursors[n].invertPitch = 1;
									break;
								case 3:
									if (menuCursors[n].invertYaw > 0)
										menuCursors[n].invertYaw = 0;
									else
										menuCursors[n].invertYaw = 1;
									break;
								case 4:
									if (menuCursors[n].invertRoll > 0)
										menuCursors[n].invertRoll = 0;
									else
										menuCursors[n].invertRoll = 1;
									break;
								}
							}
							
							if (inputHandlers[n].GetAxisKeyDown("Left_Horizontal_Right") ||
							    inputHandlers[n].GetAxisKeyDown("Right_Horizontal_Right"))
							{
								switch (menuCursors[n].menuItemSelected)
								{
								case 1:
									menuCursors[n].controlSetup += 1;
									menuCursors[n].controlSetup %= controllerSetupNames.Length;
									break;
								case 2:
									if (menuCursors[n].invertPitch > 0)
										menuCursors[n].invertPitch = 0;
									else
										menuCursors[n].invertPitch = 1;
									break;
								case 3:
									if (menuCursors[n].invertYaw > 0)
										menuCursors[n].invertYaw = 0;
									else
										menuCursors[n].invertYaw = 1;
									break;
								case 4:
									if (menuCursors[n].invertRoll > 0)
										menuCursors[n].invertRoll = 0;
									else
										menuCursors[n].invertRoll = 1;
									break;
								}
							}
							
							if (inputHandlers[n].GetAxisKeyDown("Left_Vertical_Down") ||
							    inputHandlers[n].GetAxisKeyDown("Right_Vertical_Down"))
							{
								menuCursors[n].menuItemSelected += 1;
								menuCursors[n].menuItemSelected %= menuItems.Length;
							}
							
							if (inputHandlers[n].GetAxisKeyDown("Left_Vertical_Up") ||
							    inputHandlers[n].GetAxisKeyDown("Right_Vertical_Up"))
							{
								menuCursors[n].menuItemSelected -= 1;
								if (menuCursors[n].menuItemSelected < 0)
									menuCursors[n].menuItemSelected += menuItems.Length;
							}
							
							if (inputHandlers[n].GetButtonDown("Confirm_Button") ||
							    inputHandlers[n].GetButtonDown("Start_Button"))
							{
								if (menuCursors[n].menuItemSelected == 0)
								{
									menuCursors[n].localState = JoinStates.READY;

									DataManager.SetControllerSetup(n+1, menuCursors[n].controlSetup);
									DataManager.SetInvertPitch(n+1, menuCursors[n].invertPitch);
									DataManager.SetInvertYaw(n+1, menuCursors[n].invertYaw);
									DataManager.SetInvertRoll(n+1, menuCursors[n].invertRoll);
								}
								else if (menuCursors[n].menuItemSelected == 5)
								{
									menuCursors[n].localState = JoinStates.NOT_JOINED;

									menuCursors[n].controlSetup = DataManager.GetControllerSetup(n+1);
									menuCursors[n].invertPitch = DataManager.GetInvertPitch(n+1);
									menuCursors[n].invertYaw = DataManager.GetInvertYaw(n+1);
									menuCursors[n].invertRoll = DataManager.GetInvertRoll(n+1);
								}
							}
							
							if (inputHandlers[n].GetButtonDown("Cancel_Button"))
							{
								if (menuCursors[n].menuItemSelected == 5)
								{
									menuCursors[n].localState = JoinStates.NOT_JOINED;

									menuCursors[n].controlSetup = DataManager.GetControllerSetup(n+1);
									menuCursors[n].invertPitch = DataManager.GetInvertPitch(n+1);
									menuCursors[n].invertYaw = DataManager.GetInvertYaw(n+1);
									menuCursors[n].invertRoll = DataManager.GetInvertRoll(n+1);

									menuCursors[n].menuItemSelected = 0;
								}
								else
								{
									menuCursors[n].menuItemSelected = 5;
								}
							}
						}
						break;
					case JoinStates.READY:
						if (inputHandlers[n].GetButtonDown("Confirm_Button") ||
						    inputHandlers[n].GetButtonDown("Start_Button"))
						{
							int activePlayers = 0;
							bool everyoneReady = true;
							for (int m = 0; m < menuCursors.Length; ++m)
							{
								if (menuCursors[m].localState == JoinStates.READY)
									++activePlayers;
								if (menuCursors[m].localState == JoinStates.JOINED)
									everyoneReady = false;
								if (menuCursors[m].localState == JoinStates.NOT_JOINED)
									DataManager.SetPlayerActive(m + 1, false);
							}

							if (everyoneReady && activePlayers >= 1)//2)
							{
								DataManager.SetNumberPlayers(activePlayers);
								switchingMenu = 1;
							}
						}
						if (inputHandlers[n].GetButtonDown("Cancel_Button"))
						{
							menuCursors[n].localState = JoinStates.JOINED;
						}
						break;
					}
				}
				break;
			case 1:
				time += Time.deltaTime;
				cameraPivot.rotation = Quaternion.Slerp(originalDirection, forwardRotatedDirection, slerpEasing.Evaluate(time / TIME_ANIMATING));
				
				if (time > TIME_ANIMATING)
				{
					cameraPivot.rotation = forwardRotatedDirection;
					
					SceneManager.SendMessage(this, "run SetupGameMenuAction");
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


		private string[] menuItems =
		{
			"Ready",
			"Control Setup:",
			"Invert Pitch:",
			"Invert Yaw:",
			"Invert Roll:",
			"Leave",
		};
		public override void ActionOnGUI()
		{
			if (switchingMenu == 0)
			{
				for (int j = 0; j < 2; ++j)
				{
					for (int i = 0; i < 2; ++i)
					{
						int cursorIndex = 2 * j + i;
						switch (menuCursors[cursorIndex].localState)
						{
						case JoinStates.NOT_JOINED:
							guiStyle.fontSize = fontSizeJoin;
							GUI.Label(new Rect(screenMidHorizontal * i + screenHorizontalDistance * 4,
						                   screenMidVertical * j + screenVerticalDistance * 4,
						                   Screen.width, Screen.height), "Join", guiStyle);
							break;
						case JoinStates.JOINED:
							guiStyle.fontSize = fontSize;
							for (int menuItemCounter = 0; menuItemCounter < menuItems.Length; ++menuItemCounter)
							{
								GUI.Label(new Rect(screenMidHorizontal * i + screenHorizontalDistance * (3 - i),
						                   screenMidVertical * j + (screenVerticalDistance * (2 + 3 * menuItemCounter) / 2),
						                   Screen.width, Screen.height), menuItems[menuItemCounter], guiStyle);
							}
//							guiStyle.fontSize = fontSizeControlNames;
							GUI.Label(new Rect(screenMidHorizontal * i + screenHorizontalDistance * (8 - i),
							                   screenMidVertical * j + (screenVerticalDistance * (2 + 3 * 1) / 2),
							                   Screen.width, Screen.height), controllerSetupNames[menuCursors[cursorIndex].controlSetup], guiStyle);

//							guiStyle.fontSize = fontSize;
							GUI.Label(new Rect(screenMidHorizontal * i + screenHorizontalDistance * (8 - i),
							                   screenMidVertical * j + (screenVerticalDistance * (2 + 3 * 2) / 2),
							                   Screen.width, Screen.height), ((menuCursors[cursorIndex].invertPitch > 0) ? "Yes" : "No"), guiStyle);

							GUI.Label(new Rect(screenMidHorizontal * i + screenHorizontalDistance * (8 - i),
							                   screenMidVertical * j + (screenVerticalDistance * (2 + 3 * 3) / 2),
							                   Screen.width, Screen.height), ((menuCursors[cursorIndex].invertYaw > 0) ? "Yes" : "No"), guiStyle);

							GUI.Label(new Rect(screenMidHorizontal * i + screenHorizontalDistance * (8 - i),
							                   screenMidVertical * j + (screenVerticalDistance * (2 + 3 * 4) / 2),
							                   Screen.width, Screen.height), ((menuCursors[cursorIndex].invertRoll > 0) ? "Yes" : "No"), guiStyle);

							GUI.Label(new Rect(screenMidHorizontal * i + screenHorizontalDistance * (2 - i),
						                   screenMidVertical * j + (screenVerticalDistance * (2 + 3 * menuCursors[2 * j + i].menuItemSelected) / 2),
						                   Screen.width, Screen.height), " >", guiStyle);
							break;
						case JoinStates.READY:
							guiStyle.fontSize = fontSizeReady;
							GUI.Label(new Rect(screenMidHorizontal * i + screenHorizontalDistance * 2,
						                   screenMidVertical * j + screenVerticalDistance * 2,
						                   Screen.width, Screen.height), "Ready", guiStyle);
							break;
						}
					}
				}
			}
		}
		
		private int screenMidHorizontal;
		private int screenMidVertical;
		private int screenVerticalDistance;
		private int screenHorizontalDistance;
		private int fontSize;
		private int fontSizeJoin;
		private int fontSizeReady;
		private int fontSizeControlNames;
		private void CalculateGUIValues()
		{
			fontSize = (int)(Screen.width / 1280f * 48);
			fontSizeJoin = (int)(Screen.width / 1280f * 150);
			fontSizeControlNames = (int)(Screen.width / 1280f * 42);
			fontSizeReady = (int)(Screen.width / 1280f * 180);
			guiStyle.fontSize = fontSize;
			screenMidHorizontal = Screen.width / 2;
			screenMidVertical = Screen.height / 2;
			screenVerticalDistance = Screen.height / 24;
			screenHorizontalDistance = Screen.width / 24;
		}
		
		public override void ReceiveMessage(Action action, string message)
		{
			
		}
		
		public void PassControllerMenuInputHandlers(ControllerMenuInputHandler[] inputHandlers)
		{
			this.inputHandlers = inputHandlers;
		}

		public sealed class JoinCursorDataWrapper
		{
			public int playerNumber;
			public int menuItemSelected;
			public int controlSetup;
			public int invertPitch;
			public int invertYaw;
			public int invertRoll;
			public JoinStates localState;
			public JoinCursorDataWrapper(int playerNumber, int menuItemSelected, int controlSetup, int invertPitch, int invertYaw, int invertRoll)
			{
				this.playerNumber = playerNumber;
				this.menuItemSelected = menuItemSelected;
				this.controlSetup = controlSetup;
				this.invertPitch = invertPitch;
				this.invertYaw = invertYaw;
				this.invertRoll = invertRoll;
				this.localState = JoinStates.NOT_JOINED;
			}
		}
	}
}

