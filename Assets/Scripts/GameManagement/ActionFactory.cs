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
			case "MainMenuAction":
				action = MonoBehaviour.FindObjectOfType<MainMenuAction>();
				break;
			case "SelectNumberMenuAction":
				action = MonoBehaviour.FindObjectOfType<SelectNumberMenuAction>();
				break;
			case "SingleShipControlAction":
				action = MonoBehaviour.FindObjectOfType<SingleShipControlAction>();
				break;
			case "DeathMatchAction":
				action = MonoBehaviour.FindObjectOfType<DeathMatchAction>();
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
