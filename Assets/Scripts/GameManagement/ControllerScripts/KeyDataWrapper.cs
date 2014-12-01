using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public sealed class KeyDataWrapper
	{
		public string buttonName;
		public int previousCallValue;
		public KeyDataWrapper(string buttonName)
		{
			this.buttonName = buttonName;
			previousCallValue = 0;
		}
	}
}

