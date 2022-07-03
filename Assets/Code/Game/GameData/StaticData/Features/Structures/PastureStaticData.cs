using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(StructureFeatureStaticData), typeof(EntityFeatureStaticData))]
    public class PastureStaticData : FeatureStaticData<PastureFeature>
    {
        public float growSpeed = 1;
        public float healthfactor = 1;
		public int maxCattleMass;
        public EntityStaticData[] Cattle;
		public float maxNutrition;
		public float maxWater;
		[StaticDataID("StaticData/SimpleEntities/Items/Food")]
		public string[] food;		
    }
}
