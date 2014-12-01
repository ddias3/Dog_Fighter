using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public sealed class AxisMenuDataWrapper
	{
		public string axisName;
		public int direction;
		public bool rawAxis;
		public float invertScalar;
		public float previousCallValue;
		public AxisMenuDataWrapper(string axisName, int direction, bool rawAxis)
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

