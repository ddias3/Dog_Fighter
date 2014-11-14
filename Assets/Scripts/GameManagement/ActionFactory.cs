using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class ActionFactory
	{
		public static Action FindAction(string actionName)
		{
			Action action = null;

			switch (actionName)
			{
			case "MenuAction":
				action = MonoBehaviour.FindObjectOfType<MenuAction>();
				break;
			case "SingleShipAction":
				action = MonoBehaviour.FindObjectOfType<SingleShipControlAction>();
				break;
			default:
				Debug.LogError(actionName + " does not exist");
				throw new MissingComponentException();
			}

			action.Name = actionName;
			return action;
		}
	}
}
