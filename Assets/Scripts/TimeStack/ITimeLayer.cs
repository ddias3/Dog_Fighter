using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public interface ITimeLayer
	{
		float AbsoluteTimeRule(float input);
		float DeltaTimeRule(float input);
		float FixedDeltaTimeRule(float input);

		void ScaleTimeLayer(float scalar);
	}
}