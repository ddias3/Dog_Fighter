using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public sealed class MenuCursorDataWrapper
	{
		public int playerNumber;
		public int menuItemSelected;
		public MenuCursorDataWrapper(int playerNumber, int menuItemSelected)
		{
			this.playerNumber = playerNumber;
			this.menuItemSelected = menuItemSelected;
		}
	}
}