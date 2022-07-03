using System;

namespace Endciv
{
	[Serializable]
	public class PowerSourceElectricityStaticData : FeatureStaticData<PowerSourceElectricityFeature>
	{
		[UnityEngine.Range(1, 5)]
		public int Priority;
		public float Consumption;

		[FeatureSelection]
		public string powerConsumers;
	}
}
