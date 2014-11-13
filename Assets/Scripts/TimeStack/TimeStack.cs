using UnityEngine;
using System.Collections;

namespace DogFighter
{
	public static class TimeStack
	{
		public static ITimeLayer[] timeLayers = null;
		public static float absoluteTime = Time.time;
		public static float deltaTime = Time.deltaTime;

		public static float GetAbsoluteTimeFromLayer(int layer)
		{
			if (layer < timeLayers.Length)
			{
				float output = absoluteTime;
				for (int n = 0; n <= layer; ++n)
				{
					output = timeLayers[n].AbsoluteTimeRule(output);
				}
				return output;
			}
			else
				return -1f;
		}

		public static float GetDeltaTimeFromLayer(int layer)
		{
			if (layer < timeLayers.Length)
			{
				float output = deltaTime;
				for (int n = 0; n <= layer; ++n)
				{
					output = timeLayers[n].DeltaTimeRule(output);
				}
				return output;
			}
			else
				return -1f;
		}

		public static int SetTimeLayer(int layer, ITimeLayer timeLayer)
		{
			if (layer >= timeLayers.Length)
				return -1;
			else
			{
				timeLayers[layer] = timeLayer;
				return 0;
			}
		}

		public static void ScaleTimeLayer(int layer, float scalar)
		{
			if (layer >= timeLayers.Length)
				return;
			else
			{
				timeLayers[layer].ScaleTimeLayer(scalar);
			}
		}

		public static void PushTimeLayer(ITimeLayer newTimeLayer)
		{
			if (null == timeLayers)
				timeLayers = new ITimeLayer[0];
			ITimeLayer[] temp = timeLayers;
			timeLayers = new ITimeLayer[temp.Length + 1];
			for (int n = 0; n < timeLayers.Length - 1; ++n)
				timeLayers[n] = temp[n];
			timeLayers[timeLayers.Length - 1] = newTimeLayer;
		}

		public static ITimeLayer PopTimeLayer()
		{
			if (null != timeLayers && 0 == timeLayers.Length)
				return null;
			ITimeLayer[] temp = timeLayers;
			timeLayers = new ITimeLayer[temp.Length - 1];
			for (int n = 0; n < timeLayers.Length; ++n)
				timeLayers[n] = temp[n];
			return temp[temp.Length - 1];
		}
	}
}