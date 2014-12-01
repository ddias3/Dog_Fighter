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

			axisDictionary.Add("Left_Vertical_Down", new AxisMenuDataWrapper("Left_Vertical_P" + playerNumber, -1, true));
			axisDictionary.Add("Left_Vertical_Up", new AxisMenuDataWrapper("Left_Vertical_P" + playerNumber, 1, true));
			axisDictionary.Add("Left_Horizontal_Left", new AxisMenuDataWrapper("Left_Horizontal_P" + playerNumber, -1, true));
			axisDictionary.Add("Left_Horizontal_Right", new AxisMenuDataWrapper("Left_Horizontal_P" + playerNumber, 1, true));
			axisDictionary.Add("Right_Vertical_Down", new AxisMenuDataWrapper("Right_Vertical_P" + playerNumber, -1, true));
			axisDictionary.Add("Right_Vertical_Up", new AxisMenuDataWrapper("Right_Vertical_P" + playerNumber, 1, true));
			axisDictionary.Add("Right_Horizontal_Left", new AxisMenuDataWrapper("Right_Horizontal_P" + playerNumber, -1, true));
			axisDictionary.Add("Right_Horizontal_Right", new AxisMenuDataWrapper("Right_Horizontal_P" + playerNumber, 1, true));
//			axisDictionary.Add("Confirm_Button", new AxisDataWrapper("Confirm_P" + playerNumber, 1, false));
//			axisDictionary.Add("Cancel_Button", new AxisDataWrapper("Cancel_P" + playerNumber, 1, false));
//			axisDictionary.Add("Start_Button", new AxisDataWrapper("Start_P" + playerNumber, 1, false));

			keyDictionary.Add("Confirm_Button", new KeyDataWrapper("Confirm_Button_P" + playerNumber));
			keyDictionary.Add("Cancel_Button", new KeyDataWrapper("Cancel_Button_P" + playerNumber));
			keyDictionary.Add("Start_Button", new KeyDataWrapper("Start_Button_P" + playerNumber));
		}
		
		public int PlayerNumber
		{
			get { return playerNumber; }
		}

		public bool GetAxisKeyDown(string abstractedAxisName)
		{
			AxisMenuDataWrapper axisData = axisDictionary[abstractedAxisName] as AxisMenuDataWrapper;
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
				(axisDictionary[axisName] as AxisMenuDataWrapper).invertScalar = -1;
			else
				(axisDictionary[axisName] as AxisMenuDataWrapper).invertScalar = 1;
		}

		public bool GetInvertAxis(string axisName)
		{
			return ((axisDictionary[axisName] as AxisMenuDataWrapper).invertScalar < 0) ? true : false;
		}
	}
}