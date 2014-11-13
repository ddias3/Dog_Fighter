using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public abstract class Action : ScriptableObject
	{
		private float m_time;
		private string m_name;
		
		public string Name
		{
			set { m_name = value; }
			get { return m_name; }
		}
		
		public abstract void ActionStart();
		public abstract void ActionUpdate();
		public abstract void ActionFixedUpdate();
		public abstract void ActionOnGUI();
			
		public abstract void ReceiveMessage(Action action, string message);
	}
}