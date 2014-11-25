using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public interface IPassPrefab
	{
		GameObject GetPrefab(string prefabName);
	}
}

