using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(StructureFeatureStaticData), typeof(EntityFeatureStaticData))]
    public class ProductionStaticData : FeatureStaticData<ProductionFeature>
    {
        //Type: Which type this facility is, if labor is required or not
        public bool NeedsLabour;
        public bool NeedsEnergy;
		[StaticDataID("StaticData/SimpleEntities/Recipes/", typeof(RecipeFeatureStaticData))]
		public string[] Recipes;
        public float SpeedFactor;
        public int ProductionLines;
	}
}
