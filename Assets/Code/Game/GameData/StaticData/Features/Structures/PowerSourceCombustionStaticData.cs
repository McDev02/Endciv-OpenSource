using System;

namespace Endciv
{
	[Serializable]
	public class PowerSourceCombustionStaticData : FeatureStaticData<PowerSourceCombustionFeature>
	{
		public float fuelCapacity;
		public float fuelCombustionRate;

		[StaticDataID("StaticData/SimpleEntities/Items")]
		public string[] fuelSources;

		[FeatureSelection]
		public string powerConsumers;
	}
}