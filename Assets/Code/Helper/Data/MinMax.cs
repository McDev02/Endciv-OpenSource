using UnityEngine;
namespace Endciv
{
	[System.Serializable]
	public struct MinMaxi
	{
		public int min;
		public int max;

		public float Average { get { return (min + max) / 2f; } }
		public MinMaxi(int min, int max)
		{
			this.min = min;
			this.max = max;
		}
	}
	[System.Serializable]
	public struct MinMax
	{
		public float min;
		public float max;

		public float Average { get { return (min + max) / 2f; } }
		public MinMax(float min, float max)
		{
			this.min = min;
			this.max = max;
		}
	}
}