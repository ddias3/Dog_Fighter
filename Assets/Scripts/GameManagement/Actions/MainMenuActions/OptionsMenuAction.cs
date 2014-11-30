using UnityEngine;
using System.Collections;

using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class OptionsMenuAction : Action
	{
		private int numberPlayers;
		
		private ControllerMenuInputHandler[] inputHandlers;
		private GridCursorDataWrapper[] menuCursors;
		
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
				menuCursors = new GridCursorDataWrapper[4];
				for (int n = 0; n < 4; ++n)
				{
					menuCursors[n] = new GridCursorDataWrapper(n + 1, n % 2, (n / 2) * 4);
				}
			}
			
			CalculateGUIValues();
			
			originalDirection = cameraPivot.rotation;
			rotatedDirection = Quaternion.Euler(0, -5, 0) * originalDirection;
			
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
							menuCursors[n].menuItemSelectedY += 1;
							if (menuCursors[n].menuItemSelectedY == 8)
								menuCursors[n].menuItemSelectedX = 0;
							menuCursors[n].menuItemSelectedY %= 9;
						}
						
						if (inputHandlers[n].GetAxisKeyDown("Left_Vertical_Up"))
						{
							menuCursors[n].menuItemSelectedY -= 1;
							if (menuCursors[n].menuItemSelectedY < 0)
							{
								menuCursors[n].menuItemSelectedX = 0;
								menuCursors[n].menuItemSelectedY = 8;
							}
						}

						if (inputHandlers[n].GetAxisKeyDown("Left_Horizontal_Left"))
						{
							if (menuCursors[n].menuItemSelectedY != 8)
							{
								menuCursors[n].menuItemSelectedX -= 1;
								if (menuCursors[n].menuItemSelectedX < 0)
									menuCursors[n].menuItemSelectedX = 0;
							}
						}
						
						if (inputHandlers[n].GetAxisKeyDown("Left_Horizontal_Right"))
						{
							if (menuCursors[n].menuItemSelectedY != 8)
							{
								menuCursors[n].menuItemSelectedX += 1;
								if (menuCursors[n].menuItemSelectedX > 1)
									menuCursors[n].menuItemSelectedX = 1;
							}
						}
						
						if (inputHandlers[n].GetButtonDown("Confirm_Button") ||
						    inputHandlers[n].GetButtonDown("Start_Button"))
						{
							int x = menuCursors[n].menuItemSelectedX;
							int y = menuCursors[n].menuItemSelectedY;

							if (y == 8)
							{
								switchingMenu = true;
								playerThatSelected = n;
							}
							else
							{
								int controllerNumber = 2 * (y / 4) + x;

								InputHandlerHolder.SetInvertAxis(!InputHandlerHolder.GetInvertAxis(axisNamesMinusNumbers[y % 4], controllerNumber + 1),
								                                 axisNamesMinusNumbers[y % 4],
								                                 controllerNumber + 1);
							}
						}
						
						if (inputHandlers[n].GetButtonDown("Cancel_Button"))
						{
							if (menuCursors[n].menuItemSelectedY == 8 &&
							    menuCursors[n].menuItemSelectedX == 0)
							{
								switchingMenu = true;
								playerThatSelected = n;

								menuCursors[n].menuItemSelectedX = 0;
								menuCursors[n].menuItemSelectedY = 0;
							}
							else
							{
								menuCursors[n].menuItemSelectedX = 0;
								menuCursors[n].menuItemSelectedY = 8;
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

					SceneManager.SendMessage(this, "run MainMenuAction");
					SceneManager.SendMessage(this, "remove from_action_list");
				}
			}
		}
		
		public override void ActionFixedUpdate()
		{
			// do nothing;
		}

		private string[] menuWords =
		{
			"Invert Left Vertical:",
			"Invert Left Horizontal:",
			"Invert Right Vertical:",
			"Invert Right Horizontal:"
		};
		private string[] axisNamesMinusNumbers = 
		{
			"Left_Vertical",
			"Left_Horizontal",
			"Right_Vertical",
			"Right_Horizontal",
		};
		public override void ActionOnGUI()
		{
			if (!switchingMenu)
			{
				for (int j = 0; j < 2; ++j)
				{
					for (int i = 0; i < 2; ++i)
					{
						GUI.Label(new Rect(screenMidHorizontal + screenHorizontalDistance * (-3 + (7 * i)),
						                   screenMidVertical + screenVerticalDistance * (-3 + (7 * j) - (4)),
						                   Screen.width, Screen.height), "Controller: " + ((i + 2 * j) + 1), guiStyle);

						for (int menuItemCounter = 0; menuItemCounter < 4; ++menuItemCounter)
						{
							GUI.Label(new Rect(screenMidHorizontal + screenHorizontalDistance * (-3 + (7 * i)),
							                   screenMidVertical + screenVerticalDistance * (-3 + (7 * j) + (-3 + menuItemCounter)),
							                   Screen.width, Screen.height), menuWords[menuItemCounter], guiStyle);

							if (InputHandlerHolder.GetInvertAxis(axisNamesMinusNumbers[menuItemCounter], (i + 2 * j) + 1))
							{
								GUI.Label(new Rect(screenMidHorizontal + screenHorizontalDistance * (-3 + (7 * i) + 4),
								                   screenMidVertical + screenVerticalDistance * (-3 + (7 * j) + (-3 + menuItemCounter)),
								                   Screen.width, Screen.height), "    Yes", guiStyle);
							}
							else
							{
								GUI.Label(new Rect(screenMidHorizontal + screenHorizontalDistance * (-3 + (7 * i) + 4),
								                   screenMidVertical + screenVerticalDistance * (-3 + (7 * j) + (-3 + menuItemCounter)),
								                   Screen.width, Screen.height), "    No", guiStyle);
							}
						}
					}
				}

				GUI.Label(new Rect(screenMidHorizontal + screenHorizontalDistance * (-3 + (7 * 0)),
				                   screenMidVertical + screenVerticalDistance * (-3 + (7 * 2) + (-3 + 0)),
				                   Screen.width, Screen.height), "Back", guiStyle);
				
				for (int n = 0; n < 4; ++n)
					if (DataManager.GetPlayerActive(n+1))
						GUI.Label(new Rect(screenMidHorizontal + screenHorizontalDistance * (-4 + (7 * menuCursors[n].menuItemSelectedX)) - (n * Screen.width / 128),
						                   screenMidVertical + screenVerticalDistance * (-3 + (7 * (menuCursors[n].menuItemSelectedY / 4)) + (-3 + (menuCursors[n].menuItemSelectedY % 4))),
						                   Screen.width, Screen.height), "    >", guiStyle);
			}
		}
		
		private int screenMidHorizontal;
		private int screenMidVertical;
		private int screenVerticalDistance;
		private int screenHorizontalDistance;
		private int fontSize;
		private void CalculateGUIValues()
		{
			fontSize = (int)(Screen.width / 1280f * 32);
			guiStyle.fontSize = fontSize;
			screenMidHorizontal = Screen.width / 2;
			screenMidVertical = Screen.height / 2;
			screenVerticalDistance = Screen.height / 20;
			screenHorizontalDistance = Screen.width / 20;
		}
		
		public override void ReceiveMessage(Action action, string message)
		{
			// ignore messages
		}

		public sealed class GridCursorDataWrapper
		{
			public int playerNumber;
			public int menuItemSelectedX;
			public int menuItemSelectedY;
			public GridCursorDataWrapper(int playerNumber, int menuItemSelectedX, int menuItemSelectedY)
			{
				this.playerNumber = playerNumber;
				this.menuItemSelectedX = menuItemSelectedX;
				this.menuItemSelectedY = menuItemSelectedY;
			}
		}
	}
}
