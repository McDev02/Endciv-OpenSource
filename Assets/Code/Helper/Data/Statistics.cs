using UnityEngine;
namespace Endciv
{
	[System.Serializable]
	public class SimpleAverage
	{
		public float average;
		public float minValue;
		public float maxValue;
		public int count;

		public SimpleAverage()
		{
			average = 0;
			minValue = float.MaxValue;
			maxValue = float.MinValue;
			count = 0;
		}

		public SimpleAverage(float value)
		{
			average = value;
			minValue = value;
			maxValue = value;
			count = 1;
		}

		public void AddValue(float value)
		{
			count++;
			average += value / count;

			if (value < minValue) minValue = value;
			if (value > maxValue) maxValue = value;
		}

		public void Clear()
		{
			average = 0;
			minValue = float.MaxValue;
			maxValue = float.MinValue;
			count = 0;
		}
	}

	[System.Serializable]
	public class SimpleAverageWeighted
	{
		public float average;
		public float minValue;
		public float maxValue;
		public float totalWeight;

		public SimpleAverageWeighted()
		{
			average = 0;
			minValue = float.MaxValue;
			maxValue = float.MinValue;
			totalWeight = 0;
		}

		public SimpleAverageWeighted(float value, float weight)
		{
			average = value;
			minValue = value;
			maxValue = value;
			totalWeight = weight;
		}

		public void AddValue(float value, float weight)
		{
			totalWeight += weight;
			average += value / totalWeight;

			if (value < minValue) minValue = value;
			if (value > maxValue) maxValue = value;
		}

		public void Clear()
		{
			average = 0;
			minValue = float.MaxValue;
			maxValue = float.MinValue;
			totalWeight = 0;
		}
	}
}