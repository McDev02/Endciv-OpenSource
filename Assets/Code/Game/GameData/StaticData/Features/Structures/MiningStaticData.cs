using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(StructureFeatureStaticData), typeof(EntityFeatureStaticData))]
    public class MiningStaticData : FeatureStaticData<MiningFeature>
    {
        public float value;
		[Tooltip("Radius in Tiles")]
        public float radius;
		public EMiningType miningType;
	}
}
