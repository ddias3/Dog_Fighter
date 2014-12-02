using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class PlayerShipPosition : ScriptableObject
	{
		public int shipNumber = 0;
		public bool active = true;
		public Vector3 position;
	}
}
