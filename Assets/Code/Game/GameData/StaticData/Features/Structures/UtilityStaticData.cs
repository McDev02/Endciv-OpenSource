using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(StructureFeatureStaticData), typeof(EntityFeatureStaticData))]
    public class UtilityStaticData : FeatureStaticData<UtilityFeature>
    {
        public int MaxOccupants;
        public EUtilityType type;

		public float waterConsumption;
    }
}
