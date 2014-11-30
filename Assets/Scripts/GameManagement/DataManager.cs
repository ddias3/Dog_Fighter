using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class DataManager : ScriptableObject
	{
		private static DataManager instance = null;

		private int currentPlayers = 4;
		private bool[] playerActive;
		private int[] controllerSetups;
		private int gameMode;
		private int gameLengthMinutes;
		private int mapId;
		private int[] invertPitches;
		private int[] invertYaws;
		private int[] invertRolls;

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

			if (newNumberPlayers < 1)//2)
				newNumberPlayers = 1;//2;
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

			instance.controllerSetups = new int[4];
			for (int n = 0; n < 4; ++n)
				instance.controllerSetups[n] = PlayerPrefs.GetInt("ControllerSetup_P" + (n + 1), 0);

			instance.invertPitches = new int[4];
			for (int n = 0; n < 4; ++n)
				instance.invertPitches[n] = PlayerPrefs.GetInt("InvertPitch_P" + (n + 1), 0);

			instance.invertYaws = new int[4];
			for (int n = 0; n < 4; ++n)
				instance.invertYaws[n] = PlayerPrefs.GetInt("InvertYaw_P" + (n + 1), 0);

			instance.invertRolls = new int[4];
			for (int n = 0; n < 4; ++n)
				instance.invertRolls[n] = PlayerPrefs.GetInt("InvertRoll_P" + (n + 1), 0);

			instance.gameMode = 0;
			instance.gameLengthMinutes = PlayerPrefs.GetInt("GameLengthMinutes", 5);
			instance.mapId = 0;
		}

		public static bool GetPlayerActive(int playerNumber)
		{
			if (null == instance)
				InitializeDataManager();

			return instance.playerActive[playerNumber - 1];
		}
		public static void SetPlayerActive(int playerNumber, bool active)
		{
			if (null == instance)
				InitializeDataManager();

			instance.playerActive[playerNumber - 1] = active;
		}

		public static int GetControllerSetup(int playerNumber)
		{
			if (null == instance)
				InitializeDataManager();

			return instance.controllerSetups[playerNumber - 1];
		}
		public static void SetControllerSetup(int playerNumber, int newControllerSetup)
		{
			if (null == instance)
				InitializeDataManager();

			instance.controllerSetups[playerNumber - 1] = newControllerSetup;
			PlayerPrefs.SetInt("ControllerSetup_P" + playerNumber, newControllerSetup);
		}

		public static int GetInvertPitch(int playerNumber)
		{
			if (null == instance)
				InitializeDataManager();
			
			return instance.invertPitches[playerNumber - 1];
		}
		public static void SetInvertPitch(int playerNumber, int newInvertPitch)
		{
			if (null == instance)
				InitializeDataManager();
			
			instance.invertPitches[playerNumber - 1] = newInvertPitch;
			PlayerPrefs.SetInt("InvertPitch_P" + playerNumber, newInvertPitch);
		}

		public static int GetInvertYaw(int playerNumber)
		{
			if (null == instance)
				InitializeDataManager();
			
			return instance.invertYaws[playerNumber - 1];
		}
		public static void SetInvertYaw(int playerNumber, int newInvertYaw)
		{
			if (null == instance)
				InitializeDataManager();
			
			instance.invertYaws[playerNumber - 1] = newInvertYaw;
			PlayerPrefs.SetInt("InvertYaw_P" + playerNumber, newInvertYaw);
		}

		public static int GetInvertRoll(int playerNumber)
		{
			if (null == instance)
				InitializeDataManager();
			
			return instance.invertRolls[playerNumber - 1];
		}
		public static void SetInvertRoll(int playerNumber, int newInvertRoll)
		{
			if (null == instance)
				InitializeDataManager();
			
			instance.invertRolls[playerNumber - 1] = newInvertRoll;
			PlayerPrefs.SetInt("InvertRoll_P" + playerNumber, newInvertRoll);
		}

		public static bool GetWhetherAxisInverted(string axisName)
		{
			if (null == instance)
				InitializeDataManager();

			return PlayerPrefs.GetInt(axisName, 0) == 1;
		}

		public static void SetWhetherAxisInverted(string axisName, bool invert)
		{
			if (null == instance)
				InitializeDataManager();

			PlayerPrefs.SetInt(axisName, (invert) ? 1 : 0);
		}

		public static int GetGameMode()
		{
			if (null == instance)
				InitializeDataManager();

			return instance.gameMode;
		}

		public static int GetMapId()
		{
			if (null == instance)
				InitializeDataManager();

			return instance.mapId;
		}

		public static int GetGameLengthMinutes()
		{
			if (null == instance)
				InitializeDataManager();

			return instance.gameLengthMinutes;
		}

		public static void SetGameLengthMinutes(int newLength)
		{
			if (null == instance)
				InitializeDataManager();

			instance.gameLengthMinutes = newLength;
			PlayerPrefs.SetInt("GameLengthMinutes", newLength);
		}
	}
}
