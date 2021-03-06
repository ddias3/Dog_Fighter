﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DogFighter
{
	public class SceneManager : MonoBehaviour
	{
		private LinkedList<Action> m_actionList;
		private HybridDictionary m_actionDictionary;

		public string firstMessageReceived;

		private static SceneManager instance;

		void Start()
		{
			m_actionList = new LinkedList<Action>();
			m_actionDictionary = new HybridDictionary();

			instance = this;

			SceneManager.SendMessage(null, firstMessageReceived);

			TimeStack.PushTimeLayer(new TimeLayerMenu());
			TimeStack.PushTimeLayer(new TimeLayerGameObjects());
		}

		void Update()
		{
			for (LinkedListNode<Action> currentNode = m_actionList.First; currentNode != null; currentNode = currentNode.Next)
				currentNode.Value.ActionUpdate();
		}

		void FixedUpdate()
		{
			for (LinkedListNode<Action> currentNode = m_actionList.First; currentNode != null; currentNode = currentNode.Next)
				currentNode.Value.ActionFixedUpdate();
		}

		void OnGUI()
		{
			for (LinkedListNode<Action> currentNode = m_actionList.First; currentNode != null; currentNode = currentNode.Next)
				currentNode.Value.ActionOnGUI();
		}
		
		public static void SendMessageToAction(Action actionSending, string actionName, string actionMessage)
		{
			Action receivingAction = instance.m_actionDictionary[actionName] as Action;
			receivingAction.ReceiveMessage(actionSending, actionMessage);
		}

		public static void SendMessage(Action actionSending, string message)
		{
			string[] messageTokens = message.Split(' ');

			switch (messageTokens[0])
			{
			case "remove":
				switch (messageTokens[1])
				{
				case "from_action_list":
					instance.m_actionList.Remove(actionSending);
					instance.m_actionDictionary.Remove(actionSending.Name);
					break;
				}
				break;
			case "run":
			{
				Action newAction = ActionFactory.FindAction(messageTokens[1]);

				instance.m_actionList.AddLast(newAction);
				instance.m_actionDictionary.Add(newAction.Name, newAction);

				newAction.ActionStart();
			}
				break;
			case "run_named":
			{
				Action newAction = ActionFactory.FindAction(messageTokens[1]);
				newAction.Name = messageTokens[2];

				instance.m_actionList.AddLast(newAction);
				instance.m_actionDictionary.Add(newAction.Name, newAction);

				newAction.ActionStart();
			}
				break;
			case "instantiate_named":
			{
				Action newAction = ActionFactory.CreateAction(messageTokens[1], messageTokens[2]);
				newAction.Name = messageTokens[2];

				instance.m_actionList.AddLast(newAction);
				instance.m_actionDictionary.Add(newAction.Name, newAction);

				newAction.ActionStart();
			}
				break;
			case "instantiate_named_from_prefab":
			{
				GameObject prefab = ((IPassPrefab)actionSending).GetPrefab(messageTokens[1]);
				Action newAction = ActionFactory.CreateActionFromPrefab(messageTokens[1], messageTokens[2], prefab);
				newAction.Name = messageTokens[2];

				instance.m_actionList.AddLast(newAction);
				instance.m_actionDictionary.Add(newAction.Name, newAction);

				newAction.ActionStart();
			}
				break;
			default:
				Debug.LogError("ERROR IN GameManager.cs:SendMessage(Action, string) | Message, \"" + message + "\" does not have associated code; check spelling.");
				break;
			}
		}
	}
}