using UnityEngine;

namespace Endciv
{
	public class EntityNeed
	{
		public float weight;
		private float value;

		public float Value
		{
			get { return value; }
			set
			{
				this.value = value;
				Mood = CivMath.Clamp((value - minValue) * divisor - 1, -1, 1);
			}
		}

		public float minValue { get; private set; }
		public float maxValue { get; private set; }
		private float divisor;
		internal string name;

		/// <summary>
		/// Ranges from -1 to 1
		/// </summary>
		public float Mood { get; private set; }


		public EntityNeed(string name, MinMax minMax, float weight)
		{
			this.name = name;
			this.weight = weight;
			value = 1;
			minValue = minMax.min;
			maxValue = minMax.max;
			if (Mathf.Approximately(maxValue - minValue, 0.0f))
			{
				divisor = 1;
				Debug.LogError("Wrong MinMax values for EntityNeed: " + name);
			}
			else
				divisor = 2f / maxValue - minValue;

		}
		public EntityNeed(string name, MinMax minMax, float weight, float startingValue)
		{
			this.name = name;
			this.weight = weight;
			value = startingValue;
			minValue = minMax.min;
			maxValue = minMax.max;
			if (Mathf.Approximately(maxValue - minValue, 0.0f))
			{
				divisor = 1;
				Debug.LogError("Wrong MinMax values for EntityNeed: " + name);
			}
			else
				divisor = 2f / maxValue - minValue;
		}

		public void SetMinMax(float min, float max)
		{
			minValue = min;
			maxValue = max;
			if (Mathf.Approximately(maxValue - minValue, 0.0f))
			{
				divisor = 1;
				Debug.LogError("Wrong MinMax values for EntityNeed: " + name);
			}
			else
				divisor = 2f / maxValue - minValue;
		}
	}
}
