using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(StructureFeatureStaticData), typeof(EntityFeatureStaticData))]
    public class HouseStaticData : FeatureStaticData<HousingFeature>
    {
        public int MaxOccupants;
		public float Quality;
	}
}
