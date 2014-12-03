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
			case "DeathMatchAction":
				action = MonoBehaviour.FindObjectOfType<DeathMatchAction>();
				break;
			case "OptionsMenuAction":
				action = MonoBehaviour.FindObjectOfType<OptionsMenuAction>();
				break;
			case "JoinMenuAction":
				action = MonoBehaviour.FindObjectOfType<JoinMenuAction>();
				break;
			case "SetupGameMenuAction":
				action = MonoBehaviour.FindObjectOfType<SetupGameMenuAction>();
				break;
            case "SoundPlayerAction":
                action = MonoBehaviour.FindObjectOfType<SoundPlayerAction>();
                break;
			default:
				Debug.LogError(actionName + " does not exist");
				throw new MissingComponentException();
			}

			action.Name = actionName;
			return action;
		}

		public static Action CreateAction(string actionName, string gameObjectName)
		{
			Action action = null;
			
			switch (actionName)
			{
			case "SingleShipControlAction":
			{
				GameObject newGameObject = new GameObject(gameObjectName);
				action = newGameObject.AddComponent<SingleShipControlAction>();
			}
				break;
			default:
				Debug.LogError(actionName + " does not exist");
				throw new MissingComponentException();
			}
			
			action.Name = actionName;
			return action;
		}

		public static Action CreateActionFromPrefab(string actionName, string gameObjectName, GameObject prefab)
		{
			Action action = null;
			
			switch (actionName)
			{
			case "SingleShipControlAction":
			{
				GameObject newGameObject = GameObject.Instantiate(prefab) as GameObject;
				newGameObject.name = gameObjectName;
				action = newGameObject.GetComponent<SingleShipControlAction>();
			}
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
