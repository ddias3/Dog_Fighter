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
	}
}
