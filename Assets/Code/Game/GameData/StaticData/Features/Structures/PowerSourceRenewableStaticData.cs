using System;
using System.Collections.Generic;

namespace Endciv
{
	[Serializable]
	public class PowerSourceRenewableStaticData : FeatureStaticData<PowerSourceRenewableFeature>
	{
		public PowerSourceSystem.EPowerType powerType;

        [FeatureSelection]
        public string powerConsumers;
	}
}