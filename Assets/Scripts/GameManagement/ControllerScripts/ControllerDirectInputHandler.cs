using UnityEngine;
using System.Collections.Specialized;

namespace DogFighter
{
	public class ControllerDirectInputHandler : ScriptableObject
	{
		private int playerNumber;

		private ListDictionary axisDictionary;
		private ListDictionary keyDictionary;

		public void Initialize(int playerNumber)
		{
			this.playerNumber = playerNumber;
			
			axisDictionary = new ListDictionary();
			keyDictionary = new ListDictionary();

			axisDictionary.Add("Left_Vertical", new AxisDataWrapper("Left_Vertical_P" + playerNumber,       false, true));
			axisDictionary.Add("Left_Horizontal", new AxisDataWrapper("Left_Horizontal_P" + playerNumber,   false, true));
			axisDictionary.Add("Right_Vertical", new AxisDataWrapper("Right_Vertical_P" + playerNumber,     false, true));
			axisDictionary.Add("Right_Horizontal", new AxisDataWrapper("Right_Horizontal_P" + playerNumber, false, true));
			axisDictionary.Add("Left_Trigger", new AxisDataWrapper("Left_Trigger_P" + playerNumber,         true,  true));
			axisDictionary.Add("Right_Trigger", new AxisDataWrapper("Right_Trigger_P" + playerNumber,       true,  true));

			keyDictionary.Add("Left_Bumper", new KeyDataWrapper("Left_Bumper_P" + playerNumber));
			keyDictionary.Add("Right_Bumper", new KeyDataWrapper("Right_Bumper_P" + playerNumber));
			keyDictionary.Add("Back_Button", new KeyDataWrapper("Back_Button_P" + playerNumber));
		}

		public float GetAxis(string abstractedAxisName)
		{
			AxisDataWrapper axisData = axisDictionary[abstractedAxisName] as AxisDataWrapper;

			float returnValue;
			if (axisData.rawAxis)
				returnValue = Input.GetAxisRaw(axisData.axisName);
			else
				returnValue = Input.GetAxis(axisData.axisName);
			returnValue *= axisData.invertScalar;
			if (axisData.absoluteValue)
				returnValue = Mathf.Abs(returnValue);

			return returnValue;
		}

		public bool GetButtonDown(string abstractedKeyName)
		{
			KeyDataWrapper keyData = keyDictionary[abstractedKeyName] as KeyDataWrapper;
			bool returnValue = false;
			
			if (Input.GetButtonDown(keyData.buttonName))
				returnValue = true;

			return returnValue;
		}

		public bool GetButton(string abstractedKeyName)
		{
			KeyDataWrapper keyData = keyDictionary[abstractedKeyName] as KeyDataWrapper;
			bool returnValue = false;
			
			if (Input.GetButton(keyData.buttonName))
				returnValue = true;
			
			return returnValue;
		}

		public bool GetButtonUp(string abstractedKeyName)
		{
			KeyDataWrapper keyData = keyDictionary[abstractedKeyName] as KeyDataWrapper;
			bool returnValue = false;
			
			if (Input.GetButtonUp(keyData.buttonName))
				returnValue = true;
			
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
	}
}
