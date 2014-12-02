using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class InputHandlerHolder
	{
		private ControllerDirectInputHandler[] directInputHandlers;
		private ControllerMenuInputHandler[] menuInputHandlers;

		private static InputHandlerHolder instance;

		public static ControllerMenuInputHandler[] GetMenuInputHandlers()
		{
			if (null == instance)
				instance = new InputHandlerHolder();

			if (null == instance.menuInputHandlers)
			{
				instance.menuInputHandlers = new ControllerMenuInputHandler[4];

				for (int n = 0; n < 4; ++n)
				{
					instance.menuInputHandlers[n] = ScriptableObject.CreateInstance<ControllerMenuInputHandler>();
					instance.menuInputHandlers[n].Initialize(n + 1);
				}
			}

			return instance.menuInputHandlers;
		}

		public static ControllerDirectInputHandler[] GetDirectInputHandlers()
		{
			if (null == instance)
				instance = new InputHandlerHolder();

			if (null == instance.directInputHandlers)
			{
				instance.directInputHandlers = new ControllerDirectInputHandler[4];

				for (int n = 0; n < 4; ++n)
				{
					instance.directInputHandlers[n] = ScriptableObject.CreateInstance<ControllerDirectInputHandler>();
					instance.directInputHandlers[n].Initialize(n + 1);
				}
			}
			
			return instance.directInputHandlers;
		}

		public static ControllerDirectInputHandler GetDirectInputHandler(int playerNumber)
		{
			int index = playerNumber - 1;

			if (null == instance)
				instance = new InputHandlerHolder();
			
			if (null == instance.directInputHandlers)
			{
				instance.directInputHandlers = new ControllerDirectInputHandler[4];
				
				for (int n = 0; n < 4; ++n)
				{
					instance.directInputHandlers[n] = ScriptableObject.CreateInstance<ControllerDirectInputHandler>();
					instance.directInputHandlers[n].Initialize(n + 1);
				}
			}
			
			return instance.directInputHandlers[index];
		}

		public static ControllerMenuInputHandler GetMenuInputHandler(int playerNumber)
		{
			int index = playerNumber - 1;

			if (null == instance)
				instance = new InputHandlerHolder();
			
			if (null == instance.menuInputHandlers)
			{
				instance.menuInputHandlers = new ControllerMenuInputHandler[4];
				
				for (int n = 0; n < 4; ++n)
				{
					instance.menuInputHandlers[n] = ScriptableObject.CreateInstance<ControllerMenuInputHandler>();
					instance.menuInputHandlers[n].Initialize(n + 1);
				}
			}

			return instance.menuInputHandlers[index];
		}

		public static bool GetInvertAxis(string axisName, int playerNumber)
		{
			int index = playerNumber - 1;

			bool returnValue = false;

			if (null != instance.menuInputHandlers)
			{
				switch (axisName)
				{
				case "Left_Vertical":
					returnValue = instance.menuInputHandlers[index].GetInvertAxis("Left_Vertical_Up");
					break;
				case "Left_Horizontal":
					returnValue = instance.menuInputHandlers[index].GetInvertAxis("Left_Horizontal_Right");
					break;
				case "Right_Vertical":
					returnValue = instance.menuInputHandlers[index].GetInvertAxis("Right_Vertical_Up");
					break;
				case "Right_Horizontal":
					returnValue = instance.menuInputHandlers[index].GetInvertAxis("Right_Horizontal_Right");
					break;
				}
			}

			if (null != instance.directInputHandlers)
			{
				if (returnValue != instance.directInputHandlers[index].GetInvertAxis(axisName))
					Debug.LogError("DirectInputHandler and MenuInputHandler for abstract axis named " + axisName + " do not agree on invertedness");
			}

			return returnValue;
		}

		public static void SetInvertAxis(bool invert, string axisName, int playerNumber)
		{
			int index = playerNumber - 1;

			DataManager.SetWhetherAxisInverted(axisName + "_P" + playerNumber, invert);

			if (null != instance.menuInputHandlers)
			{
				switch (axisName)
				{
				case "Left_Vertical":
					instance.menuInputHandlers[index].SetInvertAxis(invert, "Left_Vertical_Up");
					instance.menuInputHandlers[index].SetInvertAxis(invert, "Left_Vertical_Down");
					break;
				case "Left_Horizontal":
					instance.menuInputHandlers[index].SetInvertAxis(invert, "Left_Horizontal_Left");
					instance.menuInputHandlers[index].SetInvertAxis(invert, "Left_Horizontal_Right");
					break;
				case "Right_Vertical":
					instance.menuInputHandlers[index].SetInvertAxis(invert, "Right_Vertical_Up");
					instance.menuInputHandlers[index].SetInvertAxis(invert, "Right_Vertical_Down");
					break;
				case "Right_Horizontal":
					instance.menuInputHandlers[index].SetInvertAxis(invert, "Right_Horizontal_Left");
					instance.menuInputHandlers[index].SetInvertAxis(invert, "Right_Horizontal_Right");
					break;
				}
			}

			if (null != instance.directInputHandlers)
			{
				instance.directInputHandlers[index].SetInvertAxis(invert, axisName);
			}
		}
	}
}
