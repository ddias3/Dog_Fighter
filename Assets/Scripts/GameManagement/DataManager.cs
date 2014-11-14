using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class DataManager : ScriptableObject
	{
		private static DataManager instance = null;

		private int currentPlayers = 2;

		public static int GetNumberPlayers()
		{
			if (null == instance)
				InitializeDataManager();

			return instance.currentPlayers;
		}

		public static void SetNumberPlayers(int newNumberPlayers)
		{
			if (null == instance)
				InitializeDataManager();

			if (newNumberPlayers < 2)
				newNumberPlayers = 2;
			else if (newNumberPlayers > 4)
				newNumberPlayers = 4;

			instance.currentPlayers = newNumberPlayers;
			PlayerPrefs.SetInt("CurrentPlayers", newNumberPlayers);
		}

		private static void InitializeDataManager()
		{
			instance = ScriptableObject.CreateInstance<DataManager>();
			DontDestroyOnLoad(instance);
			instance.currentPlayers = PlayerPrefs.GetInt("CurrentPlayers", 4);
		}
	}
}
