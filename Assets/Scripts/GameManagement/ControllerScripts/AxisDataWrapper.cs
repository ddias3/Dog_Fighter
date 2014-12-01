using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public sealed class AxisDataWrapper
	{
		public string axisName;
		public bool rawAxis;
		public bool absoluteValue;
		public float invertScalar;
		public AxisDataWrapper(string axisName, bool absoluteValue, bool rawAxis)
		{
			this.axisName = axisName;
			this.rawAxis = rawAxis;
			this.absoluteValue = absoluteValue;
			if (DataManager.GetWhetherAxisInverted(axisName))
				this.invertScalar = -1f;
			else
				this.invertScalar = 1f;
		}
	}
}

