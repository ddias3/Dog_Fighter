using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class ControllerDirectInputHandler : ScriptableObject
	{
		private int playerNumber;

		public int PlayerNumber
		{
			get { return playerNumber; }
			set
			{
				playerNumber = value;
				if (playerNumber > 4)
					playerNumber = 4;
				else if (playerNumber < 1)
					playerNumber = 1;
			}
		}

		public void Initialize(int playerNumber)
		{

		}
	}
}
