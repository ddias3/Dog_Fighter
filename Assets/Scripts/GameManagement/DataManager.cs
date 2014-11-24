using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class DataManager : ScriptableObject
	{
		private static DataManager instance = null;

		private int currentPlayers = 4;
		private bool[] playerActive;

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

			instance.playerActive = new bool[4];
			for (int n = 0; n < 4; ++n)
				if (n == 0)
					instance.playerActive[n] = true;
				else
					instance.playerActive[n] = false;
		}

		public static bool GetPlayerActive(int playerNumber)
		{
			return instance.playerActive[playerNumber - 1];
		}
		public static void SetPlayerActive(int playerNumber, bool active)
		{
			instance.playerActive[playerNumber - 1] = active;
		}

		private sealed class ControllerSetupDataWrapper
		{

		}
	}
}
