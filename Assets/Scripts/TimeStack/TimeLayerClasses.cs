using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public class TimeLayerMenu : ITimeLayer
	{
		public float AbsoluteTimeRule(float input)
		{
			return input;
		}
		
		public float DeltaTimeRule(float input)
		{
			return input;
		}

		public float FixedDeltaTimeRule(float input)
		{
			return input;
		}

		public void ScaleTimeLayer(float scalar)
		{
			// do nothing
		}
	}

	public class TimeLayerGameObjects : ITimeLayer
	{
		private float scalar = 1f;

		public float AbsoluteTimeRule(float input)
		{
			return scalar * input;
		}
		
		public float DeltaTimeRule(float input)
		{
			return scalar * input;
		}

		public float FixedDeltaTimeRule(float input)
		{
			return scalar * input;
		}

		public void ScaleTimeLayer(float scalar)
		{
			this.scalar = scalar;
		}
	}
}