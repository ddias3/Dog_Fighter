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
		
		public static void SendMessageToAction(Action action, string actionName, string actionMessage)
		{
			Action receivingAction = instance.m_actionDictionary[actionName] as Action;
			receivingAction.ReceiveMessage(action, actionMessage);
		}

		public static void SendMessage(Action action, string message)
		{
			string[] messageTokens = message.Split(' ');

			switch (messageTokens[0])
			{
			case "remove":
				switch (messageTokens[1])
				{
				case "from_action_list":
					instance.m_actionList.Remove(action);
					instance.m_actionDictionary.Remove(action.Name);
					break;
				}
				break;
			case "run":
			{
				Action newAction = ActionFactory.FindAction(messageTokens[1]);
				newAction.ActionStart();
				instance.m_actionList.AddLast(newAction);
				instance.m_actionDictionary.Add(newAction.Name, newAction);
			}
				break;
			default:
				Debug.LogError("ERROR IN GameManager.cs:SendMessage(Action, string) | Message, \"" + message + "\" does not have associated code; check spelling.");
				break;
			}
		}
	}
}