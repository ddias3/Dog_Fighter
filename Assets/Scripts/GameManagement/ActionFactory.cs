using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class ActionFactory
	{
		public static Action CreateAction(string actionName)
		{
			Action action = null;

			switch (actionName)
			{
			case "MenuAction":
				action = ScriptableObject.CreateInstance<MenuAction>();
				break;
			case "SingleShipAction":
				action = ScriptableObject.CreateInstance<SingleShipAction>();
				break;
			default:
				throw new MissingComponentException();
			}

			return action;
		}
	}
}
