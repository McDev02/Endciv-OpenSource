using System;

namespace Endciv
{
	[Serializable]
	[RequireFeature(typeof(StructureFeatureStaticData), typeof(EntityFeatureStaticData))]
	public class FarmlandStaticData : FeatureStaticData<FarmlandFeature>
	{
		public float growSpeed = 1;
		public float nutritionFactor = 1;
        [StaticDataID("StaticData/SimpleEntities/Crops", typeof(CropFeatureStaticData))]
        public string[] CropIDs;
	}
}