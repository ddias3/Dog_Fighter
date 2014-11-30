using UnityEngine;
using System.Collections;
using System.Collections.Specialized;

namespace DogFighter
{
	public class ControllerMenuInputHandler : ScriptableObject
	{
		private int playerNumber;
		private ListDictionary axisDictionary;
		private ListDictionary keyDictionary;

		public void Initialize(int playerNumber)
		{
			if (playerNumber > 4)
				playerNumber = 4;
			else if (playerNumber < 1)
				playerNumber = 1;

			this.playerNumber = playerNumber;

			axisDictionary = new ListDictionary();
			keyDictionary = new ListDictionary();

			axisDictionary.Add("Left_Vertical_Down", new AxisDataWrapper("Left_Vertical_P" + playerNumber, -1, true));
			axisDictionary.Add("Left_Vertical_Up", new AxisDataWrapper("Left_Vertical_P" + playerNumber, 1, true));
			axisDictionary.Add("Left_Horizontal_Left", new AxisDataWrapper("Left_Horizontal_P" + playerNumber, -1, true));
			axisDictionary.Add("Left_Horizontal_Right", new AxisDataWrapper("Left_Horizontal_P" + playerNumber, 1, true));
//			axisDictionary.Add("Confirm_Button", new AxisDataWrapper("Confirm_P" + playerNumber, 1, false));
//			axisDictionary.Add("Cancel_Button", new AxisDataWrapper("Cancel_P" + playerNumber, 1, false));
//			axisDictionary.Add("Start_Button", new AxisDataWrapper("Start_P" + playerNumber, 1, false));

			keyDictionary.Add("Confirm_Button", new KeyDataWrapper("Confirm_P" + playerNumber));
			keyDictionary.Add("Cancel_Button", new KeyDataWrapper("Cancel_P" + playerNumber));
			keyDictionary.Add("Start_Button", new KeyDataWrapper("Start_P" + playerNumber));
		}
		
		public int PlayerNumber
		{
			get { return playerNumber; }
		}

		public bool GetAxisKeyDown(string abstractedAxisName)
		{
			AxisDataWrapper axisData = axisDictionary[abstractedAxisName] as AxisDataWrapper;
			bool returnValue = false;

			if (axisData.rawAxis)
			{
				if ((axisData.direction > 0 && axisData.invertScalar > 0) ||
				    (axisData.direction < 0 && axisData.invertScalar < 0))
				{
					float currentCallValue = Input.GetAxisRaw(axisData.axisName);
					if (currentCallValue > 0.5f && axisData.previousCallValue < 0.5f)
						returnValue = true;
					axisData.previousCallValue = currentCallValue;
				}
				else if ((axisData.direction < 0 && axisData.invertScalar > 0) ||
				         (axisData.direction > 0 && axisData.invertScalar < 0))
				{
					float currentCallValue = Input.GetAxisRaw(axisData.axisName);
					if (currentCallValue < -0.5f && axisData.previousCallValue > -0.5f)
						returnValue = true;
					axisData.previousCallValue = currentCallValue;
				}
			}
			else
			{
				if (axisData.direction > 0)
				{
					float currentCallValue = Input.GetAxis(axisData.axisName);
					if (currentCallValue > 0.1f && axisData.previousCallValue < 0.1f)
						returnValue = true;
					axisData.previousCallValue = currentCallValue;
				}
				else if (axisData.direction < 0)
				{
					float currentCallValue = Input.GetAxis(axisData.axisName);
					if (currentCallValue < -0.1f && axisData.previousCallValue > -0.1f)
						returnValue = true;
					axisData.previousCallValue = currentCallValue;
				}
			}

			return returnValue;
		}

		public bool GetButtonDown(string abstractedKeyName)
		{
			KeyDataWrapper keyData = keyDictionary[abstractedKeyName] as KeyDataWrapper;
			bool returnValue = false;

			if (Input.GetButtonDown(keyData.buttonName))
			{
				returnValue = true;
			}
			return returnValue;
		}

		public void SetInvertAxis(bool invert, string axisName)
		{
			if (invert)
				(axisDictionary[axisName] as AxisDataWrapper).invertScalar = -1;
			else
				(axisDictionary[axisName] as AxisDataWrapper).invertScalar = 1;
		}

		public bool GetInvertAxis(string axisName)
		{
			return ((axisDictionary[axisName] as AxisDataWrapper).invertScalar < 0) ? true : false;
		}

		private sealed class KeyDataWrapper
		{
			public string buttonName;
			public int previousCallValue;
			public KeyDataWrapper(string buttonName)
			{
				this.buttonName = buttonName;
				previousCallValue = 0;
			}
		}

		private sealed class AxisDataWrapper
		{
			public string axisName;
			public int direction;
			public bool rawAxis;
			public float invertScalar;
			public float previousCallValue;
			public AxisDataWrapper(string axisName, int direction, bool rawAxis)
			{
				this.axisName = axisName;
				this.direction = direction;
				this.rawAxis = rawAxis;
				this.previousCallValue = 0f;
				if (DataManager.GetWhetherAxisInverted(axisName))
					this.invertScalar = -1f;
				else
					this.invertScalar = 1f;
			}
		}
	}
}